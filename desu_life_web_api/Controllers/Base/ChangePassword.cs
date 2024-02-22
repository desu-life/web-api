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
using WebAPI.Database.Models;

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
        // 检查用户Token是否有效并从中获取信息
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userId, out var mailAddr, out var token))
        {
            logger.LogWarning("{CurrentTime} ChangePassword 中递交了无效的Token。", $"[{GetCurrentTime}]");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");
        }

        // 检查新密码
        if (string.IsNullOrEmpty(request.NewPassword))
            return responseService.Response(HttpStatusCodes.BadRequest, "Please provide new password.");

        // 获取用户信息
        try
        {
            (var user, var qq, var osu, var discord, var badges) = await ControllerUtils.GetFullUserInfoAsync(userId);
            if (user is null)
            {
                logger.LogWarning("{CurrentTime} ChangePassword 失败，用户不存在。", $"[{GetCurrentTime}]");
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
                return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
            }

            // 修改密码
            if (!await Client.UpdatePassword(userId, request.NewPassword))
                return responseService.Response(HttpStatusCodes.InternalServerError, "Password update failed. Please contact the administrator.");

            logger.LogWarning("{CurrentTime} ChangePassword 用户 {UID} 成功更新了密码。", $"[{GetCurrentTime}]", userId);
            // 成功
            return responseService.Response(HttpStatusCodes.Ok, $"Password update successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} ChangePassword 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }
}
