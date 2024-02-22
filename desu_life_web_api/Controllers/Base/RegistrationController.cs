using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Database.Models;
using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using static WebAPI.Security.Token;
using WebAPI.Mail;
using System.Net;

namespace WebAPI.Controllers.Registration;

[ApiController]
[Route("[controller]")]
public class RegistrationController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "Registration")]
    public async Task<ActionResult> ExecuteLinkAsync(string email)
    {
        // 检查邮箱
        if (string.IsNullOrEmpty(email))
            return responseService.Response(HttpStatusCodes.BadRequest, "Please provide complete email.");

        // 获取用户信息
        try
        {
            var user = await DB.GetUserByEmail(email);
            if (user is not null)
            {
                logger.LogWarning("{CurrentTime} RegistrationVerify 失败，邮箱已被注册。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.Conflict, "The provided email address has been registered.");
            }

            // 创建新的注册Token
            var token = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), email, "reg");

            // 发送注册邮件
            try
            {
                await MailService.SendVerificationMail(email, token, "desulife", "reg");
            }
            catch
            {
                logger.LogWarning("{CurrentTime} RegistrationVerify 失败，发送邮件时发生了错误。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while sending the registration email.");
            }

            // 成功
            return responseService.Response(HttpStatusCodes.Ok, "The registration mail has been sent.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} RegistrationVerify 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }
}

[Route("[controller]")]
public class SetAccountController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "SetAccount")]
    public async Task<ActionResult> GetAuthorizeLinkAsync([FromBody] RegistrationRequest request)
    {
        // 信息检查
        if (string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Token) ||
            string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.MailAddress))
            return responseService.Response(HttpStatusCodes.BadRequest, "Please provide full information.");

        // 检查Token有效性
        var tempTokenData = request.Token.Split("[%*#]");
        if (request.Token != GenerateVerifyToken(long.Parse(tempTokenData[0]), request.MailAddress, "reg")
            || DateTimeOffset.Now > DateTimeOffset.Parse(tempTokenData[0]).AddHours(1))
            return responseService.Response(HttpStatusCodes.BadRequest, "Verification failed.");

        // 执行注册
        if (!await DB.InsertUser(request.MailAddress, request.Password, request.Username))
        {
            logger.LogWarning("{CurrentTime} Registration 失败，向数据库中写入数据时失败。", $"[{GetCurrentTime}]");
            return responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while requesting registration. Please contact the administrator.");
        }

        // 成功
        logger.LogInformation("{CurrentTime} Registration 成功，邮箱 {Email} 创建了一个新用户。", $"[{GetCurrentTime}]", request.MailAddress);
        return responseService.Response(HttpStatusCodes.Ok, "Registration success.");
    }
}
