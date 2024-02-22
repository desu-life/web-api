using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Database.Models;
using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using static WebAPI.Security.Token;

namespace WebAPI.Controllers.Login;

[ApiController]
[Route("[controller]")]
public class LogoutController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "Logout")]
    public ActionResult ExecuteLogOut()
    {
        HttpContext.Response.Cookies.Append("token", "", cookies.Expire);
        return responseService.Response(HttpStatusCodes.Ok, "Successfully logged out.");
    }
}


[Route("[controller]")]
public class LoginController(ILogger<Log> logger, ResponseService responseService, Cookies cookies) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "Login")]
    public async Task<ActionResult> ExecuteLoginAsync([FromBody] Request.LoginRequest request)
    {
        // check if user token is valid
        // if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
        //     return _responseService.Response(HttpStatusCodes.NoContent, ""); //"Already logged in.");

        // check email&password
        if (string.IsNullOrEmpty(request.MailAddress) || string.IsNullOrEmpty(request.Password))
            return responseService.Response(HttpStatusCodes.BadRequest, "Please provide complete email or password.");

        // check user validity
        var userId = await Database.Client.CheckUserIsValidity(request.MailAddress, request.Password);
        if (userId < 0)
            return responseService.Response(HttpStatusCodes.BadRequest, "User does not exist or password is incorrect.");

        // create new token
        // string new_token = Security.SetLoginToken(userId, request.MailAddress);
        // Console.WriteLine(new_token);
        HttpContext.Response.Cookies.Append("token", SetLoginToken(userId, request.MailAddress), cookies.Default);

        // success
        logger.LogInformation($"[{Utils.GetCurrentTime}] User {userId} logged in.");
        return responseService.Response(HttpStatusCodes.Ok, "Successfully logged in.");
    }
}
