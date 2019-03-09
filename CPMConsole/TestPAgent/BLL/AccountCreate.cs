using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    [Serializable]
    public class openaccount
    {
        public string sessionid;

        public int clientDataId;

        public int accountSubNumber;

        public int counterAgentAccountID;
        
        public string extAccountNumber;

        public string externalDocNumber;
        
        public int cPart;

        public string checksum;

        public string curDate;
    }

    [Serializable]
    public class openclient
    {
        public string sessionid;

        public string name;

        public string address;

        public string index;

        public int bankid;

        public int participantid;

        public int region;

        public int clientClass;

        public DateTime contractDate;

        public string contractNumber;

        public string taxID1;

        public string taxID2;

        public string INN;

        public string OKPO;

        public string checksum;

        public string curDate;
    }
}
