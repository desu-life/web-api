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
using System.Net;

namespace WebAPI.Controllers.QQ;

[ApiController]
[Route("[controller]")]
public class QQLinkController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "LinkQQ")]
    public async Task<ActionResult> GetAuthorizeLinkAsync()
    {
        // 检查用户Token是否有效并从中获取信息
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userId, out var mailAddr, out var token))
        {
            logger.LogWarning("{CurrentTime} LinkQQ 中递交了无效的Token。", $"[{GetCurrentTime}]");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");
        }

        // 获取用户信息
        try
        {
            (var user, var qq, var osu, var discord, var badges) = await ControllerUtils.GetFullUserInfoAsync(userId);
            if (user is null)
            {
                logger.LogWarning("{CurrentTime} LinkQQ 失败，用户不存在。", $"[{GetCurrentTime}]");
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
                return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
            }

            // *注：qq的开发者id申请不下来，只能用手输token的方式验证了
            var verifyToken = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), user.ID.ToString(), "reg");

            // 成功
            return responseService.ResponseQQVerify(HttpStatusCodes.Ok, verifyToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} LinkQQ 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }
}

[Route("[controller]")]
public class QQUnLinkController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "UnlinkQQ")]
    public async Task<ActionResult> GetAuthorizeLinkAsync()
    {
        // 检查用户Token是否有效并从中获取信息
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userId, out var mailAddr, out var token))
        {
            logger.LogWarning("{CurrentTime} UnlinkQQ 中递交了无效的Token。", $"[{GetCurrentTime}]");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");
        }

        // 获取用户信息
        try
        {
            (var user, var qq, var osu, var discord, var badges) = await ControllerUtils.GetFullUserInfoAsync(userId);
            if (user is null)
            {
                logger.LogWarning("{CurrentTime} UnlinkQQ 失败，用户不存在。", $"[{GetCurrentTime}]");
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
                return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
            }

            // 解绑
            if (!await DB.UnLinkQQAccounts(user.ID))
            {
                logger.LogWarning("{CurrentTime} UnlinkQQ 失败。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
            }

            // 成功
            return responseService.Response(HttpStatusCodes.Ok, "Successfully unlinked.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} UnlinkQQ 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }
}
