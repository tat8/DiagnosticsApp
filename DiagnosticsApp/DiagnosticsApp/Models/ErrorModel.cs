using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Models
{
    public class ErrorModel
    {
        public string CustomMessage { get; set; }
        public string Message { get; set; }
        public Exception InnerException { get; set; }
        public string Source { get; set; }
    }
}
