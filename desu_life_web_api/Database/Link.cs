using LinqToDB;
using MySqlConnector;
using WebAPI.Database.Models;

namespace WebAPI.Database;

public partial class Client
{
    public static async Task<bool> IsOsuAccountAlreadyLinked(long oid)
    {
        using var db = getInstance();
        var li = db.BindOSU.Where(it => it.OSUserId == oid);
        if (await li.CountAsync() > 0) return true;
        return false;
    }

    public static async Task<bool> IsDiscordAccountAlreadyLinked(string discordUid)
    {
        using var db = getInstance();
        var li = db.BindDiscord.Where(it => it.DiscordId == discordUid).Select(it => it.UserId);
        if (await li.CountAsync() > 0) return true;
        return false;
    }

    public static async Task<bool> LinkDiscordAccount(long uid, string discordUid)
    {
        using var db = getInstance();
        var res = await db.BindDiscord.Where(it => it.UserId == uid).Set(it => it.DiscordId, discordUid).UpdateAsync();
        return res > -1;
    }

    public static async Task<bool> UnLinkDiscordAccount(long uid)
    {
        using var db = getInstance();
        var res = await db.BindDiscord.Where(it => it.UserId == uid).Set(it => it.DiscordId).UpdateAsync();
        return res > 0;
    }

    public static async Task<bool> LinkOsuAccount(int uid, long oid)
    {
        using var db = getInstance();
        var d = new BindOSU() { UserId = uid, OSUserId = oid, OSUMode = "osu", CustomInfoEngineVersion = 2, InfoV2Mode = 1 };
        try { await db.InsertAsync(d); return true; }
        catch { return false; }
    }

    public static async Task<bool> UnLinkOsuAccount(long uid)
    {
        using var db = getInstance();
        var res = await db.BindOSU.Where(it => it.UserId == uid).DeleteAsync();
        return res > 0;
    }

    public static async Task<bool> UnLinkQQAccounts(long uid)
    {
        using var db = getInstance();
        var res = await db.BindQQ.Where(it => it.UserId == uid).DeleteAsync();
        return res > 0;
    }
}
