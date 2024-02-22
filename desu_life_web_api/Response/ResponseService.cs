using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Database.Models;
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

    public ActionResult ResponseUserInfo(HttpStatusCodes request, User? user, List<BindQQ> qq, BindOSU? osu, BindDiscord? discord, List<UserBadges> badges)
    {
        var responseUser = new UserResponse()
        {
            UserId = user!.ID,
            UserName = user.UserName,
            Email = user.Email,
            LastLoginIP = user.LastLoginIp,
            LastLoginTime = user.LastLoginTime.ToString(),
            PermissionGroup = user.PermissionGroupID,
            Badges = badges,
            BindDiscord = discord,
            BindQQ = qq,
            BindOsu = osu
        };
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
