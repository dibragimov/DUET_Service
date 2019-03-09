using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPMPayAgentService.Utility
{
    public interface IErrorReporter
    {
        void reportError(string error);
    }
}
