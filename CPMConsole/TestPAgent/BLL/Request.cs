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
        public prepareduetoperation prepareduetoperation;
        public confirmduetoperation confirmduetoperation;
        public cancelduetoperation cancelduetoperation;
        public blockcardaccount blockcardaccount;
        public emvload emvload;
        public emvunload emvunload;
        public openaccount openaccount;
        public openclient openclient;
        public emvcardblockunblock emvcardblockunblock;

        public override string ToString()
        {
            string retStr = (duetOper != null) ? "true" : "false";
            retStr +=(operSuccess != null) ? "true" : "false";
            retStr +=(operStatus != null) ? "true" : "false" ;
            retStr +=(howYou != null) ? "true" : "false";
            retStr += (prepareduetoperation != null) ? "true" : "false";
            retStr += (blockcardaccount != null) ? "true" : "false";
            retStr += (emvload != null) ? "true" : "false";
            retStr += (emvunload != null) ? "true" : "false";
            retStr += (openaccount != null) ? "true" : "false";
            retStr += (openclient != null) ? "true" : "false";
            retStr += (emvcardblockunblock != null) ? "true" : "false";
            return retStr +" " + base.ToString();
            //return base.ToString();
        }
    }
}
