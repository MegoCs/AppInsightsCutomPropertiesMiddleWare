using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiSample.Helper
{
    public class AppInsightsCustomMiddlewareSettings
    {
        public bool IsEnabled { get; set; }
        public bool RequestHeaders { get; set; }
        public bool RequestBody { get; set; }
        public bool ResponseHeaders { get; set; }
        public bool ResponseBody { get; set; }
    }
}
