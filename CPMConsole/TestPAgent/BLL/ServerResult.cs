using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPMPaymentsService.BLL
{
    public class ServerResult
    {
        public string Message { get; set; }
        public operationresult Result { get; set; }
        public double ExecTime { get; set; }
    }
}
