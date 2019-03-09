using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    public class LogAssembler
    {
        private string log;

        public LogAssembler()
        {
            log = string.Empty;
        }

        public void AddToLog(string addStr)
        {
            if(string.IsNullOrEmpty(log)){
                log = addStr;
                return;
            }
            log += "\n" + addStr;
        }

        public string getLog()
        {
            return log;
        }
    }
}
