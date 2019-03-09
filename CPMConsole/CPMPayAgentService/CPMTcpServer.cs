using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Net;
using System.Configuration;
using DUETCPMConsole;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Globalization;
using CPMPayAgentService.Utility;
using Bgs.Duet.Exchange.Client.Proxy;

namespace CPMPayAgentService
{
    public class CPMTcpServer : IErrorReporter
    {
        private TcpListener listener;
        private ArrayList sockets = new ArrayList();
        private bool isListening = false;
        private Thread listenerThread;
        private DateTime beginTime;
        private string status;

        public event EventHandler<CPMErrorEventArgs> ErrorOccurred;

        private static CPMTcpServer _instance;

        private CPMTcpServer()
        {
            int port = CPMPayAgentService.Properties.Settings.Default.LocalTcpPort;
            beginTime = DateTime.Now;
            listener = new TcpListener(IPAddress.Any, port);

            status = "CPMTcpServer initialized. ";
        }

        public static CPMTcpServer Instance()
        {
            if (_instance == null)
                _instance = new CPMTcpServer();

            return _instance;
        }

        public void StartListener()
        {
            if(!isListening)
                status += "Starting TCP listener. ";
            
            isListening = true;
            listenerThread = new Thread(new ThreadStart(listenToPort));
            try
            {
                listener.Start();
                listenerThread.Start();
                string result = OperationPerformer.Instance().Initialize(this);
                status += result;
            }
            catch (SocketException ex)
            {
                if (ErrorOccurred != null)
                {
                    CPMErrorEventArgs args = new CPMErrorEventArgs();
                    args.Message = ex.Message;
                    args.time = DateTime.Now;
                    ErrorOccurred(this, args);
                }

                status = "Error occurred when starting TCP listener: "+ex.Message;
                isListening = false;
                return;
            }
            
            
        }

        public void StopListener()
        {
            if (!isListening)
                return;
            
            try
            {
                isListening = false;
                listener.Stop();
                listenerThread.Interrupt();
                listenerThread.Abort();
            }
            catch (Exception ex) {
                lock (this)
                {
                    if (ErrorOccurred != null)
                    {
                        CPMErrorEventArgs err = new CPMErrorEventArgs();
                        err.Message = "Error: "+ex.Message;
                        ErrorOccurred(this, err);
                    }
                }
            }
        }

        private void listenToPort()
        {
            status += "listenToPort() was called. ";
            while (isListening)
            {
                try
                {
                    Socket skt = listener.AcceptSocket();
                    lock (sockets)
                    {
                        sockets.Add(skt);
                    }
                    new System.Threading.Thread(new System.Threading.ThreadStart(handleCommunication)).Start();
                }
                catch (Exception ex)
                {
                    if(ErrorOccurred != null){
                        CPMErrorEventArgs args = new CPMErrorEventArgs();
                        args.Message = ex.Message;
                        args.time = DateTime.Now;
                        ErrorOccurred(this,  args);
                    }
                }
            }
        }

        private void handleCommunication()
        {
            try
            {
                System.Net.Sockets.Socket handlerSocket;
                lock (sockets)
                {
                    handlerSocket = (Socket)sockets[(sockets.Count - 1)]; // get the last socket that was added
                    sockets.RemoveAt((sockets.Count - 1));
                }
                System.Net.Sockets.NetworkStream networkStream = new System.Net.Sockets.NetworkStream(handlerSocket); //' get the stream from that socket
                BinaryReader reader = new BinaryReader(networkStream);
                int length = reader.ReadInt32();
                byte[] bytesToRead = reader.ReadBytes(length);
                String response = System.Text.Encoding.UTF8.GetString(bytesToRead);

                //CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError(response);
                operationresult res = performOperation(response, handlerSocket.RemoteEndPoint.ToString());

                StringBuilder builder = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(builder);
                new XmlSerializer(typeof(operationresult)).Serialize(writer, res);
                //CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError(builder.ToString());
                byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
                BinaryWriter binWriter = new BinaryWriter(networkStream);
                binWriter.Write(bytesToSend.Length);
                binWriter.Write(bytesToSend);
                binWriter.Flush();
                binWriter.Close();
                handlerSocket.Close();
            }
            catch (Exception ex)
            {
                ////TODO: need to send it to named pipe as well
                if (ErrorOccurred != null)
                {
                    CPMErrorEventArgs args = new CPMErrorEventArgs();
                    args.Message = "PerformOperation Error: "+ex.Message;
                    args.time = DateTime.Now;
                    ErrorOccurred(this, args);
                }

                status = "handleCommunication() error occurred: "+ex.Message;
            }
            status = "handleCommunication() was called. ";
        }

