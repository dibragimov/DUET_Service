using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    [Serializable]
    public class blockcardaccount
    {
        public string sessionid;
        public int clientAccountID;
        public int blockType;
        public int blockAction;
        public string reason;
        public string checksum;
        public string curDate;
    }

    [Serializable]
    public class emvcardblockunblock
    {
        public string sessionid;
        public int clientdataid;
        public int emvaccountid;
        public int emvcardid;
        public int reason;
        public string checksum;
        public string curDate;
    }
    
}
