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
using System.Net;

namespace WebAPI.Controllers.OSU;

[ApiController]
[Route("[controller]")]
public class OSULinkController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "OsuLink")]
    public async Task<ActionResult> GetAuthorizeLinkAsync()
    {
        // 检查用户Token是否有效并从中获取信息
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userId, out var mailAddr, out var token))
        {
            logger.LogWarning("{CurrentTime} OsuLink 中递交了无效的Token。", $"[{GetCurrentTime}]");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");
        }

        // 获取用户信息
        try
        {
            (var user, var qq, var osu, var discord, var badges) = await ControllerUtils.GetFullUserInfoAsync(userId);
            if (user is null)
            {
                logger.LogWarning("{CurrentTime} OsuLink 失败，用户不存在。", $"[{GetCurrentTime}]");
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
                return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
            }
            if (osu is not null)
            {
                logger.LogWarning("{CurrentTime} OsuLink 失败，用户已绑定Discord。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.BadRequest, "Your account is currently linked to osu! account.");
            }

            return responseService.Response(HttpStatusCodes.Ok, JsonConvert.SerializeObject($"{config.Osu!.AuthorizeUrl}" +
                $"?client_id={config.Osu!.ClientId}&response_type=code&scope=public&redirect_uri={config.Osu!.RedirectUrl}"));
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} OsuLink 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }
}

[Route("/callback/[controller]")]
public class OSUCallbackController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "OsuCallBack")]
    public async Task<ActionResult> GetAuthorizeLinkAsync(string? code)
    {
        // 检查用户Token是否有效并从中获取信息
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var userId, out var mailAddr, out var token))
        {
            logger.LogWarning("{CurrentTime} OsuLinkCallBack 中递交了无效的Token。", $"[{GetCurrentTime}]");
            HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
            return responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");
        }

        // 获取用户信息
        try
        {
            (var user, var qq, var osu, var discord, var badges) = await ControllerUtils.GetFullUserInfoAsync(userId);
            if (user is null)
            {
                logger.LogWarning("{CurrentTime} OsuLinkCallBack 失败，用户不存在。", $"[{GetCurrentTime}]");
                HttpContext.Response.Cookies.Append("token", "", cookies.Expire); // 强制登出
                return responseService.Response(HttpStatusCodes.Unauthorized, "User logged in but not found in database.");
            }

            // 检查回调代码
            if (string.IsNullOrEmpty(code))
                return responseService.Response(HttpStatusCodes.BadRequest, "Invalid operation. Please provide a valid code.");

            // 尝试获取osu! uid
            var osuUid_s = await getOsuUIDAsync(code);
            if (osuUid_s is null)
                return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
            var osuUid = long.Parse(osuUid_s);

            // 检查该osu!账户是否已被他人绑定
            if (await DB.IsOsuAccountAlreadyLinked(osuUid))
                return responseService.Response(HttpStatusCodes.Forbidden, "The provided osu! account has been linked by other desu.life user.");

            // 执行绑定
            if (!await DB.LinkOsuAccount(user.ID, osuUid))
            {
                logger.LogWarning("{CurrentTime} OsuLinkCallBack 失败，向数据库中更新内容失败。", $"[{GetCurrentTime}]");
                return responseService.Response(HttpStatusCodes.InternalServerError, "An error occurred while link with discord account. Please contact the administrator.");
            }

            // 成功
            logger.LogInformation("{CurrentTime} OsuLinkCallBack 成功，用户 {UID} 成功绑定了osu!账户。", $"[{GetCurrentTime}]", user.ID);
            return responseService.Response(HttpStatusCodes.Ok, $"Link successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("{CurrentTime} OsuLinkCallBack 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error has occurred.");
        }
    }

    private async Task<string?> getOsuUIDAsync(string code)
    {
        JObject responseBody;
        try
        {
            var requestData = new
            {
                grant_type = "authorization_code",
                client_id = config.Osu!.ClientId,
                client_secret = config.Osu!.ClientSecret,
                code = code,
                redirect_uri = config.Osu!.RedirectUrl
            };

            var response = await config.Osu.TokenUrl
                .WithHeader("Content-type", "application/json")
                .PostJsonAsync(requestData);
            responseBody = JsonConvert.DeserializeObject<JObject>((await response.GetStringAsync()))!;
        }
        catch (FlurlHttpException ex)
        {
            logger.LogWarning("{CurrentTime} Osu!API 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return null;
        }

        // get osu user info
        string access_token = responseBody["access_token"]!.ToString();
        try
        {
            var response = await $"{config.Osu.APIBaseUrl}/me"
                .WithHeader("Authorization", $"Bearer {access_token}")
                .GetAsync();
            responseBody = JsonConvert.DeserializeObject<JObject>((await response.GetStringAsync()))!;
        }
        catch (FlurlHttpException ex)
        {
            logger.LogWarning("{CurrentTime} Osu!API 失败。错误信息：{Message}", $"[{GetCurrentTime}]", ex.Message);
            return null;
        }

        // get osu user id from response data
        if (responseBody["id"] == null)
        {
            logger.LogWarning("{CurrentTime} DiscordAPI 失败，无法从应答中获取discord id。", $"[{GetCurrentTime}]");
            return null;
        }
        var osu_uid = responseBody["id"]!.ToString();

        return osu_uid;
    }
}
