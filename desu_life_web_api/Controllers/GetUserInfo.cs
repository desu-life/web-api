using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using WebAPI.Database;
using static WebAPI.Security.Token;

namespace WebAPI.Controllers.GetUserInfo;

[ApiController]
[Route("[controller]")]
public class GetUserController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "GetUserInfo")]
    public async Task<ActionResult> GetUserInfoAsync()
    {
        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] Trying to get user infomation by anonymous user.");

        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

        // get info from token
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
            return responseService.Response(HttpStatusCodes.Forbidden, "User information check failed.");

        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] Get user information triggered by user {UserId}.");

        // get user info
        var UserInfo = await Database.Client.GetUser(UserId);
        if (UserInfo is null)
        {
            // log
            logger.LogWarning($"[{Utils.GetCurrentTime}] User {UserId} logged in but not found in database. Perform a forced logout. May be a database issue.");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire);
            return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
        }

        //get osu info
        var oid = await Database.Client.GetOsuUID(UserId);

        // success
        return responseService.ResponseUserInfo(HttpStatusCodes.Ok, UserInfo, oid);
    }
}
