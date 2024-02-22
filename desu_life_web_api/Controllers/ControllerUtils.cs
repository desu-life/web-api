using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebAPI.Security;
using WebAPI.Database.Models;

namespace WebAPI.Controllers;

public static class ControllerUtils
{
    public static async Task<(User? user, List<BindQQ> qq, BindOSU? osu, BindDiscord? discord, List<UserBadges> badges)> GetFullUserInfoAsync(long uid)
    {
        var user = await DB.GetUserByUserID(uid);
        var qq = await DB.GetQQAccountInfo(uid);
        var discord = await DB.GetDiscordAccountInfo(uid);
        var osu = await DB.GetOsuAccountInfo(uid);
        var badges = await DB.GetUserBadges(uid);
        return (user, qq, osu, discord, badges);
    }
}
