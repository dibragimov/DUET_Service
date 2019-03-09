using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace DUETCPMConsole
{
    public class OperationPerformer
    {
        private static OperationPerformer _instance;
        private LoggingLayer.LoggingDataClassesDataContext dataCtx;

        public static OperationPerformer Instance()
        {
            if (_instance == null)
                _instance = new OperationPerformer();
            return _instance;
        }

        private OperationPerformer()
        {
            dataCtx = new DUETCPMConsole.LoggingLayer.LoggingDataClassesDataContext();
        }

        public operationresult PerformDuetOperation(long sessionId, int clientAccountId, int ctrgAccountId, int ctrgClientAccountId,
            int contractBindId, string externalDocDate, string externalDocNumber, decimal feeAmount, int functionType,
            string paymentDetails, decimal transactAmount, string checksum)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.PerformDuetOperation()");

            int numOfSucOhpers = dataCtx.Operations.Where(t => t.SessionID == sessionId).Count();
            if (numOfSucOhpers > 0)
            {
                operationresult result = new operationresult(IntegerConstants.DUPLICATE_SESSION_ID, "такой sessionId уже есть", resultLog.getLog());
                return result;
            }
            
            LoggingLayer.Operation oper = new DUETCPMConsole.LoggingLayer.Operation();
            oper.SessionID = sessionId;
            oper.ClientAccountID = clientAccountId;
            oper.ContrAgentAccountID = ctrgAccountId;
            oper.ContrAgentClientAccountID = ctrgClientAccountId;
            oper.ContractBindID = contractBindId;
            oper.CurrentDate = DateTime.Now;
            oper.ExternalDocNumber = externalDocNumber;
            oper.feeAmount = feeAmount;
            oper.FunctionType = functionType;
            oper.PaymentDetails = paymentDetails;
            oper.TransactAmount = transactAmount;
            DateTime externalDate = DateTime.Now;
            try
            {
                externalDate = DateTime.ParseExact(externalDocDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                oper.ExternalDocDate = externalDate;
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.PerformDuetOperation(). DateTime FormatException: " + fe.Message);
                operationresult result = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                oper.Result = IntegerConstants.WRONG_PARAMS;
                oper.ResultNote = "не указаны необходимые параметры запроса";
                oper.ResultLog = resultLog.getLog();
                dataCtx.Operations.InsertOnSubmit(oper);
                dataCtx.SubmitChanges();
                return result;
            }
            
            

            string calculatedMD5 = HashUtility.CalculateMD5(sessionId, clientAccountId, ctrgAccountId, ctrgClientAccountId, contractBindId, externalDocDate, externalDocNumber, feeAmount, functionType, paymentDetails, transactAmount);
            if (!calculatedMD5.Equals(checksum))
            {
                operationresult result =new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
                oper.Result = IntegerConstants.WRONG_CHECKSUM;
                oper.ResultNote = "неправильная контрольная сумма";
                oper.ResultLog = resultLog.getLog();
                dataCtx.Operations.InsertOnSubmit(oper);
                dataCtx.SubmitChanges();
                return result;
            }
            
            DateTime now = DateTime.Now;
            if( !(externalDate.CompareTo(now.AddMinutes(10)) < 0 && (externalDate.CompareTo(now.AddMinutes(-10)) > 0)) ){
                operationresult result =new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", resultLog.getLog());
                oper.Result = IntegerConstants.WRONG_TIME;
                oper.ResultNote = "неправильное время";
                oper.ResultLog = resultLog.getLog();
                dataCtx.Operations.InsertOnSubmit(oper);
                dataCtx.SubmitChanges();
                return result;
            }
            
            
            operationresult resultS =new operationresult(IntegerConstants.SUCCESS, "Успешно", resultLog.getLog());
            oper.Result = IntegerConstants.SUCCESS;
            oper.ResultNote = "Успешно";
            oper.ResultLog = resultLog.getLog();
            dataCtx.Operations.InsertOnSubmit(oper);
            dataCtx.SubmitChanges();
            return resultS;
        }

        public operationresult IsOperationSuccessful(long sessionId, string curDate, string checksum)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.IsOperationSuccessful()");

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
            if (!(externalDate.CompareTo(now.AddMinutes(10)) < 0 && (externalDate.CompareTo(now.AddMinutes(-10)) > 0)))
            {
                return new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", resultLog.getLog());
            }

            ////now get it from the database
            LoggingLayer.Operation oper = dataCtx.Operations.Where(t => t.SessionID == sessionId).First();
            if (oper == null)
            {
                resultLog.AddToLog("search by sessionId gave no result");
                return new operationresult(IntegerConstants.WRONG_PARAMS, "неправильный sessionID", resultLog.getLog());
            }
            else
            {
                return new operationresult(oper.Result.Value, oper.ResultNote, oper.ResultLog);
            }         
        }

        public operationresult GetOperationsStatus(Nullable<int> result, string from, string to, string curDate, string checksum)
        {
            LogAssembler resultLog = new LogAssembler();
            resultLog.AddToLog("starting OperationPerformer.GetOperationsStatus()");

            int res = 0;
            if (result.HasValue)
                res = result.Value;

            DateTime externalDate = DateTime.Now; 
            DateTime toDate = DateTime.Now;
            DateTime fromDate = DateTime.Now;

            try{
                externalDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                toDate=DateTime.ParseExact(to, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                fromDate=DateTime.ParseExact(from, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            }
            catch (System.FormatException fe)
            {
                resultLog.AddToLog("OperationPerformer.GetOperationsStatus(). DateTime FormatException: " + fe.Message);
                operationresult resultO = new operationresult(IntegerConstants.WRONG_PARAMS, "не указаны необходимые параметры запроса", resultLog.getLog());
                return resultO;
            }
            DateTime now = DateTime.Now;
            if (!(externalDate.CompareTo(now.AddMinutes(10)) < 0 && (externalDate.CompareTo(now.AddMinutes(-10)) > 0)))
            {
                return new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", resultLog.getLog());
            }
            string calculatedMD5 = HashUtility.CalculateMD5(curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                return new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", resultLog.getLog());
            }
            
            LoggingLayer.Operation[] opers;
            if (res != 0 && (from == null || to == null))
                opers = dataCtx.Operations.Where(t => t.Result == res).ToArray();
            
            else if (res == 0 && from != null && to != null)
                opers = dataCtx.Operations.Where( t => (t.CurrentDate.Value.CompareTo(toDate)<0 && t.CurrentDate.Value.CompareTo(fromDate)>0) ).ToArray();
            else if (res != 0 && from != null && to != null)
                opers = dataCtx.Operations.Where(t => (t.CurrentDate.Value.CompareTo(toDate) < 0 && t.CurrentDate.Value.CompareTo(fromDate) > 0) && t.Result == res).ToArray();
            else
            {
                
                return new operationresult(IntegerConstants.WRONG_PARAMS, "неправильные параметры - чего-то не хватает", resultLog.getLog());
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
    }
}
