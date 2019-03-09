using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    [Serializable]
    public class operation
    {
        public long sessionid;
        public int result;
        public string resultnote;
        public string resultlog;
    }
}