        private operationresult performOperation(string xmlStr, string endPoint)
        {
            status = "performOperation() was called. ";
            try
            {
                TextReader txtReader = new StringReader(xmlStr);
                request mcmessage;
                XmlSerializer serializer = new XmlSerializer(typeof(request));
                mcmessage = (request)serializer.Deserialize(txtReader);
                CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError("Message deserialized. "+mcmessage.ToString()+ "\r\n" +xmlStr);
                if (mcmessage.duetOper != null)
                {
                    return OperationPerformer.Instance().PerformDuetOperation(mcmessage.duetOper.sessionID, mcmessage.duetOper.clientAccountID
                        , mcmessage.duetOper.counterAgentAccountID, mcmessage.duetOper.counterAgentClientAccountID
                        , mcmessage.duetOper.contractBindID, mcmessage.duetOper.externalDocDate, mcmessage.duetOper.externalDocNumber
                        , mcmessage.duetOper.feeAmount, mcmessage.duetOper.functionType
                        , mcmessage.duetOper.paymentDetails, mcmessage.duetOper.transactAmount, mcmessage.duetOper.checksum, mcmessage.duetOper.curDate, endPoint, this);
                }

                else if (mcmessage.operSuccess != null)
                {
                    return OperationPerformer.Instance().IsOperationSuccessful(mcmessage.operSuccess.sessionid, mcmessage.operSuccess.curdate, mcmessage.operSuccess.checksum);
                }
                else if (mcmessage.howYou != null)
                {
                    return HowAreYou(mcmessage.howYou.curdate, mcmessage.howYou.checksum);
                }
                else if (mcmessage.operStatus != null)
                {
                    return OperationPerformer.Instance().GetOperationsStatus(mcmessage.operStatus.result, mcmessage.operStatus.from, mcmessage.operStatus.to, mcmessage.operStatus.curdate, mcmessage.operStatus.checksum);
                }
                else if (mcmessage.confirmduetoperation != null)
                {
                    CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError("Message deserialized - ConfirmOperation");
                    return OperationPerformer.Instance().ConfirmDuetOperation(mcmessage.confirmduetoperation.sessionID
                        , mcmessage.confirmduetoperation.externalDocDate, mcmessage.confirmduetoperation.externalDocNumber
                        , mcmessage.confirmduetoperation.curDate, endPoint, this);
                }
                else if (mcmessage.prepareduetoperation!= null)
                {
                    CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError("Message deserialized - PrepareOperation");
                    return OperationPerformer.Instance().PrepareDuetOperation(mcmessage.prepareduetoperation.sessionID, mcmessage.prepareduetoperation.clientAccountID
                        , mcmessage.prepareduetoperation.counterAgentAccountID, mcmessage.prepareduetoperation.counterAgentClientAccountID
                        , mcmessage.prepareduetoperation.contractBindID, mcmessage.prepareduetoperation.externalDocDate, mcmessage.prepareduetoperation.externalDocNumber
                        , mcmessage.prepareduetoperation.feeAmount, mcmessage.prepareduetoperation.functionType
                        , mcmessage.prepareduetoperation.paymentDetails, mcmessage.prepareduetoperation.transactAmount, mcmessage.prepareduetoperation.checksum, mcmessage.prepareduetoperation.curDate, endPoint, this);
                }
                else if (mcmessage.cancelduetoperation != null)
                {
                    return OperationPerformer.Instance().CancelDuetOperation(mcmessage.cancelduetoperation.sessionID
                        , mcmessage.cancelduetoperation.externalDocDate, mcmessage.cancelduetoperation.externalDocNumber
                        , mcmessage.cancelduetoperation.curDate, endPoint, this);
                }
                else if (mcmessage.blockcardaccount != null)
                {
                    return OperationPerformer.Instance().BlockCardAccount(mcmessage.blockcardaccount.sessionid, mcmessage.blockcardaccount.clientAccountID, mcmessage.blockcardaccount.blockType, mcmessage.blockcardaccount.blockAction,
                        mcmessage.blockcardaccount.reason, mcmessage.blockcardaccount.checksum,
                        mcmessage.blockcardaccount.curDate, endPoint);
                }
                else if (mcmessage.emvload != null)
                {
                    return OperationPerformer.Instance().EMVLoadOperation(mcmessage.emvload.sessionid, mcmessage.emvload.amount, mcmessage.emvload.emvaccountid, mcmessage.emvload.externalDocDate, mcmessage.emvload.externalDocNumber,
                        mcmessage.emvload.checksum, mcmessage.emvload.curDate, endPoint);
                }
                else if (mcmessage.emvunload != null)
                {
                    return OperationPerformer.Instance().EMVUnLoadOperation(mcmessage.emvunload.sessionid, mcmessage.emvunload.amount, mcmessage.emvunload.emvaccountid, mcmessage.emvunload.externalDocDate,
                        mcmessage.emvunload.externalDocNumber, mcmessage.emvunload.checksum, mcmessage.emvunload.curDate, endPoint);
                }
                else if (mcmessage.openclient != null)
                {
                    return OperationPerformer.Instance().OpenClientOperation(mcmessage.openclient.sessionid, mcmessage.openclient.name, mcmessage.openclient.address, mcmessage.openclient.index, mcmessage.openclient.bankid
                        , mcmessage.openclient.participantid, mcmessage.openclient.region, mcmessage.openclient.clientClass
                        , mcmessage.openclient.contractNumber, mcmessage.openclient.contractDate, mcmessage.openclient.taxID1
                        ,mcmessage.openclient.taxID2, mcmessage.openclient.INN, mcmessage.openclient.OKPO, mcmessage.openclient.checksum,
                        mcmessage.openclient.curDate, endPoint);
                }
                else if (mcmessage.openaccount != null)
                {
                    return OperationPerformer.Instance().OpenAccountOperation(mcmessage.openaccount.sessionid, mcmessage.openaccount.clientDataId, mcmessage.openaccount.accountSubNumber, mcmessage.openaccount.counterAgentAccountID
                        , mcmessage.openaccount.extAccountNumber, mcmessage.openaccount.externalDocNumber, mcmessage.openaccount.cPart
                        , mcmessage.openaccount.checksum, mcmessage.openaccount.curDate, endPoint);
                }
                else if (mcmessage.emvcardblockunblock != null)
                {
                    return OperationPerformer.Instance().BlockUnblockEMVCardAccount(mcmessage.emvcardblockunblock.sessionid, mcmessage.emvcardblockunblock.clientdataid, mcmessage.emvcardblockunblock.emvaccountid, 
                        mcmessage.emvcardblockunblock.emvcardid, mcmessage.emvcardblockunblock.reason, mcmessage.emvcardblockunblock.checksum, mcmessage.emvcardblockunblock.curDate, endPoint);
                }
            }
            catch(Exception ex)
            {
                return new operationresult(IntegerConstants.WRONG_PARAMS, "не удалось конвертировать запрос. неправильный формат.", "performOperation(string)" + ex.Message + " "+ex.StackTrace);
            }
            return new operationresult(IntegerConstants.ERROR, "неизвестная ошибка.", "performOperation(string)");
        }

