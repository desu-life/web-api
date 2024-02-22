using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using WebAPI.Database;
using static WebAPI.Security.Token;
using static LinqToDB.Common.Configuration;

namespace WebAPI.Controllers.ChangePassword;

[ApiController]
[Route("[controller]")]
public class ChangePasswordController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "ChangePassword")]
    public async Task<ActionResult> ExecuteChangePasswordAsync([FromBody] ChangePasswordRequest request)
    {
        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

        // get info from token
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
            return responseService.Response(HttpStatusCodes.InternalServerError, "User information check failed.");

        // check password
        if (string.IsNullOrEmpty(request.NewPassword))
            return responseService.Response(HttpStatusCodes.BadRequest, "Please provide new password.");

        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] Get user information triggered by user {UserId}.");

        // get user info
        var UserInfo = await Database.Client.GetUser(UserId);
        if (UserInfo is null)
        {
            // log
            logger.LogWarning($"[{Utils.GetCurrentTime}] User {UserId} logged in but not found in database. Perform a forced logout. May be a database issue.");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire);
            return responseService.Response(HttpStatusCodes.InternalServerError, "User logged in but not found in database.");
        }

        // update password
        if (!await Database.Client.UpdatePassword(UserId, request.NewPassword))
            return responseService.Response(HttpStatusCodes.InternalServerError, "Password update failed. Please contact the administrator.");

        // success
        logger.LogInformation($"[{Utils.GetCurrentTime}] User {UserId} successfully updated the password.");

        // need to redirect to the front-end page instead of this api
        return responseService.Response(HttpStatusCodes.Ok, $"Password update successfully.");
    }
}
