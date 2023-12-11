using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtpVerification.Models
{
    public struct OtpData
    {
        public string Code { get; set; }
        public string Url { get; set; }
        
        public OtpData(string code, string url = default)
        {
            Code = code;
            Url = url;
        }

        
        public override string ToString()
        {
            return $"Code:{Code}, Url:{Url}";
        }
    }
}
