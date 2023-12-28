using Example.WebAPI.Models;
using MhozaifaA.OtpVerification.Utils;
using Microsoft.AspNetCore.Mvc;
using OtpVerification.Services;
using System.ComponentModel.DataAnnotations;

namespace Example.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[Action]")]
    public class OtpController : ControllerBase
    {

        private readonly IOtpVerificationService otp;
        public static List<User> users = new List<User>();

        public OtpController(IOtpVerificationService otp)
        {
            this.otp = otp;
        }



        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(users);
        }


        [HttpPost]
        public IActionResult CreateUser(User user)
        {
            user.isVerified = default;
            user.Id =int.Parse(Generator.RandomString(2,StringsOfLetters.Number));
            users.Add(user);

            var otpCode = otp.GenerateOtp(user.Id.ToString(),expire: out DateTime expiryDate);
            
            return Ok(new { Code = otpCode, ExpireDate = expiryDate });// this code sent by Email or SMS
        }


        [HttpPost("{id}")]
        public IActionResult RefreshUser([FromRoute]int id)
        {
            var user = users.Where(u => u.Id == id).FirstOrDefault();
            
            if (user is null) 
                return NotFound($"user with Id:{id} not exist");
            
            user.isVerified = false;
            
            var otpCode = otp.GenerateOtp(user.Id.ToString(), expire: out DateTime expiryDate);
            
            
            // this code sent by Email or SMS
            return Ok(new { Code = otpCode, ExpireDate = expiryDate });
        }


        [HttpPost]
        public IActionResult VerifyUser([Range(0,99)] int userId,[Required] string code)
        {

            var user = users.Where(u => u.Id == userId).FirstOrDefault();

            if (user is null) 
                return NotFound($"user with Id:{userId} not exist");

            if (user.isVerified) 
                return BadRequest($"user with Id:{userId} is already verified");



            if(otp.VerifyOtp(userId.ToString(), code))
            {
                user.isVerified = true;
                return Ok($"user with Id:{userId} successful confirmed his OTP code {code}");
            }



            return BadRequest($"user with Id:{userId} enter wrong code or expired");
        }

    }
}