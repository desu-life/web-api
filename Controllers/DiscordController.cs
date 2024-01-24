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
        public ActionResult<SystemMsg> GetAuthorizeLink()
        {
            string discordAuthUrl = $"{config.discord!.AuthorizeUrl}?client_id={config.discord!.clientId}&response_type=code&scope=identify&redirect_uri={config.discord!.RedirectUrl}";

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
        public async Task<ActionResult<SystemMsg>> GetAuthorizeLinkAsync()
        {
            var QueryString = Request.Query["code"].ToString();

            if (QueryString == "")
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
                    code = QueryString,
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

            return Ok(new SystemMsg
            {
                Status = "success",
                Msg = $"uid: {responseBody["id"]}"
            });

        }
    }
}