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

namespace WebAPI.Controllers.Discord;


[ApiController]
[Route("[controller]")]
public class DiscordLinkController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "DiscordLink")]
    public async Task<ActionResult> GetAuthorizeLinkAsync()
    {
        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] Discord link started by anonymous user.");

        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

        // get info from token
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
            return responseService.Response(HttpStatusCodes.InternalServerError, "User information check failed.");

        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] Discord link operation triggered by user {UserId}.");

        // check user's links
        if (await Database.Client.CheckCurrentUserHasLinkedDiscord(UserId))
            return responseService.Response(HttpStatusCodes.BadRequest, "Your account is currently linked to discord account.");

        // create new verify token and update
        var token = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), UserId.ToString(), "discordlink");

        // success
        return responseService.Response(HttpStatusCodes.Ok, JsonConvert.SerializeObject($"{config.Discord!.AuthorizeUrl}" +
            $"?client_id={config.Discord!.ClientId}&response_type=code&scope=identify&redirect_uri={config.Discord!.RedirectUrl}"));
    }
}

[Route("/callback/[controller]")]
public class DiscordCallbackController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpGet(Name = "DiscordCallBack")]
    public async Task<ActionResult> GetAuthorizeLinkAsync(string? code)
    {
        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] Discord Callback triggerd.");

        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

        // get info from token
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
            return responseService.Response(HttpStatusCodes.InternalServerError, "User information check failed.");

        // check code
        if (string.IsNullOrEmpty(code))
            return responseService.Response(HttpStatusCodes.BadRequest, "Invalid operation. Please provide a valid code.");

        // get discord temporary token
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
            // log
            logger.LogError($"[{Utils.GetCurrentTime}] An error occurred({ex.StatusCode}): {ex.Message}");
            return responseService.Response(HttpStatusCodes.InternalServerError, ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}");
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
            // log
            logger.LogError($"[{Utils.GetCurrentTime}] An error occurred({ex.StatusCode}): {ex.Message}");
            return responseService.Response(HttpStatusCodes.InternalServerError, ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}");
        }

        // get discord user id from response data
        if (responseBody["id"] == null)
            return responseService.Response(HttpStatusCodes.InternalServerError, "Something went wrong with the request.");
        var discord_uid = responseBody["id"]!.ToString();

        // check if the discord user has linked to another desu.life account.
        if (await Database.Client.DiscordCheckUserHasLinkedByOthers(discord_uid))
            return responseService.Response(HttpStatusCodes.Forbidden, "The provided discord account has been linked by other desu.life user.");

        // execute link
        if (!await Database.Client.LinkDiscordAccount(UserId, discord_uid))
        {
            // log
            logger.LogError($"[{Utils.GetCurrentTime}] An error occurred while link with osu! account.");
            return responseService.Response(HttpStatusCodes.InternalServerError, "An error occurred while link with osu! account. Please contact the administrator.");
        }

        // success
        logger.LogInformation($"[{Utils.GetCurrentTime}] User {UserId} successfully linked to the discord account.");

        // need to redirect to the front-end page instead of this api
        return responseService.Response(HttpStatusCodes.Ok, $"Link successfully.");
    }
}
