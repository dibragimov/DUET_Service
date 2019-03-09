using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    [Serializable]
    public class getoperationstatus
    {
        public Nullable<int> result;
        public string from;
        public string to;
        public string curdate;
        public string checksum;
    }
}
