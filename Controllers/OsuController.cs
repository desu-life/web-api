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

namespace desu_life_web_backend.Controllers.OSU
{

    [ApiController]
    [Route("[controller]")]
    public class osu_linkController : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<SystemMsg> _logger;

        public osu_linkController(ILogger<SystemMsg> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "OsuLink")]
        public async Task<ActionResult<SystemMsg>> GetAuthorizeLinkAsync()
        {

            // check user's links
            if (await Database.Client.CheckCurrentUserHasLinkedOSU(8600))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Your account is currently linked to osu! account."
                }
                );
            }

            string osuAuthUrl = $"{config.osu!.AuthorizeUrl}?client_id={config.osu!.clientId}&response_type=code&scope=public&redirect_uri={config.osu!.RedirectUrl}";

            // create new token
            if (!await Database.Client.AddVerifyToken(8600, "link", "osu", DateTimeOffset.Now.AddHours(1), "new token here"))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Token generate failed. Please contact Administrator."
                });
            }

            return Ok(new SystemMsg
            {
                Status = "success",
                Msg = "Redirect to this URL.",
                Url = osuAuthUrl
            });
        }
    }

    [Route("/callback/[controller]")]
    public class osu_callbackController : ControllerBase
    {
        private static Config.Base config = Config.inner!;

        private readonly ILogger<SystemMsg> _logger;

        public osu_callbackController(ILogger<SystemMsg> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "OsuCallBack")]
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

            // get osu temporary token
            JObject responseBody;
            try
            {
                var requestData = new
                {
                    grant_type = "authorization_code",
                    client_id = config.osu!.clientId,
                    client_secret = config.osu!.clientSecret,
                    code = QueryString,
                    redirect_uri = config.osu!.RedirectUrl
                };

                var response = await config.osu.TokenUrl
                    .WithHeader("Content-type", "application/json")
                    .PostJsonAsync(requestData);
                responseBody = JsonConvert.DeserializeObject<JObject>((await response.GetStringAsync()))!;
            }
            catch (FlurlHttpException ex)
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}"
                }); ;
            }

            // get osu user info
            string access_token = responseBody["access_token"]!.ToString();
            try
            {
                var response = await $"{config.osu.APIBaseUrl}/me"
                    .WithHeader("Authorization", $"Bearer {access_token}")
                    .GetAsync();
                responseBody = JsonConvert.DeserializeObject<JObject>((await response.GetStringAsync()))!;
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine("Response Content: " + await ex.GetResponseStringAsync());
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}"
                }); ;
            }

            // get osu user id from response data
            var osu_uid = responseBody["id"].ToString();

            // check if the osu user has linked to another desu.life account.

            if (await Database.Client.OSUCheckUserHasLinkedByOthers(osu_uid))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "The provided osu! account has been linked by other desu.life user."
                }
                );

            }
            // virefy the operation Token
            // sql = "SELECT * FROM user_verify WHERE uid = '{$uid}' AND token = '{$token}' AND op = 'link' AND platform = 'osu'";
            if (!await Database.Client.CheckUserAccessbility(8600, "new token here", "link", "osu"))
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
            if (!await Database.Client.InsertOsuUser(8600, long.Parse(osu_uid)))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "An exception detected when trying to link with osu! account. Please contact the administrator."
                }
                );
            }

            return Ok(new SystemMsg
            {
                Status = "success",
                Msg = $"Link successfully. osu uid: {osu_uid}"
            });

        }
    }
}
