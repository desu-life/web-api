using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebAPI.Database.Models;
using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using static WebAPI.Security.Token;

namespace WebAPI.Controllers.QQ;

[ApiController]
[Route("[controller]")]
public class QQLinkController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "QQLink")]
    public ActionResult GetAuthorizeLink()
    {
        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] QQ link started by anonymous user.");

        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

        // get info from token
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userID, out var mailAddr, out var _token))
            return responseService.Response(HttpStatusCodes.Forbidden, "User information check failed.");

        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] QQ link operation triggered by user {userID}.");

        // *注：qq的开发者id申请不下来，只能用手输token的方式验证了
        var token = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), userID.ToString(), "reg");

        // success
        return responseService.ResponseQQVerify(HttpStatusCodes.Ok, token);
    }
}
