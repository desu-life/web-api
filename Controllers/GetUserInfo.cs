using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static desu_life_web_backend.ResponseService;
using static desu_life_web_backend.Security;
using static LinqToDB.Common.Configuration;

namespace desu_life_web_backend.Controllers.GetUserInfo;

[ApiController]
[Route("[controller]")]
public class get_userController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.inner!;
    private readonly ILogger<Log> _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpGet(Name = "GetUserInfo")]
    public async Task<ActionResult> GetUserInfoAsync()
    {
        // log
        _logger.LogInformation($"[{Utils.GetCurrentTime}] Trying to get user infomation by anonymous user.");

        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return _responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");

        // get info from token
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
            return _responseService.Response(HttpStatusCodes.Forbidden, "User information check failed.");

        // log
        _logger.LogInformation($"[{Utils.GetCurrentTime}] Get user information triggered by user {UserId}.");

        // get user info
        var UserInfo = await Database.Client.GetUser(UserId);
        if (UserInfo == null)
        {
            // log
            _logger.LogWarning($"[{Utils.GetCurrentTime}] User {UserId} logged in but not found in database. Perform a forced logout. May be a database issue.");
            HttpContext.Response.Cookies.Append("token", "", Cookies.Expire);
            return _responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
        }

        //get osu info
        var oid = await Database.Client.GetOsuUID(UserId);

        // success
        return _responseService.ResponseUserInfo(HttpStatusCodes.Ok, UserInfo, oid);
    }
}
