using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static desu_life_web_backend.ResponseService;
using static desu_life_web_backend.Security;
using static LinqToDB.Common.Configuration;

namespace desu_life_web_backend.Controllers.OSU;

[ApiController]
[Route("[controller]")]
public class osu_linkController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.inner!;
    private readonly ILogger<Log> _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpGet(Name = "OsuLink")]
    public async Task<ActionResult> GetAuthorizeLinkAsync()
    {
        // log
        _logger.LogInformation($"[{Utils.GetCurrentTime}] osu! Link started by anonymous user.");

        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return _responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");

        // get info from token
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
            return _responseService.Response(HttpStatusCodes.Forbidden, "User information check failed.");

        // log
        _logger.LogInformation($"[{Utils.GetCurrentTime}] osu! link operation triggered by user {UserId}.");

        // check user's links
        if (await Database.Client.CheckCurrentUserHasLinkedOSU(UserId))
            return _responseService.Response(HttpStatusCodes.Conflict, "Your account is currently linked to osu! account.");

        // create new verify token and update
        var token = Utils.GenerateRandomString(64);
        HttpContext.Response.Cookies.Append("token", UpdateVerifyTokenFromToken(HttpContext.Request.Cookies, token), Cookies.Default);
        if (!await Database.Client.AddVerifyToken(mailAddr, "link", "osu", DateTimeOffset.Now.AddHours(1), token))
        {
            // log
            _logger.LogError($"[{Utils.GetCurrentTime}] Token generate failed.");
            return _responseService.Response(HttpStatusCodes.Forbidden, "Token generate failed. Please contact Administrator.");
        }
            
        // success
        return _responseService.Redirect($"{config.osu!.AuthorizeUrl}?client_id={config.osu!.clientId}&response_type=code&scope=public&redirect_uri={config.osu!.RedirectUrl}");
    }
}

[Route("/callback/[controller]")]
public class osu_callbackController(ILogger<Log> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.inner!;
    private readonly ILogger<Log> _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpGet(Name = "OsuCallBack")]
    public async Task<ActionResult> GetAuthorizeLinkAsync(string? code)
    {
        // log
        _logger.LogInformation($"[{Utils.GetCurrentTime}] osu! Callback triggerd.");

        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return _responseService.Response(HttpStatusCodes.Forbidden, "Invalid request.");

        // get info from token
        if (!GetUserInfoFromToken(HttpContext.Request.Cookies, out var UserId, out var mailAddr, out var Token))
            return _responseService.Response(HttpStatusCodes.Forbidden, "User information check failed.");

        // check code
        if (string.IsNullOrEmpty(code))
            return _responseService.Response(HttpStatusCodes.BadRequest, "Invalid operation. Please provide a valid code.");

        // get osu temporary token
        JObject responseBody;
        try
        {
            var requestData = new
            {
                grant_type = "authorization_code",
                client_id = config.osu!.clientId,
                client_secret = config.osu!.clientSecret,
                code = code,
                redirect_uri = config.osu!.RedirectUrl
            };

            var response = await config.osu.TokenUrl
                .WithHeader("Content-type", "application/json")
                .PostJsonAsync(requestData);
            responseBody = JsonConvert.DeserializeObject<JObject>((await response.GetStringAsync()))!;
        }
        catch (FlurlHttpException ex)
        {
            // log
            _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred({ex.StatusCode}): {ex.Message}");
            return _responseService.Response(HttpStatusCodes.BadRequest, ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}");
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
            // log
            _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred({ex.StatusCode}): {ex.Message}");
            return _responseService.Response(HttpStatusCodes.BadRequest, ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}");
        }

        // get osu user id from response data
        if (responseBody["id"] == null)
            return _responseService.Response(HttpStatusCodes.BadRequest, "Something went wrong with the request.");
        var osu_uid = responseBody["id"]!.ToString();

        // check if the osu user has linked to another desu.life account.
        if (await Database.Client.OSUCheckUserHasLinkedByOthers(osu_uid))
            return _responseService.Response(HttpStatusCodes.Forbidden, "The provided osu! account has been linked by other desu.life user.");

        // virefy the operation Token
        if (!await Database.Client.CheckUserTokenValidity(mailAddr, Token ?? "", "link", "osu"))
            return _responseService.Response(HttpStatusCodes.Forbidden, "Invaild Token.");

        // execute link
        if (!await Database.Client.InsertOsuUser(UserId, long.Parse(osu_uid)))
        {
            // log
            _logger.LogError($"[{Utils.GetCurrentTime}] An error occurred while link with osu! account.");
            return _responseService.Response(HttpStatusCodes.Forbidden, "An error occurred while link with osu! account. Please contact the administrator.");
        }
            
        // success
        _logger.LogInformation($"[{Utils.GetCurrentTime}] User {UserId} successfully connected to the osu! account.");

        // need to redirect to the front-end page instead of this api
        return _responseService.Response(HttpStatusCodes.Ok, $"Link successfully.");
    }
}
