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
using static desu_life_web_backend.ReturnRequests;
using static desu_life_web_backend.Cookies;

namespace desu_life_web_backend.Controllers.Login
{
    [ApiController]
    [Route("[controller]")]
    public class logoutController(ILogger<SystemMsg> logger) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<SystemMsg> _logger = logger;

        [HttpGet(Name = "Logout")]
        public ActionResult<SystemMsg> ExecuteLogOut()
        {
            HttpContext.Response.Cookies.Append("token", "", Expire);
            return Request(Enums.Ok, "Successfully logged out.");
        }
    }


    [Route("[controller]")]
    public class loginController(ILogger<SystemMsg> logger) : ControllerBase
    {
        private static Config.Base config = Config.inner!;
        private readonly ILogger<SystemMsg> _logger = logger;

        [HttpGet(Name = "Login")]
        public async Task<ActionResult<SystemMsg>> ExecuteLoginAsync(string? mailAddr, string? password)
        {
            // check if user token is valid
            if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Already logged in."
                });
            }

            if (string.IsNullOrEmpty(mailAddr) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "Please provide complete email or password."
                });
            }

            // check user validity
            var userId = await Database.Client.CheckUserIsValidity(mailAddr, password);
            if (userId < 0)
            {
                return BadRequest(new SystemMsg
                {
                    Status = "failed",
                    Msg = "User does not exist or password is incorrect."
                }
                );
            }

            // create new token
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(10), // same as JWT expire time
                HttpOnly = true // no JavaScript access
            };
            HttpContext.Response.Cookies.Append("token", JWT.CreateJWTToken(userId, null), cookieOptions);

            return Ok(new SystemMsg
            {
                Status = "success",
                Msg = "Successfully logged in.",
            });
        }
    }
}
