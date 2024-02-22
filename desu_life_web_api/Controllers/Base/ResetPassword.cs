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
using static WebAPI.Security.Token;
using System.Net;
using WebAPI.Database.Models;

namespace WebAPI.Controllers.ResetPassword;
[ApiController]
[Route("[controller]")]
public class ResetPasswordVerifyController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static readonly Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "ResetPasswordVerify")]
    public async Task<ActionResult> ResetPasswordVerifyAsync(string mailAddr)
    {
        // 验证用户
        var user = await DB.GetUserByEmail(mailAddr);

        if (user is null)
        {
            logger.LogWarning("{CurrentTime} ResetPasswordVerify 失败。操作由ip {IPADDR} 触发。", $"[{GetCurrentTime}]", HttpContext.Connection.RemoteIpAddress);
            return responseService.Response(HttpStatusCodes.Forbidden, "Invaild Operation.");
        }

        // 发送验证邮件
        try
        {
            // 创建新的验证Token
            var token = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), mailAddr, "resetpw");

            // 发送
            await MailService.SendVerificationMail(mailAddr, token, "desulife", "resetPassword");

            // 成功
            return responseService.Response(HttpStatusCodes.Ok, "The registration mail has been sent.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} ResetPasswordVerify 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while sending the verify email.");
        }
    }
}

[Route("[controller]")]
public class ResetPasswordController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "ResetPassword")]
    public async Task<ActionResult> ExecuteResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        // 信息检查
        if (string.IsNullOrEmpty(request.NewPassword) || string.IsNullOrEmpty(request.MailAddress) || string.IsNullOrEmpty(request.Token))
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");

        // 检查Token有效性
        var tempTokenData = request.Token.Split("[%*#]");
        if (request.Token != GenerateVerifyToken(long.Parse(tempTokenData[0]), request.MailAddress, "resetpw")
            || DateTimeOffset.Now > DateTimeOffset.Parse(tempTokenData[0]).AddHours(1))
        {
            logger.LogWarning("{CurrentTime} ResetPassword 失败。操作由ip {IPADDR} 触发。", $"[{GetCurrentTime}]", HttpContext.Connection.RemoteIpAddress);
            return responseService.Response(HttpStatusCodes.BadRequest, "Verification failed.");
        }

        // 更新密码
        if (!await DB.UpdatePassword(request.MailAddress, request.NewPassword))
        {
            logger.LogWarning("{CurrentTime} ResetPassword 失败，向数据库中更新信息失败。", $"[{GetCurrentTime}]");
            return responseService.Response(HttpStatusCodes.BadRequest, "Password update failed. Please contact the administrator.");
        }

        // 成功
        logger.LogWarning("{CurrentTime} ResetPassword 用户 {EMAIL} 成功更新了密码。", $"[{GetCurrentTime}]", request.MailAddress);
        return responseService.Response(HttpStatusCodes.Ok, $"Password update successfully.");
    }
}
