using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using CPMPayAgentService;
using CPMPayAgentService.Utility;
using System.Configuration;
using Bgs.Duet.Exchange.Client.Proxy.StaffCard;
using Bgs.Duet.Exchange.Client.Proxy;
using Bgs.Duet.Exchange.ApplicationServer.Common.Classify;
using System.Threading;

namespace DUETCPMConsole
{
    public class OperationPerformer
    {
        private static OperationPerformer _instance;
        private CPMPayAgentService.LoggingLayer.OperationPersister dataCtx;
        private Dictionary<string, object> resultsTable;

        public static OperationPerformer Instance()
        {
            if (_instance == null)
                _instance = new OperationPerformer();
            return _instance;
        }

        private OperationPerformer()
        {
            dataCtx = CPMPayAgentService.LoggingLayer.OperationPersister.Instance();
            resultsTable = new Dictionary<string, object>();
        }

        public string Initialize(IErrorReporter errReporter)
        {
            string status = " status empty";
            try
            {
                string host = CPMPayAgentService.Properties.Settings.Default.DUETServerIPAddress ;//ConfigurationManager.AppSettings["DUETServerIPAddress"].ToString();
                int port = CPMPayAgentService.Properties.Settings.Default.DUETServerPortNumber; //Convert.ToInt32(ConfigurationManager.AppSettings["DUETServerPortNumber"].ToString());
                int channel = CPMPayAgentService.Properties.Settings.Default.ReaderChannel; //Convert.ToInt32(ConfigurationManager.AppSettings["ReaderChannel"].ToString());
                string MKPassword = CPMPayAgentService.Properties.Settings.Default.MKPassword; // ConfigurationManager.AppSettings["MKPassword"].ToString();
                //status = "host:" + host + " port:" + port + " chanel:" + channel + " MKPassword:" + MKPassword;
                Card staff_card = new Card();
                staff_card.Open(channel);
                staff_card.PresentPassword(MKPassword);//"2801");
                //status = status + " staffCard expiryDate:"+ staff_card.StaffCard.ExpiryDate.ToString();
                Registration reg = new Registration();
                string conn_id = string.Empty;

                UserInfo ui = reg.OpenSession(staff_card, host, port);
                // снятие питания с МК, освобождение ридера:
                staff_card.Close();

                // проверка - успешно ли установлена сессия:
                if (reg.RegistrationStatus != RegistrationResultItem.RegistrationSuccefully)
                {
                    //throw new Exception("NOT_CONNECTED");
                    errReporter.reportError("RegistrationFailed -  " + status);
                    return "RegistrationFailed -  " + status;
                }
                else
                {
                    errReporter.reportError("Registration Successful.");
                    return "RegistrationSuccessful. " + status;
                }
            }
            catch(Exception e)
            {
                errReporter.reportError("Exception occurred " + e.Message + " "+ e.StackTrace);
            }
            return "RegistrationUnknown "+status;
        }

        public operationresult PerformDuetOperation(string sessionId, int clientAccountId, int ctrgAccountId, int ctrgClientAccountId,
            int contractBindId, string externalDocDate, string externalDocNumber, decimal feeAmount, int functionType,
            string paymentDetails, decimal transactAmount, string checksum, string curDate, string IP, IErrorReporter errRep)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.PerformDuetOperation()");
            CPMPayAgentService.LoggingLayer.Operation oper=dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            int numOfSucOhpers = (exists && oper.Result==7)?1:0;
            if (numOfSucOhpers > 0)
            {
                operationresult result = new operationresult(IntegerConstants.DUPLICATE_SESSION_ID, "такой sessionId уже есть", resultLog.getLog());
                return result;
            }

