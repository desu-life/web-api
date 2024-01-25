using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static desu_life_web_backend.ResponseService;
using static desu_life_web_backend.Security;

namespace desu_life_web_backend.Controllers.Discord
{

    [ApiController]
    [Route("[controller]")]
    public class discord_linkController(ILogger<SystemMsg> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<SystemMsg> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "DiscordLink")]
        public async Task<ActionResult> GetAuthorizeLinkAsync(long? uid)
        {
            // check if user token is valid
            if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

            // check uid parameter
            if (!uid.HasValue)
                return _responseService.Response(HttpStatusCodes.BadRequest, "No uid provided.");

            // check user's links
            if (await Database.Client.CheckCurrentUserHasLinkedDiscord(uid!.Value))
                return _responseService.Response(HttpStatusCodes.Conflict, "Your account is currently linked to discord account.");


            // create new verify token and update
            var token = Utils.GenerateRandomString(64);
            HttpContext.Response.Cookies.Append("token", UpdateVerifyTokenFromToken(HttpContext.Request.Cookies, token), Cookies.Default);
            if (!await Database.Client.AddVerifyToken(uid.Value, "link", "discord", DateTimeOffset.Now.AddHours(1), token))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Token generate failed. Please contact Administrator.");

            // success
            return _responseService.Redirect($"{config.discord!.AuthorizeUrl}?client_id={config.discord!.clientId}&response_type=code&scope=identify&redirect_uri={config.discord!.RedirectUrl}");
        }
    }

    [Route("/callback/[controller]")]
    public class discord_callbackController(ILogger<SystemMsg> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<SystemMsg> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "DiscordCallBack")]
        public async Task<ActionResult> GetAuthorizeLinkAsync(string? code)
        {
            // check if user token is valid
            if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
                return _responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

            // get info from token
            long uid = GetUserIDFromToken(HttpContext.Request.Cookies);

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
                //Console.WriteLine("Response Content: " + await ex.GetResponseStringAsync());
                return _responseService.Response(HttpStatusCodes.BadRequest, ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}");
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
                // Console.WriteLine("Response Content: " + await ex.GetResponseStringAsync());
                return _responseService.Response(HttpStatusCodes.BadRequest, ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}");
            }

            // get discord user id from response data
            if (responseBody["id"] == null)
                return _responseService.Response(HttpStatusCodes.BadRequest, "Something went wrong with the request.");
            var discord_uid = responseBody["id"]!.ToString();

            // check if the discord user has linked to another desu.life account.
            if (await Database.Client.DiscordCheckUserHasLinkedByOthers(discord_uid))
                return _responseService.Response(HttpStatusCodes.BadRequest, "The provided discord account has been linked by other desu.life user.");

            // virefy the operation Token
            if (!await Database.Client.CheckUserTokenValidity(GetUserIDFromToken(HttpContext.Request.Cookies), GetVerifyTokenFromToken(HttpContext.Request.Cookies), "link", "discord"))
                return _responseService.Response(HttpStatusCodes.BadRequest, "Invaild Token.");

            // execute link
            if (!await Database.Client.LinkDiscordAccount(uid, discord_uid))
                return _responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while link with osu! account. Please contact the administrator.");

            // success
            return _responseService.Response(HttpStatusCodes.Ok, $"Link successfully.");
        }
    }
}
