using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static desu_life_web_backend.Database.Models;
using static desu_life_web_backend.ResponseService;
using static desu_life_web_backend.Security;

namespace desu_life_web_backend.Controllers.QQ
{
    [ApiController]
    [Route("[controller]")]
    public class qq_linkController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<Log> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "QQLink")]
        public async Task<ActionResult> GetAuthorizeLinkAsync()
        {
            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] QQ link started by anonymous user.");

            // check if user token is valid
            if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

            // get info from token
            if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
                return _responseService.Response(HttpStatusCodes.BadRequest, "User information check failed.");

            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] QQ link operation triggered by user {UserId}.");

            // check user's links
            if (await Database.Client.CheckCurrentUserHasLinkedDiscord(UserId))
                return _responseService.Response(HttpStatusCodes.Conflict, "Your account is currently linked to discord account.");

            // create new verify token and update
            var token = Utils.GenerateRandomString(64);
            HttpContext.Response.Cookies.Append("token", UpdateVerifyTokenFromToken(HttpContext.Request.Cookies, token), Cookies.Default);

            // *注：qq的开发者id申请不下来，只能用手输token的方式验证了
            if (!await Database.Client.AddVerifyToken(mailAddr, "link", "qq", DateTimeOffset.Now.AddHours(1), token))
            {
                // log
                _logger.LogError($"[{Utils.GetCurrentTime}] Token generate failed.");
                return _responseService.Response(HttpStatusCodes.BadRequest, "Token generate failed. Please contact Administrator.");
            }

            // success
            return _responseService.ResponseQQVerify(HttpStatusCodes.Ok, token);
        }
    }
}
