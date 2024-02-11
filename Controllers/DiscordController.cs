using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static desu_life_web_backend.Database.Models;
using static desu_life_web_backend.ResponseService;
using static desu_life_web_backend.Security;

namespace desu_life_web_backend.Controllers.Discord
{

    [ApiController]
    [Route("[controller]")]
    public class discord_linkController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<Log> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "DiscordLink")]
        public async Task<ActionResult> GetAuthorizeLinkAsync()
        {
            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] Discord link started by anonymous user.");

            // check if user token is valid
            if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

            // get info from token
            if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
                return _responseService.Response(HttpStatusCodes.InternalServerError, "User information check failed.");

            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] Discord link operation triggered by user {UserId}.");

            // check user's links
            if (await Database.Client.CheckCurrentUserHasLinkedDiscord(UserId))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Your account is currently linked to discord account.");

            // create new verify token and update
            var token = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), UserId.ToString(), "discordlink");

            // success
            return _responseService.Response(HttpStatusCodes.Ok, JsonConvert.SerializeObject($"{config.discord!.AuthorizeUrl}?client_id={config.discord!.clientId}&response_type=code&scope=identify&redirect_uri={config.discord!.RedirectUrl}"));
        }
    }

    [Route("/callback/[controller]")]
    public class discord_callbackController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<Log> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "DiscordCallBack")]
        public async Task<ActionResult> GetAuthorizeLinkAsync(string? code)
        {
            // log
            _logger.LogInformation($"[{Utils.GetCurrentTime}] Discord Callback triggerd.");

            // check if user token is valid
            if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

            // get info from token
            if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
                return _responseService.Response(HttpStatusCodes.InternalServerError, "User information check failed.");

            // check code
            if (string.IsNullOrEmpty(code))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Invalid operation. Please provide a valid code.");

            // get discord temporary token
            JObject responseBody;
            try
            {
                var requestData = new
                {
                    grant_type = "authorization_code",
                    client_id = config.discord!.clientId,
                    client_secret = config.discord!.clientSecret,
                    scope = "identify",
                    code = code,
                    redirect_uri = config.discord!.RedirectUrl
                };

                var response = await config.discord.TokenUrl
                    .WithHeader("Content-type", "application/x-www-form-urlencoded")
                    .PostUrlEncodedAsync(requestData);
                responseBody = JsonConvert.DeserializeObject<JObject>((await response.GetStringAsync()))!;
            }
            catch (FlurlHttpException ex)
            {
                // log
                _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred({ex.StatusCode}): {ex.Message}");
                return _responseService.Response(HttpStatusCodes.InternalServerError, ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}");
            }

            // get discord user info
            string access_token = responseBody["access_token"]!.ToString();
            try
            {
                var response = await $"{config.discord.APIBaseUrl}/users/@me"
                    .WithHeader("Authorization", $"Bearer {access_token}")
                    .GetAsync();
                responseBody = JsonConvert.DeserializeObject<JObject>((await response.GetStringAsync()))!;
            }
            catch (FlurlHttpException ex)
            {
                // log
                _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred({ex.StatusCode}): {ex.Message}");
                return _responseService.Response(HttpStatusCodes.InternalServerError, ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}");
            }

            // get discord user id from response data
            if (responseBody["id"] == null)
                return _responseService.Response(HttpStatusCodes.InternalServerError, "Something went wrong with the request.");
            var discord_uid = responseBody["id"]!.ToString();

            // check if the discord user has linked to another desu.life account.
            if (await Database.Client.DiscordCheckUserHasLinkedByOthers(discord_uid))
                return _responseService.Response(HttpStatusCodes.Forbidden, "The provided discord account has been linked by other desu.life user.");

            // execute link
            if (!await Database.Client.LinkDiscordAccount(UserId, discord_uid))
            {
                // log
                _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred while link with osu! account.");
                return _responseService.Response(HttpStatusCodes.InternalServerError, "An error occurred while link with osu! account. Please contact the administrator.");
            }

            // success
            _logger.LogInformation($"[{Utils.GetCurrentTime}] User {UserId} successfully linked to the discord account.");

            // need to redirect to the front-end page instead of this api
            return _responseService.Response(HttpStatusCodes.Ok, $"Link successfully.");
        }
    }
}
