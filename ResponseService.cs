using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace desu_life_web_backend;

public class ResponseService
{
    public enum HttpStatusCodes
    {
        None = 0,

        // 1xx Informational
        Continue = 100,
        SwitchingProtocols = 101,
        Processing = 102,

        // 2xx Success
        Ok = 200,
        Created = 201,
        Accepted = 202,
        NonAuthoritativeInformation = 203,
        NoContent = 204,
        ResetContent = 205,
        PartialContent = 206,
        MultiStatus = 207,
        AlreadyReported = 208,
        IMUsed = 226,

        // 3xx Redirection
        MultipleChoices = 300,
        MovedPermanently = 301,
        Found = 302,
        SeeOther = 303,
        NotModified = 304,
        UseProxy = 305,
        TemporaryRedirect = 307,
        PermanentRedirect = 308,

        // 4xx Client Error
        BadRequest = 400,
        Unauthorized = 401,
        PaymentRequired = 402,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        NotAcceptable = 406,
        ProxyAuthenticationRequired = 407,
        RequestTimeout = 408,
        Conflict = 409,
        Gone = 410,
        LengthRequired = 411,
        PreconditionFailed = 412,
        RequestEntityTooLarge = 413,
        RequestUriTooLong = 414,
        UnsupportedMediaType = 415,
        RequestedRangeNotSatisfiable = 416,
        ExpectationFailed = 417,
        UnprocessableEntity = 422,
        Locked = 423,
        FailedDependency = 424,
        UpgradeRequired = 426,
        PreconditionRequired = 428,
        TooManyRequests = 429,
        RequestHeaderFieldsTooLarge = 431,
        UnavailableForLegalReasons = 451,

        // 5xx Server Error
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        HttpVersionNotSupported = 505,
        VariantAlsoNegotiates = 506,
        InsufficientStorage = 507,
        LoopDetected = 508,
        NotExtended = 510,
        NetworkAuthenticationRequired = 511
    }

    public ActionResult Response(HttpStatusCodes request, string message)
    {
        return new ObjectResult(message)
        {
            StatusCode = (int)request
        };
    }

    public ActionResult ResponseQQVerify(HttpStatusCodes request, string token)
    {
        return new ObjectResult(new QQVerifyResponse
        {
            token = token,
        })
        {
            StatusCode = (int)request
        };
    }

    public ActionResult ResponseUserInfo(HttpStatusCodes request, Database.Models.User UserInfo, long oid)
    {
        UserResponse responseUser = new UserResponse()
        {
            uid = UserInfo.uid,
            username = UserInfo.username,
            email = UserInfo.email,
            osu_uid = oid == -1 ? null : oid,
            qq_id = UserInfo.qq_id == 0 ? null : UserInfo.qq_id,
            qq_guild_uid = UserInfo.qq_guild_uid,
            kook_uid = UserInfo.kook_uid,
            discord_uid = UserInfo.discord_uid,
            permissions = UserInfo.permissions,
            displayed_badge_ids = UserInfo.displayed_badge_ids,
            owned_badge_ids = UserInfo.owned_badge_ids
        };

        responseUser.uid = UserInfo.uid;
        return new ObjectResult(responseUser)
        {
            StatusCode = (int)request
        };
    }

    public ActionResult Redirect(string url) //HttpContext httpContext)
    {
        //var redirectResult = new RedirectResult(url, true);
        //httpContext.Response.Headers.Add("Location", url);
        //return redirectResult;
        return new RedirectResult(url, true);
    }
}
