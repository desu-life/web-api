using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static desu_life_web_backend.Database.Models;
using static desu_life_web_backend.ResponseService;
using static desu_life_web_backend.Security;
using static LinqToDB.Common.Configuration;

namespace desu_life_web_backend.Controllers.ResetPassword
{
    [ApiController]
    [Route("[controller]")]
    public class reset_password_verifyController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<Log> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "ResetPasswordVerify")]
        public async Task<ActionResult> ResetPasswordVerifyAsync(string mailAddr, string password)
        {
            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] Password reset verify started by anonymous user.");

            // check if user logged in
            if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.Found, "");

            // check user validity
            var userId = await Database.Client.CheckUserIsExsit(mailAddr);
            if (userId < 0)
                return _responseService.Response(HttpStatusCodes.Forbidden, "User does not exist.");

            // create new verify token and update
            var token = Utils.GenerateRandomString(64);
            if (!await Database.Client.AddVerifyToken(mailAddr, "resetPassword", "desulife", DateTimeOffset.Now, token))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Token generate failed. Please contact Administrator.");

            // send reg email
            try
            {
                await Mail.SendVerificationMail(mailAddr, token, "desulife", "resetPassword");
            }
            catch
            {
                // log
                _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred while sending the verify email.");
                return _responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while sending the verify email.");
            }

            // success
            return _responseService.Response(HttpStatusCodes.Ok, "The registration mail has been sent.");
        }

        [Route("[controller]")]
        public class reset_passwordController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
        {
            private static Config.Base config = Config.inner!;
            private readonly ILogger<Log> _logger = logger;
            private readonly ResponseService _responseService = responseService;

            [HttpGet(Name = "ResetPassword")]
            public async Task<ActionResult> ExecuteResetPasswordAsync(string password, string Token)
            {
                // log
                _logger.LogInformation($"[{Utils.GetCurrentTime}] Password reset started by anonymous user.");

                // check if the user is logged in
                if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                    return _responseService.Response(HttpStatusCodes.NoContent, "Already logged in.");

                // empty check
                if (string.IsNullOrEmpty(password))
                    return _responseService.Response(HttpStatusCodes.BadRequest, "Please provide password.");

                // get email address from database by using token
                var email = await Database.Client.GetEmailAddressByVerifyToken(Token, "resetPassword", "desulife");

                // empty check
                if (string.IsNullOrEmpty(email))
                    return _responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");

                // update password
                if (!await Database.Client.UpdatePassword(email, password))
                    return _responseService.Response(HttpStatusCodes.BadRequest, "Password update failed. Please contact the administrator.");

                // success
                _logger.LogInformation($"[{Utils.GetCurrentTime}] User {email} successfully updated the password.");

                // need to redirect to the front-end page instead of this api
                return _responseService.Response(HttpStatusCodes.Ok, $"Password update successfully.");
            }
        }
    }
}
