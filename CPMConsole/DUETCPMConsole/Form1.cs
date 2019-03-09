using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Globalization;

namespace DUETCPMConsole
{
    public partial class CPMPAMainForm : Form
    {
        private TcpListener listener;
        private ArrayList sockets = new ArrayList();
        private bool isListening = false;
        private Thread listenerThread;
        string text;
        private DateTime beginTime;
        private ContextMenu trayMenu;
        private string status;
        public CPMPAMainForm()
        {
            InitializeComponent();
            beginTime = DateTime.Now;
            listener = new TcpListener(IPAddress.Any, 5050);

            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Показать", notifyIconDUET_DoubleClick);
            notifyIconDUET.ContextMenu = trayMenu;
            //notifyIconDUET.Visible = false;

            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            this.Resize += new EventHandler(Form1_Resize);
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            status = "CPMPAMainForm initialized";
            btnStopDuet_Click(null, null);
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            MoveToTray();

        }

        void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
            {
                MoveToTray();
            }
        }

        private void MoveToTray()
        {
            Hide();
            notifyIconDUET.Visible = true;
        }

        public void showOnScreen(object o, EventArgs e)
        {
            //
            txtLogs.Text += text + "\n";
        }

        private void btnStopDuet_Click(object sender, EventArgs e)
        {
            status += "btnStopDuet_Click performed";
            if (btnStopDuet.Text.Equals("Start"))
            {
                btnStopDuet.Text = "Stop";
                btnStopDuet.BackColor = System.Drawing.Color.FromArgb(Convert.ToByte(0), Convert.ToByte(192), Convert.ToByte(0));


                isListening = true;
                listenerThread = new Thread(new ThreadStart(listenToPort));
                listener.Start();
                listenerThread.Start();
            }
            else
            {
                btnStopDuet.Text = "Start";
                btnStopDuet.BackColor = System.Drawing.Color.FromArgb(Convert.ToByte(255), Convert.ToByte(192), Convert.ToByte(192));

                try
                {
                    isListening = false;
                    listener.Stop();
                    listenerThread.Interrupt();
                    listenerThread.Abort();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
        }

        private void listenToPort()
        {
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
                    Console.WriteLine(ex.Message);
                }
            }
            status += "listenToPorts() was called";
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
                //showOnScreen(response);
                object[] pList = { this, new EventArgs()};
                text = response;
                txtLogs.BeginInvoke(new System.EventHandler(showOnScreen));

                //convertToData(response);
                operationresult res = performOperation(response);

                StringBuilder builder = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(builder);
                new XmlSerializer(typeof(operationresult)).Serialize(writer, res);
                
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
                object[] pList = { this, new EventArgs() };
                text = ex.Message;
                txtLogs.BeginInvoke(new System.EventHandler(showOnScreen));
            }
            status += "handleCommunication() was called";
        }

        private operationresult performOperation(string xmlStr)
        {
            status += "performOperation() was called";
            try
            {
                TextReader txtReader = new StringReader(xmlStr);
                request mcmessage;
                XmlSerializer serializer = new XmlSerializer(typeof(request));
                mcmessage = (request)serializer.Deserialize(txtReader);

                if (mcmessage.duetOper != null)
                {
                    return OperationPerformer.Instance().PerformDuetOperation(mcmessage.duetOper.sessionID, mcmessage.duetOper.clientAccountID
                        , mcmessage.duetOper.counterAgentAccountID, mcmessage.duetOper.counterAgentClientAccountID
                        , mcmessage.duetOper.contractBindID, mcmessage.duetOper.externalDocDate, mcmessage.duetOper.externalDocNumber
                        , mcmessage.duetOper.feeAmount, mcmessage.duetOper.functionType
                        , mcmessage.duetOper.paymentDetails, mcmessage.duetOper.transactAmount, mcmessage.duetOper.checksum);
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
            }
            catch
            {
                return new operationresult(IntegerConstants.WRONG_PARAMS, "не удалось конвертировать запрос. неправильный формат.", "performOperation(string)");
            }
            return new operationresult(IntegerConstants.ERROR, "неизвестная ошибка.", "performOperation(string)");
        }

        private operationresult HowAreYou(string curDate, string checksum)
        {
            status += "HowAreYou() was called";
            string calculatedMD5 = HashUtility.CalculateMD5(curDate);
            if (!calculatedMD5.Equals(checksum))
            {
                return new operationresult(IntegerConstants.WRONG_CHECKSUM, "неправильная контрольная сумма", "Form1.HowAreYou()");
            }
            DateTime externalDate = DateTime.ParseExact(curDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            DateTime now = DateTime.Now;
            if (!(externalDate.CompareTo(now.AddMinutes(10)) < 0 && (externalDate.CompareTo(now.AddMinutes(-10)) > 0)))
            {
                return new operationresult(IntegerConstants.WRONG_TIME, "неправильное время", "Form1.HowAreYou()");
            }

            long diff = DateTime.Now.Ticks - beginTime.Ticks;
            TimeSpan span = new TimeSpan(diff);
            string livetime = ""+(int)span.TotalHours + ":" + span.Minutes;

            operationresult result = new operationresult();
            result.result = IntegerConstants.SUCCESS;
            result.status = "1";
            result.statusNote = "Сервис готов к работе!";
            result.livetime = livetime;
            return result;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            try
            {
                isListening = false;
                listener.Stop();
                listenerThread.Interrupt();
                listenerThread.Abort();
            }
            catch { }
        }

        private void notifyIconDUET_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            //notifyIconDUET.Visible = false;
            status += "notifyIconDUET_DoubleClick() was called";
        }

        public string GetStatus()
        {
            return status;
        }

        public void Show() 
        {
            status += "Show() was called";
            base.Show();
        }
    }
}
