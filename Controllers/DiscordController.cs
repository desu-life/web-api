using Flurl.Http;
using Flurl.Util;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static desu_life_web_backend.ReturnRequests;

namespace desu_life_web_backend.Controllers.Discord
{

    [ApiController]
    [Route("[controller]")]
    public class discord_linkController : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<SystemMsg> _logger;

        public discord_linkController(ILogger<SystemMsg> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "DiscordLink")]
        public async Task<ActionResult<SystemMsg>> GetAuthorizeLinkAsync()
        {

            // check user's links
            if (await Database.Client.CheckCurrentUserHasLinkedDiscord(8600))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Your account is currently linked to discord account."
                }
                );
            }

            string discordAuthUrl = $"{config.discord!.AuthorizeUrl}?client_id={config.discord!.clientId}&response_type=code&scope=identify&redirect_uri={config.discord!.RedirectUrl}";

            // create new token
            if (!await Database.Client.AddVerifyToken(8600, "link", "discord", DateTimeOffset.Now.AddHours(1), "new token here"))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Token generate failed. Please contact Administrator."
                });
            }




            //Response.Redirect(discordAuthUrl);

            return Ok(new SystemMsg
            {
                Status = "success",
                Msg = "Redirect to this URL.",
                Url = discordAuthUrl
            });
        }
    }

    [Route("/callback/[controller]")]
    public class discord_callbackController : ControllerBase
    {
        private static Config.Base config = Config.inner!;

        private readonly ILogger<SystemMsg> _logger;

        public discord_callbackController(ILogger<SystemMsg> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "DiscordCallBack")]
        public async Task<ActionResult<SystemMsg>> GetAuthorizeLinkAsync(string? code)
        {
            if (code == null)
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Invalid operation. Please provide a valid code."
                });
            }

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
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}"
                }); ;
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
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}"
                }); ;
            }

            // get osu user id from response data
            var discord_uid = responseBody["id"].ToString();

            // check if the osu user has linked to another desu.life account.

            if (await Database.Client.DiscordCheckUserHasLinkedByOthers(discord_uid))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "The provided discord account has been linked by other desu.life user."
                }
                );

            }
            // virefy the operation Token
            // sql = "SELECT * FROM user_verify WHERE uid = '{$uid}' AND token = '{$token}' AND op = 'link' AND platform = 'osu'";
            if (!await Database.Client.CheckUserAccessbility(8600, "new token here", "link", "discord"))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Invaild Token."
                }
                );
            }

            // execute link op
            // uid=8600 *test uid
            if (!await Database.Client.LinkDiscordAccount(8600, discord_uid))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "An error occurred while link with discord account. Please contact the administrator."
                }
                );
            }

            return Ok(new SystemMsg
            {
                Status = "success",
                Msg = $"Link successfully. discord uid: {discord_uid}"
            });

        }
    }
}
