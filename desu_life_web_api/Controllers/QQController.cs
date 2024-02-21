using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static desu_life_web_api.Database.Models;
using desu_life_web_api.Response;
using desu_life_web_api.Cookie;
using desu_life_web_api.Request;
using desu_life_web_api.Security;
using desu_life_web_api.Http;
using desu_life_web_api.Database;
using static desu_life_web_api.Security.Token;

namespace desu_life_web_api.Controllers.QQ
{
    [ApiController]
    [Route("[controller]")]
    public class qq_linkController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<Log> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "QQLink")]
        public ActionResult GetAuthorizeLink()
        {
            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] QQ link started by anonymous user.");

            // check if user token is valid
            if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

            // get info from token
            if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
                return _responseService.Response(HttpStatusCodes.Forbidden, "User information check failed.");

            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] QQ link operation triggered by user {UserId}.");

            // *注：qq的开发者id申请不下来，只能用手输token的方式验证了
            var token = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), UserId.ToString(), "reg");

            // success
            return _responseService.ResponseQQVerify(HttpStatusCodes.Ok, token);
        }
    }
}
