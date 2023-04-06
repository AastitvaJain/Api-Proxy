using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interview.Models
{
    internal class ProxyStatModel
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int BadRequestCount { get; set; }
        public int RpmLimitReachedCount { get; set; }
    }
}
