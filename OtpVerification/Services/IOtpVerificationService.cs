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
        OtpData GenerateOtp(string id);
        OtpData GenerateOtp(string id, out DateTime expire);
        OtpData GenerateOtp(string id, OtpVerificationOptions option);
        OtpData Generate(string id, OtpVerificationOptions option, out DateTime expire);
        bool VerifyOtp(string id, string plain);
        bool VerifyOtp(string id, string plain, OtpVerificationOptions option);
        bool VerifyOtp(string id, string plain, int expire);
        bool VerifyOtp(string url);
    }
}
