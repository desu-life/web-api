using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using static desu_life_web_backend.Database.Models;
using static desu_life_web_backend.ResponseService;
using static desu_life_web_backend.Security;

namespace desu_life_web_backend.Controllers.Registration
{
    [ApiController]
    [Route("[controller]")]
    public class registrationController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<Log> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "Registration")]
        public async Task<ActionResult> ExecuteLinkAsync(string email)
        {
            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] Account registration started by anonymous user.");

            // check if the user is logged in
            // if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            //     return _responseService.Response(HttpStatusCodes.NoContent, "Already logged in.");

            // empty check
            if (string.IsNullOrEmpty(email))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Please provide complete email.");

            // check if user is registered
            if (await Database.Client.CheckUserIsRegistered(email))
                return _responseService.Response(HttpStatusCodes.Conflict, "The provided email address has been registered.");

            // create new verify token and update
            var token = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), email, "reg");

            // send reg email
            try
            {
                await Mail.SendVerificationMail(email, token, "desulife", "reg");
            }
            catch
            {
                // log
                _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred while sending the registration email.");
                return _responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while sending the registration email.");
            }

            // success
            return _responseService.Response(HttpStatusCodes.Ok, "The registration mail has been sent.");
        }
    }

    [Route("[controller]")]
    public class set_accountController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<Log> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "SetAccount")]
        public async Task<ActionResult> GetAuthorizeLinkAsync(string password, string email, string username, string Token)
        {
            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] Account Set started by anonymous user.");

            // check if the user is logged in
            // if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            //     return _responseService.Response(HttpStatusCodes.NoContent, "Already logged in.");

            // empty check
            if (string.IsNullOrEmpty(password))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Please provide password.");

            // check if token is vaild
            var tempTokenData = Token.Split("[%*#]");
            if (Token != GenerateVerifyToken(long.Parse(tempTokenData[0]), email, "reg") || DateTimeOffset.Now > DateTimeOffset.Parse(tempTokenData[0]).AddHours(1))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Verification failed.");

            // execute reg
            if (!await Database.Client.InsertUser(email, password, username))
            {
                // log
                _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred while requesting registration");
                return _responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while requesting registration. Please contact the administrator.");
            }

            // success
            _logger.LogInformation($"[{Utils.GetCurrentTime}] Email {email} successfully registered.");
            return _responseService.Response(HttpStatusCodes.Ok, "Registration success.");
        }
    }
}
