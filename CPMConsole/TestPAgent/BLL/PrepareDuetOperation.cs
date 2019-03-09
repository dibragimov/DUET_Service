using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DUETCPMConsole
{
    [Serializable]
    public class prepareduetoperation
    {
        public string sessionID;

        public int clientAccountID;

        public int counterAgentAccountID;

        public int counterAgentClientAccountID;

        public int contractBindID;

        public string externalDocDate;

        public string externalDocNumber;

        public decimal feeAmount;

        public int functionType;

        public string paymentDetails;

        public decimal transactAmount;

        public string checksum;

        public string curDate;
    }

    [Serializable]
    public class confirmduetoperation
    {
        public string sessionID;

        public string externalDocDate;

        public string externalDocNumber;

        public string checksum;

        public string curDate;
    }

    [Serializable]
    public class cancelduetoperation
    {
        public string sessionID;

        public string externalDocDate;

        public string externalDocNumber;

        public string checksum;

        public string curDate;
    }
}
