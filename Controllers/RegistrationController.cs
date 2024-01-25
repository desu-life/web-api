using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.NoContent, "Already logged in.");

            // empty check
            if (string.IsNullOrEmpty(email))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Please provide complete email.");

            // check if user is registered
            if (await Database.Client.CheckUserIsRegistered(email))
                return _responseService.Response(HttpStatusCodes.Conflict, "The provided email address has been registered.");

            // create new verify token and update
            var token = Utils.GenerateRandomString(64);
            HttpContext.Response.Cookies.Append("token", UpdateVerifyTokenFromToken(HttpContext.Request.Cookies, token), Cookies.Default);
            if (!await Database.Client.AddVerifyToken(email, "reg", "desulife", DateTimeOffset.Now, token))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Token generate failed. Please contact Administrator.");

            // send reg email
            try
            {
                await Mail.SendVerificationMail(email, token, "desulife");
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
        public async Task<ActionResult> GetAuthorizeLinkAsync(string password, string Token)
        {
            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] Account Set started by anonymous user.");

            // check if the user is logged in
            if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.NoContent, "Already logged in.");

            // empty check
            if (string.IsNullOrEmpty(password))
                return _responseService.Response(HttpStatusCodes.Unauthorized, "Please provide password.");

            // get email address from database by using token
            var email = await Database.Client.RegGetEmailAddressByVerifyToken(Token);

            // empty check
            if (string.IsNullOrEmpty(email))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Invalid request.");

            // execute reg
            if (!await Database.Client.InsertUser(email, password))
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
