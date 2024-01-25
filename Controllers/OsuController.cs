using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static desu_life_web_backend.ResponseService;
using static desu_life_web_backend.Security;

namespace desu_life_web_backend.Controllers.OSU;

[ApiController]
[Route("[controller]")]
public class osu_linkController(ILogger logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.inner!;
    private readonly ILogger _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpGet(Name = "OsuLink")]
    public async Task<ActionResult> GetAuthorizeLinkAsync(long? uid)
    {
        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return _responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

        // check uid parameter
        if (!uid.HasValue)
            return _responseService.Response(HttpStatusCodes.BadRequest, "No uid provided.");

        // check user's links
        if (await Database.Client.CheckCurrentUserHasLinkedOSU(uid!.Value))
            return _responseService.Response(HttpStatusCodes.Conflict, "Your account is currently linked to osu! account.");

        // create new verify token and update
        var token = Utils.GenerateRandomString(64);
        HttpContext.Response.Cookies.Append("token", UpdateVerifyTokenFromToken(HttpContext.Request.Cookies, token), Cookies.Default);
        if (!await Database.Client.AddVerifyToken(uid.Value, "link", "osu", DateTimeOffset.Now.AddHours(1), token))
            return _responseService.Response(HttpStatusCodes.BadRequest, "Token generate failed. Please contact Administrator.");

        // success
        return _responseService.Redirect($"{config.osu!.AuthorizeUrl}?client_id={config.osu!.clientId}&response_type=code&scope=public&redirect_uri={config.osu!.RedirectUrl}");
    }
}

[Route("/callback/[controller]")]
public class osu_callbackController(ILogger<SystemMsg> logger, ResponseService responseService) : ControllerBase
{
    private static Config.Base config = Config.inner!;
    private readonly ILogger<SystemMsg> _logger = logger;
    private readonly ResponseService _responseService = responseService;

    [HttpGet(Name = "OsuCallBack")]
    public async Task<ActionResult<SystemMsg>> GetAuthorizeLinkAsync(string? code)
    {
        // check if user token is valid
        if (!JWT.CheckJWTTokenIsVaild(HttpContext.Request.Cookies))
            return _responseService.Response(HttpStatusCodes.Unauthorized, "Invalid request.");

        // get info from token
        long uid = GetUserIDFromToken(HttpContext.Request.Cookies);

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
            //_logger.LogError();
            return _responseService.Response(HttpStatusCodes.BadRequest, ex.StatusCode == 400 ? "Request failed." : $"Exception with code({ex.StatusCode}): {ex.Message}");
        }

        // get osu user id from response data
        if (responseBody["id"] == null)
            return _responseService.Response(HttpStatusCodes.BadRequest, "Something went wrong with the request.");
        var osu_uid = responseBody["id"]!.ToString();

        // check if the osu user has linked to another desu.life account.
        if (await Database.Client.OSUCheckUserHasLinkedByOthers(osu_uid))
            return _responseService.Response(HttpStatusCodes.BadRequest, "The provided osu! account has been linked by other desu.life user.");

        // virefy the operation Token
        if (!await Database.Client.CheckUserTokenValidity(uid, GetVerifyTokenFromToken(HttpContext.Request.Cookies), "link", "osu"))
            return _responseService.Response(HttpStatusCodes.BadRequest, "Invaild Token.");

        // execute link
        if (!await Database.Client.InsertOsuUser(uid, long.Parse(osu_uid)))
            return _responseService.Response(HttpStatusCodes.BadRequest, "An error occurred while link with osu! account. Please contact the administrator.");

        // success
        return _responseService.Response(HttpStatusCodes.Ok, $"Link successfully.");
    }
}
