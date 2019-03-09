using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    [Serializable]
    public class operation
    {
        public string sessionid;
        public int result;
        public string resultnote;
        public string resultlog;
        public string datetime;
        public string duetOperCode;
        public string duetOperNote;
    }
}
