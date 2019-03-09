using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace CPMPayAgentService
{
    public partial class CPMDUETPayAgentService : ServiceBase
    {
        private CPMTcpServer server;
        public CPMDUETPayAgentService()
        {
            InitializeComponent();
            AutoLog = true;
            if (!System.Diagnostics.EventLog.SourceExists("CPMSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "CPMSource", "CPMLog");
            }
            eventLogCPM.Source = "CPMSource";
            eventLogCPM.Log = "CPMLog";

        }

        protected override void OnStart(string[] args)
        {
            eventLogCPM.WriteEntry("In OnStart. Listener to be started...");
            //eventLogCPM.WriteEntry("Listener to be started: ");
            //DUETCPMConsole.CPMPAMainForm form = new DUETCPMConsole.CPMPAMainForm();
            //form.Show();
           
            //System.Diagnostics.Process.Start("C:\\Documents and Settings\\Dilshod\\My Documents\\Visual Studio 2008\\Projects\\DUETCPMConsole\\CPMPayAgentService\\bin\\Debug\\CPMPayAgentService.exe"); 
            server = CPMTcpServer.Instance();
            server.StartListener();
            eventLogCPM.WriteEntry("Listener started: " + CPMTcpServer.Instance().GetStatus());
            CPMTcpServer.Instance().ErrorOccurred += new EventHandler<CPMErrorEventArgs>(OnErrorOccurred);

            //eventLogCPM.WriteEntry(CPMTcpServer.Instance().GetStatus());//"Main Console is shown");
            //TestForm form = new TestForm();
            //form.Show();
            //Application.Run(form);
            //eventLogCPM.WriteEntry(CPMTcpServer.Instance().GetStatus());
        }

        protected override void OnStop()
        {
            server.StopListener();
            CPMTcpServer.Instance().ErrorOccurred -= new EventHandler<CPMErrorEventArgs>(OnErrorOccurred);
            eventLogCPM.WriteEntry("In OnStop - stopping listener.");
        }

        private void OnErrorOccurred(object sender, CPMErrorEventArgs args)
        {
            //eventLogCPM.WriteEntry(args.Message);
            CPMPayAgentService.LoggingLayer.PayAgentLogger.Instance().logError(args.Message);
        }
    }
}
