using MhozaifaA.OtpVerification.Utils;
using OtpVerification.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MhozaifaA.OtpVerification
{
    public class OtpVerificationExtension : SecureHasher
    {
        public static string Generate(OtpVerificationOptions option, out DateTime expire, out string hash)
        {
            if (option is null)
                throw new ArgumentNullException($"{nameof(OtpVerificationService)} {nameof(option)} can't be null");

            if (option.OtpLength <= 0)
                throw new ArgumentException($"{nameof(OtpVerificationService)} {nameof(option.OtpLength)} can't be 0 or low");

            if (option.HashLength <= 0)
                throw new ArgumentException($"{nameof(OtpVerificationService)} {nameof(option.HashLength)} can't be 0 or low");

            if (option.ExpiryTimeMin < 0)
                throw new ArgumentException($"{nameof(OtpVerificationService)} {nameof(option.ExpiryTimeMin)} can't be low than 0");

            if (option.HashIterations <= 0)
                throw new ArgumentException($"{nameof(OtpVerificationService)} {nameof(option.HashIterations)} can't be 0 or low");

            DateTime dateNow = DateTime.Now;
            string otpCode = Generator.RandomString(option.OtpLength,StringsOfLetters.Number);
            expire = dateNow.AddMinutes(option.ExpiryTimeMin);
            hash = Hash(otpCode + dateNow.ToString("yyyyMMddHHmm"), option.HashLength, option.HashIterations);
            return otpCode;
        }

        public static string Generate(out DateTime expire, out string hash)
        {
            return Generate(new OtpVerificationOptions(), out expire, out hash);
        }

        public static string Generate(OtpVerificationOptions option, out string hash)
        {
            return Generate(option, out _, out hash);
        }

        public static string Generate(out string hash)
        {
            return Generate(new OtpVerificationOptions(), out hash);
        }

        public static string Generate(OtpVerificationOptions option, out DateTime expire)
        {
            return Generate(option, out expire, out _);
        }

        public static string Generate(out DateTime expire)
        {
            return Generate(new OtpVerificationOptions(), out expire);
        }

        public static string Generate(OtpVerificationOptions option)
        {
            return Generate(option, out _, out _);
        }

        public static string Generate()
        {
            return Generate(new OtpVerificationOptions());
        }


        public static bool VerifyOtp(string otpCode, string hashedReference, OtpVerificationOptions option)
        {
            if (string.IsNullOrEmpty(otpCode))
                throw new ArgumentNullException($"{nameof(OtpVerificationService)} {nameof(otpCode)} can't be null or empty");

            if (string.IsNullOrEmpty(hashedReference))
                throw new ArgumentNullException($"{nameof(OtpVerificationService)} {nameof(hashedReference)} can't be null or empty");

            bool verify;
            int begin = 0;
            do
            {
                var valueToHash = otpCode + DateTime.Now.AddMinutes(-begin).ToString("yyyyMMddHHmm");
                verify = Verify(valueToHash, hashedReference);
                begin++;
            } while (verify == false && begin <= option.ExpiryTimeMin);

            return verify;
        }

        public static bool VerifyOtp(string plain, string hash, int expire)
        {
            return VerifyOtp(plain, hash, new OtpVerificationOptions() { ExpiryTimeMin = expire });
        }

        public static bool VerifyOtp(string plain, string hash)
        {
            return VerifyOtp(plain, hash, new OtpVerificationOptions());
        }

    }
}