        private operationresult HowAreYou(string curDate, string checksum)
        {
            status = "HowAreYou() was called";
            string calculatedMD5 = HashUtility.CalculateMD5(curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                return new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", "CPMTcpServer.HowAreYou()");
            }
            DateTime externalDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            DateTime now = DateTime.Now;
            CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError("HOWYOU Now: " + now.ToString() + " curdate: " + externalDate.ToString() +
               (externalDate.CompareTo(now.AddMinutes(15)) < 0) + " " + (externalDate.CompareTo(now.AddMinutes(-15)) > 0));
            if (!(externalDate.CompareTo(now.AddMinutes(15)) < 0 && (externalDate.CompareTo(now.AddMinutes(-15)) > 0)))
            {
                return new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", "Server.HowAreYou()");
            }

            long diff = DateTime.Now.Ticks - beginTime.Ticks;
            TimeSpan span = new TimeSpan(diff);
            string livetime = "" + span.Days + "d " + (int)span.Hours + "h " + span.Minutes + "m ";

            operationresult result = new operationresult();

            RegistrationResultItem item;
            try
            {
                item = OperationPerformer.Instance().getRegistrationResult();
            }
            catch(Exception e)
            {
                result.result = IntegerConstants.SUCCESS;
                result.status = "-500";
                result.statusNote = "Сервис не готов к работе! "+e.Message;
                result.livetime = livetime;
                return result;
            }

            switch (item)
            {
                case RegistrationResultItem.RegistrationFailed:
                    result.result = IntegerConstants.SUCCESS;
                    result.status = "-1";
                    result.statusNote = "Сервис не готов к работе!";
                    break;
                case RegistrationResultItem.RegistrationSuccefully:
                    result.result = IntegerConstants.SUCCESS;
                    result.status = "1";
                    result.statusNote = "Сервис готов к работе!";
                    break;
                case RegistrationResultItem.Unknown:
                    result.result = IntegerConstants.SUCCESS;
                    result.status = "-500";
                    result.statusNote = "Сервис не готов к работе!";
                    break;
                default:
                    break;
            }
            
            result.livetime = livetime;
            return result;
        }

        public string GetStatus()
        {
            return status;
        }

        #region IErrorReporter Members

        public void reportError(string error)
        {
            if (ErrorOccurred != null)
            {
                CPMErrorEventArgs err = new CPMErrorEventArgs();
                err.Message = error;
                ErrorOccurred(this, err);
            }
        }

        #endregion
    }

    public class CPMErrorEventArgs : EventArgs
    {
        public DateTime time;
        public string Message;
        public string Tip;
        public int code;
    }
}