            if (!exists)
            {
                oper = new CPMPayAgentService.LoggingLayer.Operation();
                oper.SessionID = sessionId;
                oper.ClientAccountID = clientAccountId;
                oper.ContrAgentAccountID = ctrgAccountId;
                oper.ContrAgentClientAccountID = ctrgClientAccountId;
                oper.ContractBindID = contractBindId;
                oper.ExternalDocNumber = externalDocNumber;
                oper.feeAmount = feeAmount;
                oper.FunctionType = functionType;
                oper.PaymentDetails = paymentDetails;
                oper.TransactAmount = transactAmount;
            }
            oper.CurrentDate = DateTime.Now;
            DateTime externalDate = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            try
            {
                externalDate = DateTime.ParseExact(externalDocDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                currentDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.CurrentDate = currentDate;
                oper.ExternalDocDate = externalDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.PerformDuetOperation(). DateTime FormatException: " + fe.Message);
                operationresult result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                //oper.Result = IntegerConstants.WRONG_PARAMS;
                //oper.ResultNote = "не указаны необходимые параметры запроса";
                //oper.ResultLog = resultLog.getLog();
                //dataCtx.Operations.InsertOnSubmit(oper);
                //dataCtx.SubmitChanges();
                oper = null;
                errRep.reportError("Wrong parameters "+IP +" "+ sessionId+" "+/*clientAccountId+" "+ctrgAccountId+" "+ctrgClientAccountId
                    +" "+contractBindId+" "+externalDocDate+" "+externalDocNumber+" "+feeAmount+" "+functionType
                    +" "+paymentDetails+" "+transactAmount+" "+*/externalDocDate+" "+checksum+" "+curDate);
                return result;
            }
            
            

            string calculatedMD5 = HashUtility.CalculateMD5(sessionId, clientAccountId, ctrgAccountId, ctrgClientAccountId, contractBindId, externalDocDate, externalDocNumber, feeAmount, functionType, paymentDetails, transactAmount, curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                operationresult result =new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
                //oper.Result = IntegerConstants.WRONG_CHECKSUM;
                //oper.ResultNote = "неправильная контрольная сумма";
                //oper.ResultLog = resultLog.getLog();
                //dataCtx.Operations.InsertOnSubmit(oper);
                //dataCtx.SubmitChanges();
                oper = null;
                errRep.reportError("Wrong checksum " + IP +" "+ sessionId + " " +/* clientAccountId + " " + ctrgAccountId + " " + ctrgClientAccountId
                    + " " + contractBindId + " " + externalDocDate + " " + externalDocNumber + " " + feeAmount + " " + functionType
                    + " " + paymentDetails + " " + transactAmount + " " + */checksum + " " + curDate);
                return result;
            }
            
            DateTime now = DateTime.Now;
            CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError("Now: "+now.ToString()+" curdate: "+curDate.ToString()+
               (currentDate.CompareTo(now.AddMinutes(15)) < 0) + " " + (currentDate.CompareTo(now.AddMinutes(-15)) > 0));
            if( !(currentDate.CompareTo(now.AddMinutes(15)) < 0 && (currentDate.CompareTo(now.AddMinutes(-15)) > 0)) ){
                operationresult result =new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", resultLog.getLog());
                //oper.Result = IntegerConstants.WRONG_TIME;
                //oper.ResultNote = "неправильное время";
                //oper.ResultLog = resultLog.getLog();
                //dataCtx.Operations.InsertOnSubmit(oper);
                //dataCtx.SubmitChanges();
                oper = null;
                errRep.reportError("Wrong time " + IP + sessionId + " " + clientAccountId + " " + ctrgAccountId + " " + ctrgClientAccountId
                    + " " + contractBindId + " " + externalDocDate + " " + externalDocNumber + " " + feeAmount + " " + functionType
                    + " " + paymentDetails + " " + transactAmount + " " + checksum + " " + curDate);
                return result;
            }

            string host = CPMPayAgentService.Properties.Settings.Default.DUETServerIPAddress;//ConfigurationManager.AppSettings["DUETServerIPAddress"].ToString();
            int port = CPMPayAgentService.Properties.Settings.Default.DUETServerPortNumber; //Convert.ToInt32(ConfigurationManager.AppSettings["DUETServerPortNumber"].ToString());
            int channel = CPMPayAgentService.Properties.Settings.Default.ReaderChannel; //Convert.ToInt32(ConfigurationManager.AppSettings["ReaderChannel"].ToString());
            string MKPassword = CPMPayAgentService.Properties.Settings.Default.MKPassword;
            
            ////save data to DB before making DUET transaction
            oper.Result = IntegerConstants.IN_PROCESS;
            oper.ResultNote = "В процессе";
            oper.ResultLog = resultLog.getLog();
            if(!exists)
                dataCtx.InsertNew(oper);
            else
                dataCtx.UpdateExisting(oper);
            //dataCtx.SubmitChanges();

            oper = dataCtx.getOperation(sessionId);

            ////Thread.Sleep(60 * 1000);
            //// start connecting to DUET server
            Card staff_card = new Card();
            try
            {
                staff_card.Open(channel);
                staff_card.PresentPassword(MKPassword);//"2801");
            }
            catch (Exception ex)
            {
                resultLog.AddToLog(" Error: " + ex.Message);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.WRONG_PASS;
                oper.ResultNote = "Ошибка пароля карты";
                oper.ResultLog = resultLog.getLog();
                //oper.DuetOperCode = res.ErrorCode;
                //oper.DuetOperNote = res.ErrorMessage;
                dataCtx.UpdateExisting(oper);
                return resultS;
                
            }

            Registration reg = new Registration();
            string conn_id = string.Empty;

            UserInfo ui = reg.OpenSession(staff_card, host, port);
            // снятие питания с МК, освобождение ридера:
            staff_card.Close();

            // проверка - успешно ли установлена сессия:
            if (reg.RegistrationStatus != RegistrationResultItem.RegistrationSuccefully)
            {
                //throw new Exception("NOT_CONNECTED");
                resultLog.AddToLog(" " + reg.RegistrationStatus.ToString());
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.WRONG_PASS;
                oper.ResultNote = "Ошибка cессии регистрации";
                oper.ResultLog = resultLog.getLog();
                //oper.DuetOperCode = res.ErrorCode;
                //oper.DuetOperNote = res.ErrorMessage;
                dataCtx.UpdateExisting(oper);
                return resultS;
                
            }

            ////nachinaetsya vipolnenie operacii
            Common mainDuet = new Common();
            CliAccountOperation operation = new CliAccountOperation();
            operation.CommandName = CommandNameType.ClientAccountDebet;
            operation.CommandSubName = CommandSubNameType.Prepare;
            operation.client_account_id = clientAccountId;
            operation.cntrg_account_id = ctrgAccountId;
            operation.cntrg_clnt_acc_id = ctrgClientAccountId;
            operation.contract_bind_id = contractBindId;
            operation.doc_date = externalDate;
            operation.doc_num = externalDocNumber;
            operation.fee_amount = feeAmount;
            operation.currency = 11;
            operation.FunctionType = functionType;
            operation.PaymentDetails = paymentDetails;
            operation.transact_amount = transactAmount;


            OperationPrepareResult res = null;
            
            res = mainDuet.Prepare_PaymentOperations(operation);
            if (!String.IsNullOrEmpty(res.ErrorMessage))
            {
                resultLog.AddToLog(" operation preparation error: " + res.ErrorMessage);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = res.ErrorCode;
                oper.DuetOperNote = res.ErrorMessage;
                //dataCtx.Operations.InsertOnSubmit(oper);
                dataCtx.UpdateExisting(oper);//SubmitChanges();
                resultS.duetOperCode = res.ErrorCode.ToString();
                resultS.duetOperNote = res.ErrorMessage;
                return resultS;
            }

            ExchangeOperationInfo info = new ExchangeOperationInfo();
            info.CommandName = CommandNameType.ClientAccountDebet;
            //info.CommandSubName = CommandSubNameType.Confirm;
            info.ExtUniqueId = externalDocNumber;
            info.ExtSubsystemId = 111;////test
            info.PackId = res.PackId;
            info.RowId = res.RowId;

            OperationConfirmResult confirmRes = mainDuet.Confirm_PaymentOperations(info);
            if (confirmRes.ErrorCode != 0)
            {
                resultLog.AddToLog(" exchange (confirmation) operation  error: " + confirmRes.ErrorMessage + " RecordId: " + confirmRes.RecordId);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = confirmRes.ErrorCode;
                oper.DuetOperNote = confirmRes.ErrorMessage;
                //dataCtx.Operations.InsertOnSubmit(oper);
                dataCtx.UpdateExisting(oper);//SubmitChanges();
                resultS.duetOperCode = confirmRes.ErrorCode.ToString();
                resultS.duetOperNote = confirmRes.ErrorMessage;
                return resultS;
            }
            
            operationresult resultSS =new operationresult(IntegerConstants.SUCCESS, "Успешно", resultLog.getLog());
            oper.Result = IntegerConstants.SUCCESS;
            oper.ResultNote = "Успешно";
            oper.ResultLog = resultLog.getLog();
            //dataCtx.Operations.InsertOnSubmit(oper);
            dataCtx.UpdateExisting(oper);//SubmitChanges();
            return resultSS;
        }

        public operationresult IsOperationSuccessful(string sessionId, string curDate, string checksum)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.IsOperationSuccessful(). ");

            DateTime externalDate = DateTime.Now;
            try
            {
                externalDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.IsOperationSuccessful(). DateTime FormatException: " + fe.Message);
                operationresult result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                return result;
            }
            string calculatedMD5 = HashUtility.CalculateMD5(curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                return new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
            }
            
            DateTime now = DateTime.Now;
            if (!(externalDate.CompareTo(now.AddMinutes(15)) < 0 && (externalDate.CompareTo(now.AddMinutes(-15)) > 0)))
            {
                return new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", resultLog.getLog());
            }

            ////now get it from the database
            CPMPayAgentService.LoggingLayer.Operation oper = null;
            oper = dataCtx.getOperation(sessionId);
                
            if (oper == null)
            {
                resultLog.AddToLog(" Search by sessionId gave no result. ");
                return new operationresult(IntegerConstants.WRONG_PARAMS, "неправильный sessionID", resultLog.getLog());
            }
            else
            {
                operationresult result = new operationresult(oper.Result.Value, oper.ResultNote, oper.ResultLog);
                if (oper.DuetOperCode.HasValue)
                    result.duetOperCode = oper.DuetOperCode.Value.ToString();
                if (!String.IsNullOrEmpty(oper.DuetOperNote))
                    result.duetOperNote = oper.DuetOperNote;
                return result;
            }         
        }

        public operationresult GetOperationsStatus(Nullable<int> result, string from, string to, string curDate, string checksum)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("Starting OperationPerformer.GetOperationsStatus(). ");

