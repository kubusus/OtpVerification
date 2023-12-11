using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MhozaifaA.OtpVerification;
using OtpVerification.Models;

namespace OtpVerification.Services
{
    public interface IOtpVerificationService
    {
        OtpData Generate(string id);
        OtpData Generate(string id, out DateTime expire);
        OtpData Generate(string id, OtpVerificationOptions option);
        OtpData Generate(string id, OtpVerificationOptions option, out DateTime expire);
        bool Scan(string id, string plain);
        bool Scan(string id, string plain, OtpVerificationOptions option);
        bool Scan(string id, string plain, int expire);
        bool Scan(string url);
    }
}
