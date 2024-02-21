using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using desu_life_web_api.Http;

namespace desu_life_web_api.Response;

public class ResponseService
{
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
