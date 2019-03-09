using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPMPaymentsService.Logging
{
    public class CPMPaymentLogging
    {
        private static CPMPaymentLogging _instance;
        private string dir;
        private string fullFileName;

        private CPMPaymentLogging()
        {
            dir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Logs");
            try
            {
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
            
                fullFileName = dir + System.IO.Path.DirectorySeparatorChar + "CPMPaymentLogging_{0}.log";
                System.IO.StreamWriter sw = new System.IO.StreamWriter(string.Format(fullFileName, DateTime.Now.ToString("yyyyMMdd")), true);
                //System.IO.StreamWriter sw = new System.IO.StreamWriter(fullFileName, true);
                sw.WriteLine("==========================\r\n" + DateTime.Now.ToString() + " New entries.");
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {
                // TBD Ignore Error
            }
        }

        public static CPMPaymentLogging Instance()
        {
            if (_instance == null)
                _instance = new CPMPaymentLogging();

            return _instance;
        }

        public void log(string text)
        {
            if (Configuration.Settings.SaveLog)
            {
                try
                {
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(string.Format(fullFileName, DateTime.Now.ToString("yyyyMMdd")), true);
                    sw.WriteLine(DateTime.Now.ToString() + ": " + text);
                    sw.Flush();
                    sw.Close();
                }
                catch (Exception)
                {
                    // TBD Ignore Error
                }
            }
        }

        public void logXML(string text)
        {
            if (Configuration.Settings.SaveXML)
            {
                try
                {
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(string.Format(fullFileName, DateTime.Now.ToString("yyyyMMdd")), true);
                    sw.WriteLine(DateTime.Now.ToString() + ": " + text);
                    sw.Flush();
                    sw.Close();
                }
                catch (Exception)
                {
                    // TBD Ignore Error
                }
            }
        }
    }
}
