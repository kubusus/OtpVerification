using MhozaifaA.OtpVerification;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OtpVerification.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OtpVerification.Services
{

    public class OtpVerificationService : IOtpVerificationService
    {

        #region dependency injection
        private readonly IHttpContextAccessor httpContext;
        private readonly IDistributedCache distributedCache;
        private readonly IMemoryCache memoryCache;
        private readonly IDataProtector dataProtection;
        private readonly OtpVerificationOptions options;

        public OtpVerificationService(IDataProtectionProvider dataProtection, IHttpContextAccessor httpContext,
            IDistributedCache distributedCache = null, IMemoryCache memoryCache = null,
             IOptions<OtpVerificationOptions> options = null)
        {
            this.httpContext = httpContext;
            this.distributedCache = distributedCache;
            this.memoryCache = memoryCache;
            this.dataProtection = dataProtection.CreateProtector("This is a very secure key");
            this.options = options?.Value ?? new OtpVerificationOptions();
        }
        #endregion

        private record class IdPlain(string id, string plain);


        private string BaseOtpUrl => $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}/{nameof(OtpVerificationService)}/";


        private string GenerateCacheKey(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException($"{nameof(OtpVerificationService)} Unique {nameof(id)} can't be null or empty");

            return $"{nameof(OtpVerificationService)}:{id}";
        }


        private bool TryDecryptProtectedUrl(string key, out string id, out string plain)
        {
            id = plain = string.Empty;
            try
            {
                var data = dataProtection.Unprotect(key);

                var obj = System.Text.Json.JsonSerializer.Deserialize<IdPlain>(data);
                id = obj.id;
                plain = obj.plain;
                return true;
            }
            catch (Exception ex) when (ex is CryptographicException || ex is RuntimeBinderException)
            {
                return false;
            }
        }

        public OtpData GenerateOtp(string id)
        {
            return GenerateOtp(id, out _);
        }

        public OtpData GenerateOtp(string id, out DateTime expire)
        {
            return Generate(id, options, out expire);
        }

        public OtpData GenerateOtp(string id, OtpVerificationOptions option)
        {
            return Generate(id, option, out _);
        }

        public OtpData Generate(string id, OtpVerificationOptions option, out DateTime expire)
        {
            var plain = OtpVerificationExtension.Generate(option, out expire, out string hash);

            if (options.IsInMemoryCache)
                memoryCache.Set(GenerateCacheKey(id), hash, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = expire,
                    Priority = CacheItemPriority.High,
                });
            else
                distributedCache.SetString(GenerateCacheKey(id), hash, new DistributedCacheEntryOptions()
                {
                    AbsoluteExpiration = expire,
                });

            string url = string.Empty;
            if (option.EnableUrl)
                url = BaseOtpUrl + dataProtection.Protect(
                    System.Text.Json.JsonSerializer.Serialize(new IdPlain(id, plain)));

            return new OtpData(plain, url);
        }


        public bool VerifyOtp(string id, string otpCode, OtpVerificationOptions option)//plane is the otp code entered by user
        {
            string hashFromMemory = string.Empty;

            if (options.IsInMemoryCache)
                hashFromMemory = memoryCache.Get<string>(GenerateCacheKey(id));
            else
                hashFromMemory = distributedCache.GetString(GenerateCacheKey(id));

            if (hashFromMemory is null)
                return false;

            if (OtpVerificationExtension.VerifyOtp(otpCode, hashFromMemory, option))
            {
                if (options.IsInMemoryCache)
                    memoryCache.Remove(GenerateCacheKey(id));
                else
                    distributedCache.Remove(GenerateCacheKey(id));
                return true;
            }

            return false;
        }

        public bool VerifyOtp(string id, string otpCode, int expire)
        {
            return VerifyOtp(id, otpCode, new OtpVerificationOptions() { ExpiryTimeMin = expire });
        }

        public bool VerifyOtp(string id, string otpCode)//plane is the otp code entered by user
        {
            return VerifyOtp(id, otpCode, options);
        }

        public bool VerifyOtp(string url)
        {
            if (TryDecryptProtectedUrl(url, out string id, out string code))
                return VerifyOtp(id, code);
            return false;
        }
    }
}
