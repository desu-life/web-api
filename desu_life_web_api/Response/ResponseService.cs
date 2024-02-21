using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Http;
using Models = WebAPI.Database.Models;

namespace WebAPI.Response;

public class Service
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
            Token = token,
        })
        {
            StatusCode = (int)request
        };
    }

    public ActionResult ResponseUserInfo(HttpStatusCodes request, Models.User UserInfo, long oid)
    {
        UserResponse responseUser = new UserResponse()
        {
            Uid = UserInfo.id,
            Username = UserInfo.username,
            Email = UserInfo.email,
            osu_uid = oid == -1 ? null : oid,
            Qq_id = UserInfo.qq_id == 0 ? null : UserInfo.qq_id,
            qq_guild_uid = UserInfo.qq_guild_uid,
            kook_uid = UserInfo.kook_uid,
            DiscordUid = UserInfo.discord_uid,
            Permissions = UserInfo.permissions,
            Displayed_badge_ids = UserInfo.displayed_badge_ids,
            owned_badge_ids = UserInfo.owned_badge_ids
        };

        responseUser.Uid = UserInfo.uid;
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
