using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CPMPaymentsService.BLL
{
    [Serializable]
    public class operationresult
    {
        public int result;
        public string resultnote;
        public string resultlog;

        public string duetOperCode;
        public string duetOperNote;

        public string status;
        public string statusNote;
        public string livetime;
        public string opercount;

        public operationresult(int result, string resultnote, string resultlog)
        {
            this.result = result;
            this.resultlog = resultlog;
            this.resultnote = resultnote;
        }
        public operationresult()
        {
        }

        public operation[] operations;
    }

    
}
