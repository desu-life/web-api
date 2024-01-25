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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using static desu_life_web_backend.ResponseService;

namespace desu_life_web_backend.Controllers.Registration
{

    [ApiController]
    [Route("[controller]")]
    public class registrationController(ILogger<SystemMsg> logger, ResponseService responseService) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<SystemMsg> _logger = logger;
        private readonly ResponseService _responseService = responseService;

        [HttpGet(Name = "Registration")]
        public async Task<ActionResult<SystemMsg>> GetAuthorizeLinkAsync(string name)
        {
            var email = Request.Query["email"].ToString();
            var password = Request.Query["password"].ToString();

            if (email == "")
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Please provide complete email."
                });
            }

            // check if user is registered
            if (await Database.Client.CheckUserIsRegistered(email))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "The provided email address has been registered."
                }
                );
            }

            // create new token
            if (!await Database.Client.AddVerifyToken(8600, "link", "osu", DateTimeOffset.Now.AddHours(1), "new token here"))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Token generate failed. Please contact Administrator."
                });
            }

            // send reg email
            try
            {
                await Mail.SendVerificationMail(email, "new code here", "desulife");
            }
            catch
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "An error occurred while sending the registration email."
                }
                );
            }

            return Ok(new SystemMsg
            {
                Status = "success",
                Msg = "The registration mail has been sent."
            });
        }
    }
}
