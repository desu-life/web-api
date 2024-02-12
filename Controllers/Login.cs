using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static desu_life_web_backend.Database.Models;
using static desu_life_web_backend.ResponseService;

namespace desu_life_web_backend.Controllers.Login
{
    [ApiController]
    [Route("[controller]")]
    public class logoutController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<Log> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpPost(Name = "Logout")]
        public ActionResult ExecuteLogOut()
        {
            HttpContext.Response.Cookies.Append("token", "", Cookies.Expire);
            return _responseService.Response(HttpStatusCodes.Ok, "Successfully logged out.");
        }
    }


    [Route("[controller]")]
    public class loginController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<Log> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpPost(Name = "Login")]
        public async Task<ActionResult> ExecuteLoginAsync([FromBody] LoginRequest request)
        {
            // check if user token is valid
            // if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            //     return _responseService.Response(HttpStatusCodes.NoContent, ""); //"Already logged in.");

            // check email&password
            if (string.IsNullOrEmpty(request.MailAddress) || string.IsNullOrEmpty(request.Password))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Please provide complete email or password.");

            // check user validity
            var userId = await Database.Client.CheckUserIsValidity(request.MailAddress, request.Password);
            if (userId < 0)
                return _responseService.Response(HttpStatusCodes.BadRequest, "User does not exist or password is incorrect.");

            // create new token
            HttpContext.Response.Cookies.Append("token", Security.SetLoginToken(userId, request.MailAddress), Cookies.Default);

            // success
            _logger.LogInformation($"[{Utils.GetCurrentTime}] User {userId} logged in.");
            return _responseService.Response(HttpStatusCodes.Ok, "Successfully logged in.");
        }
    }
}