            int res = 0;
            if (result.HasValue)
                res = result.Value;

            DateTime externalDate = DateTime.Now; 
            DateTime toDate = DateTime.Now;
            DateTime fromDate = DateTime.Now;

            try
            {
                externalDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                toDate = DateTime.ParseExact(to, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                fromDate = DateTime.ParseExact(from, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            }
            catch (System.ArgumentNullException ane)
            {
                resultLog.AddToLog("OperationPerformer.GetOperationsStatus(). DateTime ArgumentNullException: " + ane.Message);
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.GetOperationsStatus(). DateTime FormatException: " + fe.Message);
                //operationresult resultO = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                //return resultO;
            }
            DateTime now = DateTime.Now;
            //if (!(externalDate.CompareTo(now.AddMinutes(10)) < 0 && (externalDate.CompareTo(now.AddMinutes(-10)) > 0)))
            //{
            //    return new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", resultLog.getLog());
            //}
            string calculatedMD5 = HashUtility.CalculateMD5(curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                return new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
            }

            CPMPayAgentService.LoggingLayer.Operation[] opers;
            if (res != 0 && (from == null || to == null))
                opers = dataCtx.RetrieveOperations(res);//Operations.Where(t => t.Result == res).ToArray();

            else if (res != 0 && from != null && to != null)
                opers = dataCtx.RetrieveOperations(res, fromDate, toDate);//Operations.Where(t => (t.ExternalDocDate.Value.CompareTo(toDate) < 0 && t.ExternalDocDate.Value.CompareTo(fromDate) > 0) && t.Result == res).ToArray();
            else if (from != null && to != null)
                opers = dataCtx.RetrieveOperations(fromDate, toDate);//.Operations.Where(t => (t.ExternalDocDate.Value.CompareTo(toDate) < 0 && t.ExternalDocDate.Value.CompareTo(fromDate) > 0)).ToArray();
            else
            {

                return new operationresult(IntegerConstants.WRONG_PARAMS, "неправильные параметры - чего-то не хватает. ", resultLog.getLog());
            }

            if (opers != null && opers.Length > 0)
            {
                int number = opers.Length;
                operation[] retopers = new operation[number];
                for (int i = 0; i < number; i++)
                {
                    retopers[i] = new operation();
                    retopers[i].result = opers[i].Result.Value;
                    retopers[i].resultlog = opers[i].ResultLog;
                    retopers[i].resultnote = opers[i].ResultNote;
                    retopers[i].sessionid = opers[i].SessionID;
                    retopers[i].datetime = opers[i].ExternalDocDate.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                    if (opers[i].DuetOperCode.HasValue)
                        retopers[i].duetOperCode = opers[i].DuetOperCode.Value.ToString();
                    if (!String.IsNullOrEmpty(opers[i].DuetOperNote))
                        retopers[i].duetOperNote = opers[i].DuetOperNote;
                }
                operationresult operRes = new operationresult();
                operRes.result = IntegerConstants.SUCCESS;
                operRes.statusNote = "Успешно";
                operRes.opercount = number.ToString();
                operRes.operations = retopers;
                return operRes;
            }
            else
            {
                operationresult operRes = new operationresult();
                operRes.result = IntegerConstants.SUCCESS;
                operRes.statusNote = "Успешно";
                operRes.opercount = 0.ToString();//// no operations
                return operRes;
            }
        }

        public RegistrationResultItem getRegistrationResult()
        {
            string host = CPMPayAgentService.Properties.Settings.Default.DUETServerIPAddress;//ConfigurationManager.AppSettings["DUETServerIPAddress"].ToString();
            int port = CPMPayAgentService.Properties.Settings.Default.DUETServerPortNumber; //Convert.ToInt32(ConfigurationManager.AppSettings["DUETServerPortNumber"].ToString());
            int channel = CPMPayAgentService.Properties.Settings.Default.ReaderChannel; //Convert.ToInt32(ConfigurationManager.AppSettings["ReaderChannel"].ToString());
            string MKPassword = CPMPayAgentService.Properties.Settings.Default.MKPassword;
            
            Card staff_card = new Card();
            staff_card.Open(channel);
            staff_card.PresentPassword(MKPassword);//"2801");

            Registration reg = new Registration();
            string conn_id = string.Empty;

            UserInfo ui = reg.OpenSession(staff_card, host, port);
            // снятие питания с МК, освобождение ридера:
            staff_card.Close();

            return reg.RegistrationStatus;
        }

        #region Added interface (first prepare, then pay)
        public operationresult PrepareDuetOperation(string sessionId, int clientAccountId, int ctrgAccountId, int ctrgClientAccountId,
            int contractBindId, string externalDocDate, string externalDocNumber, decimal feeAmount, int functionType,
            string paymentDetails, decimal transactAmount, string checksum, string curDate, string IP, IErrorReporter errRep)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.PrepareDuetOperation()");
            CPMPayAgentService.LoggingLayer.Operation oper = dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            int numOfSucOhpers = (exists && oper.Result == 7) ? 1 : 0;
            if (numOfSucOhpers > 0)
            {
                operationresult result = new operationresult(IntegerConstants.DUPLICATE_SESSION_ID, "такой sessionId уже есть", resultLog.getLog());
                return result;
            }

            if (!exists)
            {
                oper = new CPMPayAgentService.LoggingLayer.Operation();
                oper.SessionID = sessionId;
                oper.ClientAccountID = clientAccountId;
                oper.ContrAgentAccountID = ctrgAccountId;
                oper.ContrAgentClientAccountID = ctrgClientAccountId;
                oper.ContractBindID = contractBindId;
                oper.ExternalDocNumber = externalDocNumber;
                oper.feeAmount = feeAmount;
                oper.FunctionType = functionType;
                oper.PaymentDetails = paymentDetails;
                oper.TransactAmount = transactAmount;
            }
            oper.CurrentDate = DateTime.Now;
            DateTime externalDate = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            try
            {
                externalDate = DateTime.ParseExact(externalDocDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                currentDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.CurrentDate = currentDate;
                oper.ExternalDocDate = externalDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.PerformDuetOperation(). DateTime FormatException: " + fe.Message);
                operationresult result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper = null;
                errRep.reportError("Wrong parameters " + IP + " " + sessionId + " " + externalDocDate + 
                    " " + checksum + " " + curDate);
                return result;
            }
            
            string calculatedMD5 = HashUtility.CalculateMD5(sessionId, clientAccountId, ctrgAccountId, ctrgClientAccountId, contractBindId, externalDocDate, externalDocNumber, feeAmount, functionType, paymentDetails, transactAmount, curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                operationresult result = new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
                oper = null;
                errRep.reportError("Wrong checksum " + IP + " " + sessionId + " " +checksum + " " + curDate);
                return result;
            }

            DateTime now = DateTime.Now;
            CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError("Now: " + now.ToString() + " curdate: " + curDate.ToString() +
               (currentDate.CompareTo(now.AddMinutes(15)) < 0) + " " + (currentDate.CompareTo(now.AddMinutes(-15)) > 0));
            if (!(currentDate.CompareTo(now.AddMinutes(15)) < 0 && (currentDate.CompareTo(now.AddMinutes(-15)) > 0)))
            {
                operationresult result = new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", resultLog.getLog());
                oper = null;
                errRep.reportError("Wrong time " + IP +" "+ sessionId + " " + checksum + " " + curDate);
                return result;
            }

            string host = CPMPayAgentService.Properties.Settings.Default.DUETServerIPAddress;//ConfigurationManager.AppSettings["DUETServerIPAddress"].ToString();
            int port = CPMPayAgentService.Properties.Settings.Default.DUETServerPortNumber; //Convert.ToInt32(ConfigurationManager.AppSettings["DUETServerPortNumber"].ToString());
            int channel = CPMPayAgentService.Properties.Settings.Default.ReaderChannel; //Convert.ToInt32(ConfigurationManager.AppSettings["ReaderChannel"].ToString());
            string MKPassword = CPMPayAgentService.Properties.Settings.Default.MKPassword;

            ////save data to DB before making DUET transaction
            oper.Result = IntegerConstants.IN_PROCESS;
            oper.ResultNote = "В процессе";
            oper.ResultLog = resultLog.getLog();
            if (!exists)
                dataCtx.InsertNew(oper);
            else
                dataCtx.UpdateExisting(oper);
            
            oper = dataCtx.getOperation(sessionId);
            //// start connecting to DUET server
            Card staff_card = new Card();
            try
            {
                staff_card.Open(channel);
                staff_card.PresentPassword(MKPassword);
            }
            catch (Exception ex)
            {
                resultLog.AddToLog(" Error: " + ex.Message);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.WRONG_PASS;
                oper.ResultNote = "Ошибка пароля карты";
                oper.ResultLog = resultLog.getLog();
                dataCtx.UpdateExisting(oper);
                return resultS;

            }

            Registration reg = new Registration();
            string conn_id = string.Empty;

            UserInfo ui = reg.OpenSession(staff_card, host, port);
            // снятие питания с МК, освобождение ридера:
            staff_card.Close();

            // проверка - успешно ли установлена сессия:
            if (reg.RegistrationStatus != RegistrationResultItem.RegistrationSuccefully)
            {                
                resultLog.AddToLog(" " + reg.RegistrationStatus.ToString());
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.WRONG_PASS;
                oper.ResultNote = "Ошибка cессии регистрации";
                oper.ResultLog = resultLog.getLog();
                dataCtx.UpdateExisting(oper);
                return resultS;
            }

            ////nachinaetsya vipolnenie operacii
            Common mainDuet = new Common();
            CliAccountOperation operation = new CliAccountOperation();
            operation.CommandName = CommandNameType.ClientAccountDebet;
            operation.CommandSubName = CommandSubNameType.Prepare;
            operation.client_account_id = clientAccountId;
            operation.cntrg_account_id = ctrgAccountId;
            operation.cntrg_clnt_acc_id = ctrgClientAccountId;
            operation.contract_bind_id = contractBindId;
            operation.doc_date = externalDate;
            operation.doc_num = externalDocNumber;
            operation.fee_amount = feeAmount;
            operation.currency = 11;
            operation.FunctionType = functionType;
            operation.PaymentDetails = paymentDetails;
            operation.transact_amount = transactAmount;


            OperationPrepareResult res = null;

            res = mainDuet.Prepare_PaymentOperations(operation);
            if (!String.IsNullOrEmpty(res.ErrorMessage))
            {
                resultLog.AddToLog(" operation preparation error: " + res.ErrorMessage);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = res.ErrorCode;
                oper.DuetOperNote = res.ErrorMessage;
                dataCtx.UpdateExisting(oper);
                resultS.duetOperCode = res.ErrorCode.ToString();
                resultS.duetOperNote = res.ErrorMessage;
                return resultS;
            }

            ExchangeOperationInfo info = new ExchangeOperationInfo();
            info.CommandName = CommandNameType.ClientAccountDebet;
            info.ExtUniqueId = externalDocNumber;
            info.ExtSubsystemId = 111;////this is the ID we chose
            info.PackId = res.PackId;
            info.RowId = res.RowId;

            resultsTable.Add(sessionId, info);
            /*OperationConfirmResult confirmRes = mainDuet.Confirm_PaymentOperations(info);
            if (confirmRes.ErrorCode != 0)
            {
                resultLog.AddToLog(" exchange (confirmation) operation  error: " + confirmRes.ErrorMessage + " RecordId: " + confirmRes.RecordId);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = confirmRes.ErrorCode;
                oper.DuetOperNote = confirmRes.ErrorMessage;
                //dataCtx.Operations.InsertOnSubmit(oper);
                dataCtx.UpdateExisting(oper);//SubmitChanges();
                resultS.duetOperCode = confirmRes.ErrorCode.ToString();
                resultS.duetOperNote = confirmRes.ErrorMessage;
                return resultS;
            }*/

            operationresult resultSS = new operationresult(IntegerConstants.PREPARED, "Успешно", resultLog.getLog());
            oper.Result = IntegerConstants.PREPARED;
            oper.ResultNote = "Успешно";
            oper.ResultLog = resultLog.getLog();
            dataCtx.UpdateExisting(oper);
            return resultSS;
        }

        public operationresult ConfirmDuetOperation(string sessionId, string externalDocDate, string externalDocNumber, 
            string curDate, string IP, IErrorReporter errRep)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.ConfirmDuetOperation()");
            CPMPayAgentService.LoggingLayer.Operation oper = dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            if (!exists || !resultsTable.ContainsKey(sessionId))
            {
                operationresult result = new operationresult(IntegerConstants.NO_SUCH_SESSION, "такого sessionId нет.", resultLog.getLog());
                return result;
            }
            DateTime externalDate;
            DateTime currentDate;

            try
            {
                externalDate = DateTime.ParseExact(externalDocDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                currentDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.CurrentDate = currentDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.ConfirmDuetOperation(). DateTime FormatException: " + fe.Message);
                operationresult result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper = null;
                errRep.reportError("Wrong parameters " + IP + " " + sessionId + " " + externalDocDate +
                    " " + curDate);
                return result;
            }

            DateTime now = DateTime.Now;
            CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError("Now: " + now.ToString() + " curdate: " + curDate.ToString() +
               (currentDate.CompareTo(now.AddMinutes(15)) < 0) + " " + (currentDate.CompareTo(now.AddMinutes(-15)) > 0));
            if (!(currentDate.CompareTo(now.AddMinutes(15)) < 0 && (currentDate.CompareTo(now.AddMinutes(-15)) > 0)))
            {
                operationresult result = new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", resultLog.getLog());
                oper = null;
                errRep.reportError("Wrong time " + IP + " " + sessionId + " " + curDate);
                return result;
            }



            string host = CPMPayAgentService.Properties.Settings.Default.DUETServerIPAddress;//ConfigurationManager.AppSettings["DUETServerIPAddress"].ToString();
            int port = CPMPayAgentService.Properties.Settings.Default.DUETServerPortNumber; //Convert.ToInt32(ConfigurationManager.AppSettings["DUETServerPortNumber"].ToString());
            int channel = CPMPayAgentService.Properties.Settings.Default.ReaderChannel; //Convert.ToInt32(ConfigurationManager.AppSettings["ReaderChannel"].ToString());
            string MKPassword = CPMPayAgentService.Properties.Settings.Default.MKPassword;

            //// start connecting to DUET server
            Card staff_card = new Card();
            try
            {
                staff_card.Open(channel);
                staff_card.PresentPassword(MKPassword);
            }
            catch (Exception ex)
            {
                resultLog.AddToLog(" Error: " + ex.Message);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", "Ошибка пароля карты "+resultLog.getLog());
                return resultS;

            }

            Registration reg = new Registration();
            string conn_id = string.Empty;

            UserInfo ui = reg.OpenSession(staff_card, host, port);
            // снятие питания с МК, освобождение ридера:
            staff_card.Close();

            // проверка - успешно ли установлена сессия:
            if (reg.RegistrationStatus != RegistrationResultItem.RegistrationSuccefully)
            {
                resultLog.AddToLog(" " + reg.RegistrationStatus.ToString());
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", "Ошибка cессии регистрации"+resultLog.getLog());
                //oper.Result = IntegerConstants.WRONG_PASS;
                //oper.ResultNote = "Ошибка cессии регистрации";
                //oper.ResultLog = resultLog.getLog();
                //dataCtx.UpdateExisting(oper);
                return resultS;
            }

            ////nachinaetsya vipolnenie operacii
            Common mainDuet = new Common();

            ExchangeOperationInfo info = (ExchangeOperationInfo)resultsTable[sessionId];
            info.CommandSubName = CommandSubNameType.Prepare;

            OperationConfirmResult confirmRes = mainDuet.Confirm_PaymentOperations(info);
            if (confirmRes.ErrorCode != 0)
            {
                resultLog.AddToLog(" exchange (confirmation) operation  error: " + confirmRes.ErrorMessage + " RecordId: " + confirmRes.RecordId);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = confirmRes.ErrorCode;
                oper.DuetOperNote = confirmRes.ErrorMessage;
                dataCtx.UpdateExisting(oper);
                resultS.duetOperCode = confirmRes.ErrorCode.ToString();
                resultS.duetOperNote = confirmRes.ErrorMessage;
                return resultS;
            }
            resultsTable.Remove(sessionId);
            operationresult resultSS = new operationresult(IntegerConstants.SUCCESS, "Успешно", resultLog.getLog());
            oper.Result = IntegerConstants.SUCCESS;
            oper.ResultNote = "Успешно";
            oper.ResultLog = resultLog.getLog();
            dataCtx.UpdateExisting(oper);
            return resultSS;
        }

        public operationresult CancelDuetOperation(string sessionId, string externalDocDate, string externalDocNumber,
            string curDate, string IP, IErrorReporter errRep)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.ConfirmDuetOperation()");
            CPMPayAgentService.LoggingLayer.Operation oper = dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            if (!exists || !resultsTable.ContainsKey(sessionId))
            {
                operationresult result = new operationresult(IntegerConstants.NO_SUCH_SESSION, "такого sessionId нет.", resultLog.getLog());
                return result;
            }
            DateTime externalDate;
            DateTime currentDate;

            try
            {
                externalDate = DateTime.ParseExact(externalDocDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                currentDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.CurrentDate = currentDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.ConfirmDuetOperation(). DateTime FormatException: " + fe.Message);
                operationresult result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper = null;
                errRep.reportError("Wrong parameters " + IP + " " + sessionId + " " + externalDocDate +
                    " " + curDate);
                return result;
            }

            DateTime now = DateTime.Now;
            CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError("Now: " + now.ToString() + " curdate: " + curDate.ToString() +
               (currentDate.CompareTo(now.AddMinutes(15)) < 0) + " " + (currentDate.CompareTo(now.AddMinutes(-15)) > 0));
            if (!(currentDate.CompareTo(now.AddMinutes(15)) < 0 && (currentDate.CompareTo(now.AddMinutes(-15)) > 0)))
            {
                operationresult result = new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", resultLog.getLog());
                oper = null;
                errRep.reportError("Wrong time " + IP + " " + sessionId + " " + curDate);
                return result;
            }



            string host = CPMPayAgentService.Properties.Settings.Default.DUETServerIPAddress;//ConfigurationManager.AppSettings["DUETServerIPAddress"].ToString();
            int port = CPMPayAgentService.Properties.Settings.Default.DUETServerPortNumber; //Convert.ToInt32(ConfigurationManager.AppSettings["DUETServerPortNumber"].ToString());
            int channel = CPMPayAgentService.Properties.Settings.Default.ReaderChannel; //Convert.ToInt32(ConfigurationManager.AppSettings["ReaderChannel"].ToString());
            string MKPassword = CPMPayAgentService.Properties.Settings.Default.MKPassword;

            //// start connecting to DUET server
            Card staff_card = new Card();
            try
            {
                staff_card.Open(channel);
                staff_card.PresentPassword(MKPassword);
            }
            catch (Exception ex)
            {
                resultLog.AddToLog(" Error: " + ex.Message);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", "Ошибка пароля карты " + resultLog.getLog());
                return resultS;

            }

            Registration reg = new Registration();
            string conn_id = string.Empty;

            UserInfo ui = reg.OpenSession(staff_card, host, port);
            // снятие питания с МК, освобождение ридера:
            staff_card.Close();

            // проверка - успешно ли установлена сессия:
            if (reg.RegistrationStatus != RegistrationResultItem.RegistrationSuccefully)
            {
                resultLog.AddToLog(" " + reg.RegistrationStatus.ToString());
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", "Ошибка cессии регистрации" + resultLog.getLog());
                //oper.Result = IntegerConstants.WRONG_PASS;
                //oper.ResultNote = "Ошибка cессии регистрации";
                //oper.ResultLog = resultLog.getLog();
                //dataCtx.UpdateExisting(oper);
                return resultS;
            }

            ////nachinaetsya vipolnenie operacii
            Common mainDuet = new Common();

            ExchangeOperationInfo info = (ExchangeOperationInfo)resultsTable[sessionId];

            OperationResult confirmRes = mainDuet.Cancel_PaymentOperations(info);
            if (confirmRes.ErrorCode != 0)
            {
                resultLog.AddToLog(" exchange (cancel) operation  error: " + confirmRes.ErrorMessage + " ErrorId: " + confirmRes.ErrorCode);
                operationresult resultS = new operationresult(IntegerConstants.ERROR, "Ошибка", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = confirmRes.ErrorCode;
                oper.DuetOperNote = confirmRes.ErrorMessage;
                dataCtx.UpdateExisting(oper);
                resultS.duetOperCode = confirmRes.ErrorCode.ToString();
                resultS.duetOperNote = confirmRes.ErrorMessage;
                return resultS;
            }
            resultsTable.Remove(sessionId);
            operationresult resultSS = new operationresult(IntegerConstants.SUCCESS, "Успешно", resultLog.getLog());
            oper.Result = IntegerConstants.SUCCESS;
            oper.ResultNote = "Успешно";
            oper.ResultLog = resultLog.getLog();
            dataCtx.UpdateExisting(oper);
            return resultSS;
        }
        #endregion

        #region Block Cards
        public operationresult BlockCardAccount(string sessionId, int clientAccountId, int block_type, int block_action, string reason, string checksum, string curDate, string IP)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.Prepare_CardAccountBlock()");

            operationresult result = new operationresult(IntegerConstants.ERROR, "Card Block Error", resultLog.getLog());

            CPMPayAgentService.LoggingLayer.Operation oper = dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            int numOfSucOpers = (exists && oper.Result == 7) ? 1 : 0;
            if (numOfSucOpers > 0)
            {
                result = new operationresult(IntegerConstants.DUPLICATE_SESSION_ID, "такой sessionId уже есть", resultLog.getLog());
                return result;
            }

            if (!exists)
            {
                oper = new CPMPayAgentService.LoggingLayer.Operation();
                oper.SessionID = sessionId;
                oper.ClientAccountID = clientAccountId;
                oper.ExternalDocNumber = sessionId;
                oper.FunctionType = block_type;
                oper.TransactAmount = 0;
            }
            oper.CurrentDate = DateTime.Now;
            //DateTime externalDate = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            try
            {
                //externalDate = DateTime.ParseExact(externalDocDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                currentDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.ExternalDocDate = currentDate;
                //oper.ExternalDocDate = externalDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.Prepare_CardAccountBlock(). DateTime FormatException: " + fe.Message);
                result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper = null;
                return result;
            }

            string calculatedMD5 = HashUtility.CalculateMD5(sessionId, curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                result = new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
                oper = null;
                return result;
            }
            resultLog.AddToLog("OperationPerformer.Prepare_CardAccountBlock() - passed checksum test.");
            CardAccountBlock blockData = new CardAccountBlock()
            {
                ExtUniqueId = sessionId, //Guid.NewGuid().ToString()
                card_account_id = clientAccountId,
                block_type = (BlockType)block_type,
                block_action = (BlockAction)block_action,
                reason = reason
            };

            resultLog.AddToLog("Preparing card block...");
            OperationPrepareResult ret = Common.CurrentCommon.Prepare_CardAccountBlock(blockData);
            if (ret.ErrorCode != 0)
            {
                resultLog.AddToLog("Error prepare: " + ret.ErrorCode + " " + ret.ErrorMessage);
                result = new operationresult(IntegerConstants.ERROR, "Card Block Prepare Error", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = ret.ErrorCode;
                oper.DuetOperNote = ret.ErrorMessage;
                dataCtx.InsertNew(oper);
                return result;
            }
            resultLog.AddToLog("Card Block Prepare OK. pack_id: " + ret.PackId + " row_id: " + ret.RowId);
            OperationConfirmResult retConfirm = Common.CurrentCommon.Confirm_CardAccountBlock(new ExchangeOperationInfo()
            {
                PackId = ret.PackId,
                RowId = ret.RowId,
                ExtUniqueId = sessionId //Guid.NewGuid().ToString()
            });

            if (retConfirm.ErrorCode != 0)
            {
                resultLog.AddToLog("Error Card Block confirm - Code: " + retConfirm.ErrorCode + ", Message: " + retConfirm.ErrorMessage);
                result = new operationresult(IntegerConstants.ERROR, "Card Block Confirm Error", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = ret.ErrorCode;
                oper.DuetOperNote = ret.ErrorMessage;
                dataCtx.InsertNew(oper);
                return result;
            }
            resultLog.AddToLog("Card Block Confirm OK. pack_id, row_id: " + ret.PackId + ", " + ret.RowId +
            "record_id, record_date: " + retConfirm.RecordId + ", " + retConfirm.RecordDate);

            result = new operationresult(IntegerConstants.SUCCESS, "Card Block Confirm OK", resultLog.getLog());
            oper.Result = IntegerConstants.SUCCESS;
            oper.ResultNote = "Успешно";
            oper.ResultLog = resultLog.getLog();
            dataCtx.InsertNew(oper);
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="clientDataId"></param>
        /// <param name="emvAccountId"></param>
        /// <param name="emvCardId"></param>
        /// <param name="reason">1 - ukradena, 2 - uteryana, 3 - razblokirovat'</param>
        /// <param name="checksum"></param>
        /// <param name="curDate"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public operationresult BlockUnblockEMVCardAccount(string sessionId, int clientDataId, int emvAccountId, int emvCardId, int reason, string checksum, string curDate, string IP)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.BlockUnblockEMVCardAccount()");

            operationresult result = new operationresult(IntegerConstants.ERROR, "EMV Card Block/Unblock Error", resultLog.getLog());

            CPMPayAgentService.LoggingLayer.Operation oper = dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            int numOfSucOpers = (exists && oper.Result == 7) ? 1 : 0;
            if (numOfSucOpers > 0)
            {
                result = new operationresult(IntegerConstants.DUPLICATE_SESSION_ID, "такой sessionId уже есть", resultLog.getLog());
                return result;
            }

            if (!exists)
            {
                oper = new CPMPayAgentService.LoggingLayer.Operation();
                oper.SessionID = sessionId;
                oper.ClientAccountID = emvAccountId;
                oper.ExternalDocNumber = sessionId;
                oper.FunctionType = emvCardId;
                oper.TransactAmount = 0;
            }
            oper.CurrentDate = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            try
            {
                currentDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.ExternalDocDate = currentDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.BlockUnblockEMVCardAccount(). DateTime FormatException: " + fe.Message);
                result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper = null;
                return result;
            }

            string calculatedMD5 = HashUtility.CalculateMD5(sessionId, curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                result = new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
                oper = null;
                return result;
            }

            string appSubType = "STCC01";
            string hotCardStatus = "CHST7";
            switch (reason)
	        {
                case 1:
                    appSubType = "STCC01";
                    hotCardStatus = "CHST7";
                    break;
                case 2:
                    appSubType = "STCC01";
                    hotCardStatus = "CHST6";
                    break;
                case 3:
                    appSubType = "STCC03";
                    hotCardStatus = "CHST0";
                    break;
		        default:
                    break;
	        }


            resultLog.AddToLog("EMV Request: " + string.Format(StringConstants.EMVBlockUnblockOperation, clientDataId.ToString(), emvAccountId.ToString(), emvCardId.ToString(), appSubType, hotCardStatus));
            var proxy = Common.CurrentCommon;

            OperationConfirmResult ret = proxy.EMVApplicationOperation(new EMVOperation()
            {
                Request = string.Format(StringConstants.EMVBlockUnblockOperation, clientDataId.ToString(), emvAccountId.ToString(), emvCardId.ToString(), appSubType, hotCardStatus)
            });

            resultLog.AddToLog("Response: " + ret.DoSerialize());

            if (ret.ErrorCode != 0)
            {
                resultLog.AddToLog("Error: " + ret.ErrorMessage);
                result = new operationresult(IntegerConstants.ERROR, "EMV Card Block/Unblock Error ", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = ret.ErrorCode;
                oper.DuetOperNote = ret.ErrorMessage;
                dataCtx.InsertNew(oper);
                return result;
            }
            else
                resultLog.AddToLog("Success: rec ID: " + ret.RecordId+", date: "+ret.RecordDate.ToString());

            result = new operationresult(IntegerConstants.SUCCESS, "EMV Card Block/Unblock Confirm OK", resultLog.getLog());
            oper.Result = IntegerConstants.SUCCESS;
            oper.ResultNote = "Успешно";
            oper.ResultLog = resultLog.getLog();
            dataCtx.InsertNew(oper);
            return result;
        }
        #endregion

        #region EMV Load/Unload
        /// <summary>
        ///  
        /// </summary>
        /// <param name="transactAmount"></param>
        /// <param name="EMVAccountID"> etot parameter vozmozhno dolzhen bit int ili long</param>
        /// <param name="externalDocDate"></param>
        /// <param name="externalDocNumber"></param>
        /// <returns></returns>
        public operationresult EMVLoadOperation(string sessionId, decimal transactAmount, string EMVAccountID, string externalDocDate, string externalDocNumber, string checksum, string curDate, string IP)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("EMV Load...");
            
            operationresult result = new operationresult(IntegerConstants.ERROR, "EMVLoadOperation", resultLog.getLog());

            CPMPayAgentService.LoggingLayer.Operation oper = dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            int numOfSucOpers = (exists && oper.Result == 7) ? 1 : 0;
            if (numOfSucOpers > 0)
            {
                result = new operationresult(IntegerConstants.DUPLICATE_SESSION_ID, "такой sessionId уже есть", resultLog.getLog());
                return result;
            }
            int cliAccId = 0;
            Int32.TryParse(EMVAccountID, out cliAccId);
            if (!exists)
            {
                oper = new CPMPayAgentService.LoggingLayer.Operation();
                oper.SessionID = sessionId;
                oper.ClientAccountID = cliAccId;
                oper.ExternalDocNumber = externalDocNumber;
                oper.FunctionType = 0;
                oper.TransactAmount = transactAmount;
            }
            oper.CurrentDate = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            try
            {
                currentDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.ExternalDocDate = currentDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.BlockUnblockEMVCardAccount(). DateTime FormatException: " + fe.Message);
                result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper = null;
                return result;
            }

            string calculatedMD5 = HashUtility.CalculateMD5(sessionId, curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                result = new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
                oper = null;
                return result;
            }

            string operStr = string.Format(StringConstants.EMVLoadOperation, EMVAccountID.ToString());

            resultLog.AddToLog("Request: " + operStr);
            resultLog.AddToLog("Amount: " + transactAmount.ToString());

            var proxy = Common.CurrentCommon;

            OperationConfirmResult ret = proxy.EMVAccountOperation(new EMVOperation()
            {
                TransactAmount = transactAmount,
                Request = operStr,
                // Id файла:
                ExtUniqueId = externalDocNumber
            });

            resultLog.AddToLog("Response: " + ret.DoSerialize());
            if (ret.ErrorCode != 0)
            {
               resultLog.AddToLog("Error: " + ret.ErrorMessage);
               result = new operationresult(IntegerConstants.ERROR, "EMVLoadOperation Confirm Error", resultLog.getLog());
               oper.Result = IntegerConstants.ERROR;
               oper.ResultNote = "Ошибка";
               oper.ResultLog = resultLog.getLog();
               oper.DuetOperCode = ret.ErrorCode;
               oper.DuetOperNote = ret.ErrorMessage;
               dataCtx.InsertNew(oper);
            }
            else
            {
                resultLog.AddToLog("RecordDate: " + ret.RecordDate);
                resultLog.AddToLog("RecordId: " + ret.RecordId);
                result = new operationresult(IntegerConstants.SUCCESS, "EMVLoadOperation Confirm OK", resultLog.getLog());
                oper.Result = IntegerConstants.SUCCESS;
                oper.ResultNote = "Успешно";
                oper.ResultLog = resultLog.getLog();
                dataCtx.InsertNew(oper);
            }
            return result;
        }

        public operationresult EMVUnLoadOperation(string sessionId, decimal transactAmount, string EMVAccountID, string externalDocDate, string externalDocNumber, string checksum, string curDate, string IP)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("EMV Load...");

            operationresult result = new operationresult(IntegerConstants.ERROR, "EMVUnLoadOperation", resultLog.getLog());

            CPMPayAgentService.LoggingLayer.Operation oper = dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            int numOfSucOpers = (exists && oper.Result == 7) ? 1 : 0;
            if (numOfSucOpers > 0)
            {
                result = new operationresult(IntegerConstants.DUPLICATE_SESSION_ID, "такой sessionId уже есть", resultLog.getLog());
                return result;
            }

            int cliAccId = 0;
            Int32.TryParse(EMVAccountID, out cliAccId);
            if (!exists)
            {
                oper = new CPMPayAgentService.LoggingLayer.Operation();
                oper.SessionID = sessionId;
                oper.ClientAccountID = cliAccId;
                oper.ExternalDocNumber = externalDocNumber;
                oper.FunctionType = 0;
                oper.TransactAmount = transactAmount;
            }
            oper.CurrentDate = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            try
            {
                currentDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.ExternalDocDate = currentDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.BlockUnblockEMVCardAccount(). DateTime FormatException: " + fe.Message);
                result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper = null;
                return result;
            }

            string calculatedMD5 = HashUtility.CalculateMD5(sessionId, curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                result = new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
                oper = null;
                return result;
            }

            string operStr = string.Format(StringConstants.EMVUnLoadOperation, EMVAccountID.ToString());
            resultLog.AddToLog("Request: " + operStr);
            resultLog.AddToLog("Amount: " + transactAmount.ToString());

            var proxy = Common.CurrentCommon;

            OperationConfirmResult ret = proxy.EMVAccountOperation(new EMVOperation()
            {
                TransactAmount = transactAmount,
                Request = operStr,
                // Id файла:
                ExtUniqueId = externalDocNumber
            });

            resultLog.AddToLog("Response: " + ret.DoSerialize());
            if (ret.ErrorCode != 0)
            {
                resultLog.AddToLog("Error: " + ret.ErrorMessage);
                result = new operationresult(IntegerConstants.ERROR, "EMVLoadOperation Confirm Error", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = ret.ErrorCode;
                oper.DuetOperNote = ret.ErrorMessage;
                dataCtx.InsertNew(oper);
            }
            else
            {
                resultLog.AddToLog("RecordDate: " + ret.RecordDate);
                resultLog.AddToLog("RecordId: " + ret.RecordId);
                result = new operationresult(IntegerConstants.SUCCESS, "EMVLoadOperation Confirm OK", resultLog.getLog());
                oper.Result = IntegerConstants.SUCCESS;
                oper.ResultNote = "Успешно";
                oper.ResultLog = resultLog.getLog();
                dataCtx.InsertNew(oper);
            }
            return result;
        }
        #endregion

        #region Accounts
        
        public operationresult OpenClientOperation(string sessionId, string name, string address, string index, int bankId,
            int clientParticipant, int region, int clientClass, string contractNumber, DateTime contractDate,
            string taxID1, string taxID2, string INN, string OKPO, string checksum, string curDate, string IP)
        {
            // CREATE CLIENT:
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.OpenClientOperation()");
            resultLog.AddToLog("Client create operation...");
            
            operationresult result = new operationresult(IntegerConstants.ERROR, "Client Create Error", resultLog.getLog());

            CPMPayAgentService.LoggingLayer.Operation oper = dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            int numOfSucOpers = (exists && oper.Result == 7) ? 1 : 0;
            if (numOfSucOpers > 0)
            {
                result = new operationresult(IntegerConstants.DUPLICATE_SESSION_ID, "такой sessionId уже есть", resultLog.getLog());
                return result;
            }

            if (!exists)
            {
                oper = new CPMPayAgentService.LoggingLayer.Operation();
                oper.SessionID = sessionId;
                oper.ContrAgentAccountID = bankId;
                oper.ContrAgentClientAccountID = region;
                oper.ContractBindID = clientClass;
                oper.ExternalDocNumber = contractNumber;
                oper.FunctionType = clientParticipant;
                oper.PaymentDetails = name;
                oper.TransactAmount = 0;
            }
            oper.CurrentDate = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            try
            {
                //currentDate = DateTime.ParseExact(contractDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.ExternalDocDate = contractDate;//currentDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.OpenClientOperation(). DateTime FormatException: " + fe.Message);
                result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper = null;
                return result;
            }

            string calculatedMD5 = HashUtility.CalculateMD5(sessionId, curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                result = new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
                oper = null;
                return result;
            }

            resultLog.AddToLog("Create ClientCreate proxy...");
            ClientCreate clCreate = new ClientCreate();

            ClientData clData = new ClientData()
            {
                client_name = name,
                address_1 = address,
                client_index_id = index,
                cli_bank_number = bankId,
                cli_participant = clientParticipant,
                cli_region = region,
                client_class = clientClass,
                client_identity_id = 1,
                client_status = 2,
                contract_id = contractNumber,
                contract_date = contractDate,
                client_tax_id1 = taxID1,
                client_tax_id2 = taxID2,
                //ewp_data_id = (Settings.Default.EwpDataId > 0) ? Settings.Default.EwpDataId : (int?)null,
                inn = INN,
                okpo = OKPO//,
                //client_data_id = Settings.Default.ClientDataId
            };

            var retCreate = clCreate.PrepareOperation(clData);
            if (retCreate.ErrorCode != 0)
            {
                resultLog.AddToLog("Error prepare: " + retCreate.ErrorCode + " " + retCreate.ErrorMessage);
                result = new operationresult(IntegerConstants.ERROR, "Client Create Prepare Error", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = retCreate.ErrorCode;
                oper.DuetOperNote = retCreate.ErrorMessage;
                dataCtx.InsertNew(oper);
                return result;
            }
            Console.WriteLine("Prepare ok. pack_id: " + retCreate.PackId + " row_id: " + retCreate.RowId);
            var retConfirm = clCreate.ConfirmOperation(new OperationInfo()
            {
                PackId = retCreate.PackId,
                RowId = retCreate.RowId,
                ExtUniqueId = Guid.NewGuid().ToString()
            });

            if (retConfirm.ErrorCode != 0)
            {
                Console.WriteLine("Error confirm: " + retConfirm.ErrorCode + " " + retConfirm.ErrorMessage);
                result = new operationresult(IntegerConstants.ERROR, "Client Create Confirm Error", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = retConfirm.ErrorCode;
                oper.DuetOperNote = retConfirm.ErrorMessage;
                dataCtx.InsertNew(oper);
                return result;
            }

            resultLog.AddToLog("Confirm OK. client_data_id: " + retConfirm.client_data_id);
            result = new operationresult(IntegerConstants.SUCCESS, "Client Create Confirm OK", resultLog.getLog());
            oper.Result = IntegerConstants.SUCCESS;
            oper.ResultNote = "Успешно";
            oper.ResultLog = resultLog.getLog();
            dataCtx.InsertNew(oper);
            return result;
        }

        public operationresult OpenAccountOperation(string sessionId, int clientDataId, int accountSubNumber,
            int cntrgAccountId, string extAccountNumber, string externalDocNumber, int cPart,
            string checksum, string curDate, string IP)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.OpenAccountOperation()");
            resultLog.AddToLog("Account create operation... (client_data_id = " + clientDataId + ")");

            operationresult result = new operationresult(IntegerConstants.ERROR, "Open Account Error", resultLog.getLog());

            CPMPayAgentService.LoggingLayer.Operation oper = dataCtx.getOperation(sessionId);
            bool exists = (oper != null);
            int numOfSucOpers = (exists && oper.Result == 7) ? 1 : 0;
            if (numOfSucOpers > 0)
            {
                result = new operationresult(IntegerConstants.DUPLICATE_SESSION_ID, "такой sessionId уже есть", resultLog.getLog());
                return result;
            }

            if (!exists)
            {
                oper = new CPMPayAgentService.LoggingLayer.Operation();
                oper.SessionID = sessionId;
                oper.ClientAccountID = clientDataId;
                oper.ContrAgentAccountID = accountSubNumber;
                oper.ExternalDocNumber = externalDocNumber;
                oper.FunctionType = cPart;
                oper.TransactAmount = 0;
            }
            oper.CurrentDate = DateTime.Now;
            DateTime currentDate = DateTime.Now;
            try
            {
                currentDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.ExternalDocDate = currentDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.OpenClientOperation(). DateTime FormatException: " + fe.Message);
                result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper = null;
                return result;
            }

            string calculatedMD5 = HashUtility.CalculateMD5(sessionId, curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                result = new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
                oper = null;
                return result;
            }

            CliAccountOpen clAcc = new CliAccountOpen()
            {
                ExtUniqueId = externalDocNumber,
                client_data_id = clientDataId,
                account_subnumber = accountSubNumber,
                cntrg_account_id = cntrgAccountId,
                ext_account_number = extAccountNumber,
                cPart = cPart
            };
            resultLog.AddToLog("Prepare...");
            OperationPrepareResult ret = Common.CurrentCommon.Prepare_AccountOpen(clAcc);
            if (ret.ErrorCode != 0)
            {
                resultLog.AddToLog("Error prepare: " + ret.ErrorCode + " " + ret.ErrorMessage);
                result = new operationresult(IntegerConstants.ERROR, "Open Account Prepare Error", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = ret.ErrorCode;
                oper.DuetOperNote = ret.ErrorMessage;
                dataCtx.InsertNew(oper);
                return result;
            }
            resultLog.AddToLog("Prepare ok. pack_id: " + ret.PackId + " row_id: " + ret.RowId);
            CliAccountConfirmResult retConfirm = Common.CurrentCommon.Confirm_AccountOpen(new ExchangeOperationInfo()
            {
                PackId = ret.PackId,
                RowId = ret.RowId,
                ExtUniqueId = externalDocNumber
            });

            if (retConfirm.ErrorCode != 0)
            {
                resultLog.AddToLog("Error confirm: " + retConfirm.ErrorCode + " " + retConfirm.ErrorMessage);
                result = new operationresult(IntegerConstants.ERROR, "Open Account Confirm Error", resultLog.getLog());
                oper.Result = IntegerConstants.ERROR;
                oper.ResultNote = "Ошибка";
                oper.ResultLog = resultLog.getLog();
                oper.DuetOperCode = ret.ErrorCode;
                oper.DuetOperNote = ret.ErrorMessage;
                dataCtx.InsertNew(oper);
                return result;
            }
            resultLog.AddToLog("Confirm ok. client_account_id: " + retConfirm.client_account_id);
            result = new operationresult(IntegerConstants.SUCCESS, "Open Account Confirm OK", resultLog.getLog());
            oper.Result = IntegerConstants.SUCCESS;
            oper.ResultNote = "Успешно";
            oper.ResultLog = resultLog.getLog();
            dataCtx.InsertNew(oper);
            return result;
        }
        
        #endregion
    }
}
