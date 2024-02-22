using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Database.Models;
using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using WebAPI.Database;
using static WebAPI.Security.Token;
using System.Collections.Generic;

namespace WebAPI.Controllers.Login;

[ApiController]
[Route("[controller]")]
public class LogoutController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "Logout")]
    public ActionResult ExecuteLogOut()
    {
        HttpContext.Response.Cookies.Append("token", "", cookies.Expire);
        return responseService.Response(HttpStatusCodes.Ok, "Successfully logged out.");
    }
}

[Route("[controller]")]
public class LoginController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "Login")]
    public async Task<ActionResult> ExecuteLoginAsync([FromBody] Request.LoginRequest request)
    {
        // 检查用户名与密码
        if (string.IsNullOrEmpty(request.MailAddress) || string.IsNullOrEmpty(request.Password))
            return responseService.Response(HttpStatusCodes.BadRequest, "Please provide complete email or password.");

        // 检查用户有效性
        try
        {
            var userId = await DB.ValidateUserCredentials(request.MailAddress, request.Password);
            if (userId is null)
            {
                logger.LogWarning("{CurrentTime} Login 失败。操作由ip {IPADDR} 触发。", $"[{GetCurrentTime}]", HttpContext.Connection.RemoteIpAddress);
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire);
                return responseService.Response(HttpStatusCodes.BadRequest, "User does not exist or password is incorrect."); ;
            }

            // 下发Token
            HttpContext.Response.Cookies.Append("token", SetLoginToken((long)userId, request.MailAddress), cookies.Default);
            logger.LogInformation("{CurrentTime} Login 用户 {UID} 成功登录。", $"[{GetCurrentTime}]", userId);

            // 成功
            return responseService.Response(HttpStatusCodes.Ok, "Successfully logged in.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} ChangePassword 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }
}
