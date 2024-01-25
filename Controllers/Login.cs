using Microsoft.AspNetCore.Mvc;
using static desu_life_web_backend.ResponseService;

namespace desu_life_web_backend.Controllers.Login
{
    [ApiController]
    [Route("[controller]")]
    public class logoutController(ILogger<SystemMsg> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<SystemMsg> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "Logout")]
        public ActionResult ExecuteLogOut()
        {
            HttpContext.Response.Cookies.Append("token", "", Cookies.Expire);
            return _responseService.Response(HttpStatusCodes.Ok, "Successfully logged out.");
        }
    }


    [Route("[controller]")]
    public class loginController(ILogger<SystemMsg> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<SystemMsg> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "Login")]
        public async Task<ActionResult> ExecuteLoginAsync(string? mailAddr, string? password)
        {
            // check if user token is valid
            if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.NoContent, "Already logged in.");

            // check email&password
            if (string.IsNullOrEmpty(mailAddr) || string.IsNullOrEmpty(password))
                return _responseService.Response(HttpStatusCodes.Unauthorized, "Please provide complete email or password.");

            // check user validity
            var userId = await Database.Client.CheckUserIsValidity(mailAddr, password);
            if (userId < 0)
                return _responseService.Response(HttpStatusCodes.Unauthorized, "User does not exist or password is incorrect.");

            // create new token
            HttpContext.Response.Cookies.Append("token", JWT.CreateJWTToken(userId, null), Cookies.Login);

            // success
            return _responseService.Response(HttpStatusCodes.Ok, "Successfully logged in.");
        }
    }
}
