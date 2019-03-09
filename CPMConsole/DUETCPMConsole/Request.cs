using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DUETCPMConsole
{
    [Serializable]
    public class request
    {
        public performduetoperation duetOper;
        public isoperationsuccessful operSuccess;
        public getoperationstatus operStatus;
        public howareyou howYou;
    }
}
