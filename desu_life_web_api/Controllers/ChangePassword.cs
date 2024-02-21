using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using desu_life_web_api.Response;
using desu_life_web_api.Cookie;
using desu_life_web_api.Request;
using desu_life_web_api.Security;
using desu_life_web_api.Http;
using desu_life_web_api.Database;
using static desu_life_web_api.Security.Token;
using static LinqToDB.Common.Configuration;

namespace desu_life_web_api.Controllers.ChangePassword;

[ApiController]
[Route("[controller]")]
public class change_passwordController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.inner!;
    private readonly ILogger<Log> _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpPost(Name = "ChangePassword")]
    public async Task<ActionResult> ExecuteChangePasswordAsync([FromBody] ChangePasswordRequest request)
    {
        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return _responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

        // get info from token
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
            return _responseService.Response(HttpStatusCodes.InternalServerError, "User information check failed.");

        // check password
        if (string.IsNullOrEmpty(request.NewPassword))
            return _responseService.Response(HttpStatusCodes.BadRequest, "Please provide new password.");

        // log
        _logger.LogInformation($"[{Utils.GetCurrentTime}] Get user information triggered by user {UserId}.");

        // get user info
        var UserInfo = await Database.Client.GetUser(UserId);
        if (UserInfo == null)
        {
            // log
            _logger.LogWarning($"[{Utils.GetCurrentTime}] User {UserId} logged in but not found in database. Perform a forced logout. May be a database issue.");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire);
            return _responseService.Response(HttpStatusCodes.InternalServerError, "User logged in but not found in database.");
        }

        // update password
        if (!await Database.Client.UpdatePassword(UserId, request.NewPassword))
            return _responseService.Response(HttpStatusCodes.InternalServerError, "Password update failed. Please contact the administrator.");

        // success
        _logger.LogInformation($"[{Utils.GetCurrentTime}] User {UserId} successfully updated the password.");

        // need to redirect to the front-end page instead of this api
        return _responseService.Response(HttpStatusCodes.Ok, $"Password update successfully.");
    }
}
