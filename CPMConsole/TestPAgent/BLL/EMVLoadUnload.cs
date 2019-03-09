using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    [Serializable]
    public class emvload
    {
        public string sessionid;
        public string emvaccountid;
        public decimal amount;
        public string externalDocDate;
        public string externalDocNumber;
        public string checksum;
        public string curDate;
        
    }

    [Serializable]
    public class emvunload
    {
        public string sessionid;
        public string emvaccountid;
        public decimal amount;
        public string externalDocDate;
        public string externalDocNumber;
        public string checksum;
        public string curDate;
        
    }
}
