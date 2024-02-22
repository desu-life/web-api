using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Database.Models;
using WebAPI.Response;
using WebAPI.Cookie;
using WebAPI.Request;
using WebAPI.Security;
using WebAPI.Http;
using static WebAPI.Security.Token;
using WebAPI.Mail;

namespace WebAPI.Controllers.Registration;

[ApiController]
[Route("[controller]")]
public class RegistrationController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "Registration")]
    public async Task<ActionResult> ExecuteLinkAsync(string email)
    {
        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] Account registration started by anonymous user.");

        // check if the user is logged in
        // if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
        //     return _responseService.Response(HttpStatusCodes.NoContent, "Already logged in.");

        // empty check
        if (string.IsNullOrEmpty(email))
            return responseService.Response(HttpStatusCodes.BadRequest, "Please provide complete email.");

        // check if user is registered
        if (await Database.Client.CheckUserIsRegistered(email))
            return responseService.Response(HttpStatusCodes.Conflict, "The provided email address has been registered.");

        // create new verify token and update
        var token = GenerateVerifyToken(DateTimeOffset.Now.ToUnixTimeSeconds(), email, "reg");

        // send reg email
        try
        {
            await MailService.SendVerificationMail(email, token, "desulife", "reg");
        }
        catch
        {
            // log
            logger.LogError($"[{Utils.GetCurrentTime}] An error occurred while sending the registration email.");
            return responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while sending the registration email.");
        }

        // success
        return responseService.Response(HttpStatusCodes.Ok, "The registration mail has been sent.");
    }
}

[Route("[controller]")]
public class SetAccountController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.Inner!;
    private readonly ILogger<Log> logger = logger;
    private readonly ResponseService responseService = responseService;

    [HttpPost(Name = "SetAccount")]
    public async Task<ActionResult> GetAuthorizeLinkAsync([FromBody] RegistrationRequest request)
    //(string password, string email, string username, string Token)
    {
        // log
        logger.LogInformation($"[{Utils.GetCurrentTime}] Account Set started by anonymous user.");

        // check if the user is logged in
        // if (JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
        //     return _responseService.Response(HttpStatusCodes.NoContent, "Already logged in.");

        // empty check
        if (string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Token) ||
            string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.MailAddress))
            return responseService.Response(HttpStatusCodes.BadRequest, "Please provide full information.");

        // check if token is vaild
        var tempTokenData = request.Token.Split("[%*#]");
        if (request.Token != GenerateVerifyToken(long.Parse(tempTokenData[0]), request.MailAddress, "reg") || DateTimeOffset.Now > DateTimeOffset.Parse(tempTokenData[0]).AddHours(1))
            return responseService.Response(HttpStatusCodes.BadRequest, "Verification failed.");

        // execute reg
        if (!await Database.Client.InsertUser(request.MailAddress, request.Password, request.Username))
        {
            // log
            logger.LogError($"[{Utils.GetCurrentTime}] An error occurred while requesting registration");
            return responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while requesting registration. Please contact the administrator.");
        }

        // success
        logger.LogInformation($"[{Utils.GetCurrentTime}] Email {request.MailAddress} successfully registered.");
        return responseService.Response(HttpStatusCodes.Ok, "Registration success.");
    }
}
