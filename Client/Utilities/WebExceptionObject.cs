using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Client.Utilities
{
    public class WebExceptionObject
    {
        public string Message { get; set; }

        public string ExceptionMessage { get; set; }

        public string ExceptionType { get; set; }

        public string StackTrace { get; set; }
    }
}