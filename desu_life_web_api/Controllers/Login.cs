using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static desu_life_web_api.Database.Models;
using desu_life_web_api.Response;
using desu_life_web_api.Cookie;
using desu_life_web_api.Request;
using desu_life_web_api.Security;
using desu_life_web_api.Http;
using desu_life_web_api.Database;
using static desu_life_web_api.Security.Token;

namespace desu_life_web_api.Controllers.Login;

[ApiController]
[Route("[controller]")]
public class logoutController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.inner!;
    private readonly ILogger<Log> _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpPost(Name = "Logout")]
    public ActionResult ExecuteLogOut()
    {
        HttpContext.Response.Cookies.Append("token", "", cookies.Expire);
        return _responseService.Response(HttpStatusCodes.Ok, "Successfully logged out.");
    }
}


[Route("[controller]")]
public class loginController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.inner!;
    private readonly ILogger<Log> _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpPost(Name = "Login")]
    public async Task<ActionResult> ExecuteLoginAsync([FromBody] Request.LoginRequest request)
    {
        // check if user token is valid
        // if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
        //     return _responseService.Response(HttpStatusCodes.NoContent, ""); //"Already logged in.");

        // check email&password
        if (string.IsNullOrEmpty(request.MailAddress) || string.IsNullOrEmpty(request.Password))
            return _responseService.Response(HttpStatusCodes.BadRequest, "Please provide complete email or password.");

        // check user validity
        var userId = await Database.Client.CheckUserIsValidity(request.MailAddress, request.Password);
        if (userId < 0)
            return _responseService.Response(HttpStatusCodes.BadRequest, "User does not exist or password is incorrect.");

        // create new token
        // string new_token = Security.SetLoginToken(userId, request.MailAddress);
        // Console.WriteLine(new_token);
        HttpContext.Response.Cookies.Append("token", SetLoginToken(userId, request.MailAddress), cookies.Default);

        // success
        _logger.LogInformation($"[{Utils.GetCurrentTime}] User {userId} logged in.");
        return _responseService.Response(HttpStatusCodes.Ok, "Successfully logged in.");
    }
}
