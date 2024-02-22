using LinqToDB;
using MySqlConnector;
using WebAPI.Database.Models;

namespace WebAPI.Database;

public partial class Client
{
    public static async Task<BindOSU?> GetOsuAccountInfo(long uid)
    {
        using var db = getInstance();
        return await db.BindOSU.Where(it => it.ID == uid).FirstOrDefaultAsync();
    }

    public static async Task<BindDiscord?> GetDiscordAccountInfo(long uid)
    {
        using var db = getInstance();
        return await db.BindDiscord.Where(it => it.ID == uid).FirstOrDefaultAsync();
    }

    public static async Task<List<BindQQ>> GetQQAccountInfo(long uid)
    {
        using var db = getInstance();
        return await db.BindQQ.Where(it => it.ID == uid).ToListAsync();
    }

    public static async Task<List<UserBadges>> GetUserBadges(long uid)
    {
        using var db = getInstance();
        return await db.UsersBadges.Where(it => it.ID == uid).ToListAsync();
    }

    public static async Task<bool> UpdateUserOsuDefaultMode(long uid, string mode)
    {
        using var db = getInstance();
        var res = await db.BindOSU.Where(it => it.ID == uid).Set(it => it.OSUMode, mode).UpdateAsync();
        return res > -1;
    }

}
