using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using desu_life_web_api.Database.Models;
using desu_life_web_api.Response;
using desu_life_web_api.Cookie;
using desu_life_web_api.Request;
using desu_life_web_api.Security;
using desu_life_web_api.Http;
using static desu_life_web_api.Security.Token;
using desu_life_web_api.Mail;

namespace desu_life_web_api.Controllers.ResetPassword;

[ApiController]
[Route("[controller]")]
public class reset_password_verifyController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpGet(Name = "ResetPasswordVerify")]
    public async Task<ActionResult> ResetPasswordVerifyAsync(string mailAddr, string password)
    {
        // log
        _logger.LogInformation($"[{Utils.GetCurrentTime}] Password reset verify started by anonymous user.");

        // check if user logged in
        // if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
        //     return _responseService.Response(HttpStatusCodes.Found, "");

        // check user validity
        var userId = await Database.Client.CheckUserIsExsit(mailAddr);
        if (userId < 0)
            return _responseService.Response(HttpStatusCodes.Forbidden, "Invaild Operation.");

        // create new verify token and update
        var token = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), mailAddr, "resetpw");

        // send reg email
        try
        {
            await MailService.SendVerificationMail(mailAddr, token, "desulife", "resetPassword");
        }
        catch
        {
            // log
            _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred while sending the verify email.");
            return _responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while sending the verify email.");
        }

        // success
        return _responseService.Response(HttpStatusCodes.Ok, "The registration mail has been sent.");
    }
}

[Route("[controller]")]
public class reset_passwordController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpPost(Name = "ResetPassword")]
    public async Task<ActionResult> ExecuteResetPasswordAsync([FromBody] ResetPasswordRequest request)
        //(string password, string email, string Token)
    {
        // log
        _logger.LogInformation($"[{Utils.GetCurrentTime}] Password reset started by anonymous user.");

        // check if the user is logged in
        // if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
        //     return _responseService.Response(HttpStatusCodes.NoContent, "Already logged in.");

        // empty check
        if (string.IsNullOrEmpty(request.NewPassword))
            return _responseService.Response(HttpStatusCodes.BadRequest, "Please provide password.");

        // empty check
        if (string.IsNullOrEmpty(request.MailAddress) || string.IsNullOrEmpty(request.Token))
            return _responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");

        // check if token is vaild
        var tempTokenData = request.Token.Split("[%*#]");
        if (request.Token != GenerateVerifyToken(long.Parse(tempTokenData[0]), request.MailAddress, "resetpw") || DateTimeOffset.Now > DateTimeOffset.Parse(tempTokenData[0]).AddHours(1))
            return _responseService.Response(HttpStatusCodes.BadRequest, "Verification failed.");

        // update password
        if (!await Database.Client.UpdatePassword(request.MailAddress, request.NewPassword))
            return _responseService.Response(HttpStatusCodes.BadRequest, "Password update failed. Please contact the administrator.");

        // success
        _logger.LogInformation($"[{Utils.GetCurrentTime}] User {request.MailAddress} successfully updated the password.");

        // need to redirect to the front-end page instead of this api
        return _responseService.Response(HttpStatusCodes.Ok, $"Password update successfully.");
    }
}
