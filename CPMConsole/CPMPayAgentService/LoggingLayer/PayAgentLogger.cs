using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CPMPayAgentService.LoggingLayer
{

    public class PayAgentLogger
    {
        StringBuilder errors;
        StringBuilder successes;
        private static PayAgentLogger _instance;

        private PayAgentLogger()
        {
            errors = new StringBuilder();
            successes = new StringBuilder();
        }

        public static PayAgentLogger Instance()
        {
            if (_instance == null)
                _instance = new PayAgentLogger();

            return _instance;
        }

        public void logError(string text)
        {
            try
            {
                string dir = Directory.GetCurrentDirectory();
                string fullFileName = dir + Path.DirectorySeparatorChar + "PayAgentLogger.log";
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fullFileName, true);
                //sw.WriteLine("==========================\r\n" + DateTime.Now.ToString() + " New entries.");
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
