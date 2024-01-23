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
        public ActionResult<SystemMsg> GetAuthorizeLink()
        {
            string discordAuthUrl = $"{config.osu!.AuthorizeUrl}?client_id={config.osu!.clientId}&response_type=code&scope=public&redirect_uri={config.osu!.RedirectUrl}";

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
                //var requestData = new JObject
                //{
                //    { "grant_type", "authorization_code" },
                //    { "client_id", config.osu?.clientId },
                //    { "client_secret", config.osu?.clientSecret },
                //    { "code", QueryString },
                //    { "redirect_uri", config.osu!.RedirectUrl }
                //};

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
                //Console.WriteLine("Response Content: " + await ex.GetResponseStringAsync());
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

            return Ok(new SystemMsg
            {
                Status = "success",
                Msg = $"uid: {responseBody["id"]}"
            });

        }
    }
}
