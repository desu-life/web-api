using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using WebAPI.Database;
using static WebAPI.Security.Token;
using System.Net;
using WebAPI.Database.Models;

namespace WebAPI.Controllers.Discord;


[ApiController]
[Route("[controller]")]
public class DiscordLinkController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "LinkDiscord")]
    public async Task<ActionResult> GetAuthorizeLinkAsync()
    {
        // 检查用户Token是否有效并从中获取信息
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userId, out var mailAddr, out var token))
        {
            logger.LogWarning("{CurrentTime} LinkDiscord 中递交了无效的Token。", $"[{GetCurrentTime}]");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");
        }

        // 获取用户信息
        try
        {
            (var user, var qq, var osu, var discord, var badges) = await ControllerUtils.GetFullUserInfoAsync(userId);
            if (user is null)
            {
                logger.LogWarning("{CurrentTime} LinkDiscord 失败，用户不存在。", $"[{GetCurrentTime}]");
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
                return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
            }
            if (discord is not null)
            {
                logger.LogWarning("{CurrentTime} LinkDiscord 失败，用户已绑定Discord。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.BadRequest, "Your account is currently linked to discord account.");
            }

            return responseService.Response(HttpStatusCodes.Ok, JsonConvert.SerializeObject($"{config.Discord!.AuthorizeUrl}" +
            $"?client_id={config.Discord!.ClientId}&response_type=code&scope=identify&redirect_uri={config.Discord!.RedirectUrl}"));
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} LinkDiscord 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }
}

[Route("/callback/[controller]")]
public class DiscordCallbackController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "Discord")]
    public async Task<ActionResult> GetAuthorizeLinkAsync(string? code)
    {
        // 检查用户Token是否有效并从中获取信息
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userId, out var mailAddr, out var token))
        {
            logger.LogWarning("{CurrentTime} DiscordLinkCallBack 中递交了无效的Token。", $"[{GetCurrentTime}]");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");
        }

        // 获取用户信息
        try
        {
            (var user, var qq, var osu, var discord, var badges) = await ControllerUtils.GetFullUserInfoAsync(userId);
            if (user is null)
            {
                logger.LogWarning("{CurrentTime} DiscordLinkCallBack 失败，用户不存在。", $"[{GetCurrentTime}]");
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
                return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
            }

            // 检查回调代码
            if (string.IsNullOrEmpty(code))
                return responseService.Response(HttpStatusCodes.BadRequest, "Invalid operation. Please provide a valid code.");

            // 尝试获取discord uid
            var discordUid = await getDiscordIDAsync(code);
            if (discordUid is null)
                return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");

            // 检查该dc账户是否已被他人绑定
            if (await DB.IsDiscordAccountAlreadyLinked(discordUid))
                return responseService.Response(HttpStatusCodes.Forbidden, "The provided discord account has been linked by other desu.life user.");

            // 执行绑定
            if (!await DB.LinkDiscordAccount(user.ID, discordUid))
            {
                logger.LogWarning("{CurrentTime} DiscordLinkCallBack 失败，向数据库中更新内容失败。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.InternalServerError, "An error occurred while link with discord account. Please contact the administrator.");
            }

            // 成功
            logger.LogInformation("{CurrentTime} DiscordLinkCallBack 成功，用户 {UID} 成功绑定了Discord账户。", $"[{GetCurrentTime}]", user.ID);
            return responseService.Response(HttpStatusCodes.Ok, $"Link successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} DiscordLinkCallBack 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }

    private async Task<string?> getDiscordIDAsync(string code)
    {
        JObject responseBody;
        try
        {
            var requestData = new
            {
                grant_type = "authorization_code",
                client_id = config.Discord!.ClientId,
                client_secret = config.Discord!.ClientSecret,
                scope = "identify",
                code = code,
                redirect_uri = config.Discord!.RedirectUrl
            };

            var response = await config.Discord.TokenUrl
                .WithHeader("Content-type", "application/x-www-form-urlencoded")
                .PostUrlEncodedAsync(requestData);
            responseBody = JsonConvert.DeserializeObject<JObject>((await response.GetStringAsync()))!;
        }
        catch (FlurlHttpException ex)
        {
            logger.LogWarning("{CurrentTime} DiscordAPI 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return null;
        }

        // get discord user info
        string access_token = responseBody["access_token"]!.ToString();
        try
        {
            var response = await $"{config.Discord.APIBaseUrl}/users/@me"
                .WithHeader("Authorization", $"Bearer {access_token}")
                .GetAsync();
            responseBody = JsonConvert.DeserializeObject<JObject>((await response.GetStringAsync()))!;
        }
        catch (FlurlHttpException ex)
        {
            logger.LogWarning("{CurrentTime} DiscordAPI 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return null;
        }

        // get discord user id from response data
        if (responseBody["id"] == null)
        {
            logger.LogWarning("{CurrentTime} DiscordAPI 失败，无法从应答中获取discord id。", $"[{GetCurrentTime}]");
            return null;
        }
        var discord_uid = responseBody["id"]!.ToString();

        return discord_uid;
    }
}

[Route("[controller]")]
public class DiscordUnLinkController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "UnlinkDiscord")]
    public async Task<ActionResult> GetAuthorizeLinkAsync()
    {
        // 检查用户Token是否有效并从中获取信息
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userId, out var mailAddr, out var token))
        {
            logger.LogWarning("{CurrentTime} UnlinkDiscord 中递交了无效的Token。", $"[{GetCurrentTime}]");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");
        }

        // 获取用户信息
        try
        {
            (var user, var qq, var osu, var discord, var badges) = await ControllerUtils.GetFullUserInfoAsync(userId);
            if (user is null)
            {
                logger.LogWarning("{CurrentTime} UnlinkDiscord 失败，用户不存在。", $"[{GetCurrentTime}]");
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
                return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
            }

            // 解绑
            if (!await DB.UnLinkDiscordAccount(user.ID))
            {
                logger.LogWarning("{CurrentTime} UnlinkDiscord 失败。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
            }

            // 成功
            return responseService.Response(HttpStatusCodes.Ok, "Successfully unlinked.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} UnlinkDiscord 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }
}
