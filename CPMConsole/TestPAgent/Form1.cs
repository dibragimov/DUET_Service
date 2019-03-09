using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DUETCPMConsole;
using System.Xml;
using CPMPaymentsService.BLL;
using CPMPaymentsService.Logging;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCreateClient_Click(object sender, EventArgs e)
        {
            ServerResult res = new ServerResult() { Message = string.Empty };
            try
            {

                string curDateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                openclient ocrequest = new openclient()
                {
                    sessionid = txtCCSessionID.Text,
                    address = txtCCAddress.Text,
                    bankid = Int32.Parse(txtCCBankID.Text),
                    clientClass = Int32.Parse(txtCCClientClass.Text),
                    contractDate = dtPickerCCContractDate.Value,
                    contractNumber = txtCCContractNum.Text,
                    curDate = curDateStr,
                    index = txtCCIndex.Text,
                    INN = txtCCINN.Text,
                    name = txtCCName.Text,
                    OKPO = txtCCOKPO.Text,
                    participantid = Int32.Parse(txtCCParticipantID.Text),
                    region = Int32.Parse(txtCCRegion.Text),
                    taxID1 = txtCCTaxID1.Text,
                    taxID2 = txtCCTaxID2.Text
                };
                ocrequest.checksum = CPMPaymentsService.Utility.HashUtility.CalculateMD5(txtCCSessionID.Text, curDateStr);

                request request = new request() { openclient = ocrequest };
                operationresult result = null;
                StringBuilder builder = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(builder);
                new System.Xml.Serialization.XmlSerializer(typeof(request)).Serialize(writer, request);

                CPMPaymentLogging.Instance().logXML("openClient() XML to send: " + builder.ToString());

                // commented for test puproses
                System.Net.Sockets.Socket handlerSocket =
                  new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                handlerSocket.Connect(CPMPaymentsService.Configuration.Settings.IP, CPMPaymentsService.Configuration.Settings.Port);
                if (handlerSocket.Connected)
                {
                    System.Net.Sockets.NetworkStream networkStream = new System.Net.Sockets.NetworkStream(handlerSocket); //' get the stream from that socket
                    byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
                    BinaryWriter binWriter = new BinaryWriter(networkStream);
                    binWriter.Write(bytesToSend.Length);
                    binWriter.Write(bytesToSend);
                    binWriter.Flush();

                    BinaryReader reader = new BinaryReader(networkStream);
                    int length = reader.ReadInt32();
                    byte[] bytesToRead = reader.ReadBytes(length);
                    String response = System.Text.Encoding.UTF8.GetString(bytesToRead);

                    TextReader txtReader = new StringReader(response);
                    result = (operationresult)(new System.Xml.Serialization.XmlSerializer(typeof(operationresult))).Deserialize(txtReader);
                    res.Result = result;
                    binWriter.Close();
                    handlerSocket.Close();
                    richTBCCAnswer.Text = txtReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
        }

        private void btnOACreate_Click(object sender, EventArgs e)
        {
            ServerResult res = new ServerResult() { Message = string.Empty };
            try
            {

                string curDateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                openaccount ocrequest = new openaccount()
                {
                    sessionid = txtCCSessionID.Text,
                    accountSubNumber = Int32.Parse(txtCAAccSubNum.Text),
                    clientDataId = Int32.Parse(txtCACliDataID.Text),
                    counterAgentAccountID = Int32.Parse(txtCACtrAgentAccID.Text),
                    cPart = Int32.Parse(txtCACPart.Text),
                    curDate = curDateStr,
                    extAccountNumber = txtCAExtAccNum.Text,
                    externalDocNumber = txtCAExtDocNum.Text
                };
                ocrequest.checksum = CPMPaymentsService.Utility.HashUtility.CalculateMD5(txtCCSessionID.Text, curDateStr);

                request request = new request() { openaccount = ocrequest };
                operationresult result = null;
                StringBuilder builder = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(builder);
                new System.Xml.Serialization.XmlSerializer(typeof(request)).Serialize(writer, request);

                CPMPaymentLogging.Instance().logXML("openAccount() XML to send: " + builder.ToString());

                // commented for test puproses
                System.Net.Sockets.Socket handlerSocket =
                  new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                handlerSocket.Connect(CPMPaymentsService.Configuration.Settings.IP, CPMPaymentsService.Configuration.Settings.Port);
                if (handlerSocket.Connected)
                {
                    System.Net.Sockets.NetworkStream networkStream = new System.Net.Sockets.NetworkStream(handlerSocket); //' get the stream from that socket
                    byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
                    BinaryWriter binWriter = new BinaryWriter(networkStream);
                    binWriter.Write(bytesToSend.Length);
                    binWriter.Write(bytesToSend);
                    binWriter.Flush();

                    BinaryReader reader = new BinaryReader(networkStream);
                    int length = reader.ReadInt32();
                    byte[] bytesToRead = reader.ReadBytes(length);
                    String response = System.Text.Encoding.UTF8.GetString(bytesToRead);

                    TextReader txtReader = new StringReader(response);
                    result = (operationresult)(new System.Xml.Serialization.XmlSerializer(typeof(operationresult))).Deserialize(txtReader);
                    res.Result = result;
                    binWriter.Close();
                    handlerSocket.Close();
                    richTBCCAnswer.Text = txtReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
        }

        private void btnBCACreate_Click(object sender, EventArgs e)
        {
            ServerResult res = new ServerResult() { Message = string.Empty };
            try
            {

                string curDateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                blockcardaccount ocrequest = new blockcardaccount()
                {
                    sessionid = txtCCSessionID.Text,
                    blockAction = Int32.Parse(txtBCABlockAction.Text),
                    blockType = Int32.Parse(txtBCABlockType.Text),
                    clientAccountID = Int32.Parse(txtBCACliAccID.Text),
                    reason = txtBCAReason.Text
                };
                ocrequest.checksum = CPMPaymentsService.Utility.HashUtility.CalculateMD5(txtCCSessionID.Text, curDateStr);

                request request = new request() { blockcardaccount = ocrequest };
                operationresult result = null;
                StringBuilder builder = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(builder);
                new System.Xml.Serialization.XmlSerializer(typeof(request)).Serialize(writer, request);

                CPMPaymentLogging.Instance().logXML("BlockCardAccount() XML to send: " + builder.ToString());

                // commented for test puproses
                System.Net.Sockets.Socket handlerSocket =
                  new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                handlerSocket.Connect(CPMPaymentsService.Configuration.Settings.IP, CPMPaymentsService.Configuration.Settings.Port);
                if (handlerSocket.Connected)
                {
                    System.Net.Sockets.NetworkStream networkStream = new System.Net.Sockets.NetworkStream(handlerSocket); //' get the stream from that socket
                    byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
                    BinaryWriter binWriter = new BinaryWriter(networkStream);
                    binWriter.Write(bytesToSend.Length);
                    binWriter.Write(bytesToSend);
                    binWriter.Flush();

                    BinaryReader reader = new BinaryReader(networkStream);
                    int length = reader.ReadInt32();
                    byte[] bytesToRead = reader.ReadBytes(length);
                    String response = System.Text.Encoding.UTF8.GetString(bytesToRead);

                    TextReader txtReader = new StringReader(response);
                    result = (operationresult)(new System.Xml.Serialization.XmlSerializer(typeof(operationresult))).Deserialize(txtReader);
                    res.Result = result;
                    binWriter.Close();
                    handlerSocket.Close();
                    richTBBCAAnswer.Text = txtReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
        }

        private void btnEMVLoad_Click(object sender, EventArgs e)
        {
            ServerResult res = new ServerResult() { Message = string.Empty };
            try
            {

                string curDateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                emvload ocrequest = new emvload()
                {
                    sessionid = txtCCSessionID.Text,
                    amount = Decimal.Parse(txtEMVLoadAmount.Text),
                    emvaccountid = txtEMVLoadAccID.Text,
                    externalDocDate = dtPickerEMVLoadExtDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    externalDocNumber = txtEMVLoadExtDocNum.Text
                };
                ocrequest.checksum = CPMPaymentsService.Utility.HashUtility.CalculateMD5(txtCCSessionID.Text, curDateStr);

                request request = new request() { emvload = ocrequest };
                operationresult result = null;
                StringBuilder builder = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(builder);
                new System.Xml.Serialization.XmlSerializer(typeof(request)).Serialize(writer, request);

                CPMPaymentLogging.Instance().logXML("EMVLoad() XML to send: " + builder.ToString());

                // commented for test puproses
                System.Net.Sockets.Socket handlerSocket =
                  new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                handlerSocket.Connect(CPMPaymentsService.Configuration.Settings.IP, CPMPaymentsService.Configuration.Settings.Port);
                if (handlerSocket.Connected)
                {
                    System.Net.Sockets.NetworkStream networkStream = new System.Net.Sockets.NetworkStream(handlerSocket); //' get the stream from that socket
                    byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
                    BinaryWriter binWriter = new BinaryWriter(networkStream);
                    binWriter.Write(bytesToSend.Length);
                    binWriter.Write(bytesToSend);
                    binWriter.Flush();

                    BinaryReader reader = new BinaryReader(networkStream);
                    int length = reader.ReadInt32();
                    byte[] bytesToRead = reader.ReadBytes(length);
                    String response = System.Text.Encoding.UTF8.GetString(bytesToRead);

                    TextReader txtReader = new StringReader(response);
                    result = (operationresult)(new System.Xml.Serialization.XmlSerializer(typeof(operationresult))).Deserialize(txtReader);
                    res.Result = result;
                    binWriter.Close();
                    handlerSocket.Close();
                    richTBEMVLoadAnswer.Text = txtReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
        }

        private void btnEMVUnload_Click(object sender, EventArgs e)
        {
            ServerResult res = new ServerResult() { Message = string.Empty };
            try
            {

                string curDateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                emvunload ocrequest = new emvunload()
                {
                    sessionid = txtCCSessionID.Text,
                    amount = Decimal.Parse(txtEMVLoadAmount.Text),
                    emvaccountid = txtEMVLoadAccID.Text,
                    externalDocDate = dtPickerEMVLoadExtDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    externalDocNumber = txtEMVLoadExtDocNum.Text
                };
                ocrequest.checksum = CPMPaymentsService.Utility.HashUtility.CalculateMD5(txtCCSessionID.Text, curDateStr);

                request request = new request() { emvunload = ocrequest };
                operationresult result = null;
                StringBuilder builder = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(builder);
                new System.Xml.Serialization.XmlSerializer(typeof(request)).Serialize(writer, request);

                CPMPaymentLogging.Instance().logXML("EMVUnload() XML to send: " + builder.ToString());

                // commented for test puproses
                System.Net.Sockets.Socket handlerSocket =
                  new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                handlerSocket.Connect(CPMPaymentsService.Configuration.Settings.IP, CPMPaymentsService.Configuration.Settings.Port);
                if (handlerSocket.Connected)
                {
                    System.Net.Sockets.NetworkStream networkStream = new System.Net.Sockets.NetworkStream(handlerSocket); //' get the stream from that socket
                    byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
                    BinaryWriter binWriter = new BinaryWriter(networkStream);
                    binWriter.Write(bytesToSend.Length);
                    binWriter.Write(bytesToSend);
                    binWriter.Flush();

                    BinaryReader reader = new BinaryReader(networkStream);
                    int length = reader.ReadInt32();
                    byte[] bytesToRead = reader.ReadBytes(length);
                    String response = System.Text.Encoding.UTF8.GetString(bytesToRead);

                    TextReader txtReader = new StringReader(response);
                    result = (operationresult)(new System.Xml.Serialization.XmlSerializer(typeof(operationresult))).Deserialize(txtReader);
                    res.Result = result;
                    binWriter.Close();
                    handlerSocket.Close();
                    richTBEMVUnloadAnswer.Text = txtReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
        }

        private void btnEMVBlockUnblock_Click(object sender, EventArgs e)
        {
            ServerResult res = new ServerResult() { Message = string.Empty };
            try
            {

                string curDateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                emvcardblockunblock ocrequest = new emvcardblockunblock()
                {
                    sessionid = txtCCSessionID.Text,
                    clientdataid = Int32.Parse(txtEMVBlockUnblockCliDataID.Text),
                    emvaccountid = Int32.Parse(txtEMVBlockUnblockAccID.Text),
                    emvcardid = Int32.Parse(txtEMVBlockUnblockCardID.Text),
                    reason = Int32.Parse(txtEMVBlockUnblockReason.Text)
                };
                ocrequest.checksum = CPMPaymentsService.Utility.HashUtility.CalculateMD5(txtCCSessionID.Text, curDateStr);

                request request = new request() { emvcardblockunblock = ocrequest };
                operationresult result = null;
                StringBuilder builder = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(builder);
                new System.Xml.Serialization.XmlSerializer(typeof(request)).Serialize(writer, request);

                CPMPaymentLogging.Instance().logXML("EMVcardBlockUnblock() XML to send: " + builder.ToString());

                // commented for test puproses
                System.Net.Sockets.Socket handlerSocket =
                  new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                handlerSocket.Connect(CPMPaymentsService.Configuration.Settings.IP, CPMPaymentsService.Configuration.Settings.Port);
                if (handlerSocket.Connected)
                {
                    System.Net.Sockets.NetworkStream networkStream = new System.Net.Sockets.NetworkStream(handlerSocket); //' get the stream from that socket
                    byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
                    BinaryWriter binWriter = new BinaryWriter(networkStream);
                    binWriter.Write(bytesToSend.Length);
                    binWriter.Write(bytesToSend);
                    binWriter.Flush();

                    BinaryReader reader = new BinaryReader(networkStream);
                    int length = reader.ReadInt32();
                    byte[] bytesToRead = reader.ReadBytes(length);
                    String response = System.Text.Encoding.UTF8.GetString(bytesToRead);

                    TextReader txtReader = new StringReader(response);
                    result = (operationresult)(new System.Xml.Serialization.XmlSerializer(typeof(operationresult))).Deserialize(txtReader);
                    res.Result = result;
                    binWriter.Close();
                    handlerSocket.Close();
                    richTBEMVBUAnswer.Text = txtReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
            }
        }
    }
}
