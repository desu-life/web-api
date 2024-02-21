using LanguageExt.Pretty;
using LinqToDB;
using LinqToDB.Common;
using MySqlConnector;
using static WebAPI.Config;
using static WebAPI.Database.Connection;
using WebAPI.Database.Models;

namespace WebAPI.Database;

public class Client
{
    private static Config.Base config = Config.Inner!;

    private static Connection getInstance()
    {
        var options = new DataOptions().UseMySqlConnector(
            new MySqlConnectionStringBuilder
            {
                Server = config.Database!.Host,
                Port = (uint)config.Database.Port,
                UserID = config.Database.User,
                Password = config.Database.Password,
                Database = config.Database.DB,
                CharacterSet = "utf8mb4",
                CancellationTimeout = 5,
            }.ConnectionString
        );
        // 暂时只有Mysql
        return new Connection(options);
    }

    public static async Task<User?> GetUser(string mailAddr)
    {
        using var db = getInstance();
        return await db.Users.Where(it => it.Email == mailAddr).FirstOrDefaultAsync();
    }

    public static async Task<User?> GetUser(long userId)
    {
        using var db = getInstance();
        return await db.Users.Where(it => it.ID == userId).FirstOrDefaultAsync();
    }

    public static async Task<bool> OSUCheckUserHasLinkedByOthers(string OSUserId)
    {
        return await OSUCheckUserHasLinkedByOthers(long.Parse(OSUserId));
    }

    public static async Task<bool> OSUCheckUserHasLinkedByOthers(long OSUserId)
    {
        using var db = getInstance();
        var li = db.BindOSU.Where(it => it.OSUserId == OSUserId);
        if (await li.CountAsync() > 0)
            return true;
        return false;
    }

    public static async Task<bool> DiscordCheckUserHasLinkedByOthers(string discordUid)
    {
        using var db = getInstance();
        var li = db.Users.Where(it => it.discord_uid == discordUid).Select(it => it.uid);
        if (await li.CountAsync() > 0)
            return true;
        return false;
    }

    public static async Task<bool> CheckCurrentUserHasLinkedDiscord(int uid)
    {
        using var db = getInstance();
        var li = await db.BindDiscord.Where(it => it.UserId == uid).Select(it => it.DiscordId).FirstOrDefaultAsync();
        if (li != null)
            return true;
        return false;
    }

    public static async Task<bool> CheckCurrentUserHasLinkedOSU(int uid)
    {
        using var db = getInstance();
        var li = db.BindOSU.Where(it => it.UserId == uid);
        if (await li.CountAsync() > 0)
            return true;
        return false;
    }

    public static async Task<long> GetOsuUID(int uid)
    {
        using var db = getInstance();
        try
        {
            var li = await db.UsersOSU.Where(it => it.uid == uid).Select(it => it.osu_uid).FirstOrDefaultAsync();
            return li;
        }
        catch
        {
            return -1;
        }
    }

    public static async Task<bool> CheckUserIsRegistered(string email)
    {
        using var db = getInstance();
        var li = db.Users.Where(it => it.Email == email).Select(it => it.ID);
        if (await li.CountAsync() > 0)
            return true;
        return false;
    }

    public static async Task<long> CheckUserIsValidity(string email, string password)
    {
        using var db = getInstance();
        var li = db.Users.Where(it => it.Email == email && it.Password == password).Select(it => it.ID);
        if (await li.CountAsync() > 0)
        {
            return await li.FirstOrDefaultAsync();
        }
        return -1;
    }

    public static async Task<long> CheckUserIsExsit(string email)
    {
        using var db = getInstance();
        var li = db.Users.Where(it => it.Email == email).Select(it => it.ID);
        if (await li.CountAsync() > 0)
        {
            return await li.FirstOrDefaultAsync();
        }
        return -1;
    }

    public static async Task<bool> LinkDiscordAccount(long uid, string discordUid)
    {
        using var db = getInstance();
        var res = await db.BindDiscord.Where(it => it.DiscordId ==)
            .Where(it => it.id == uid)
            .Set(it => it.discord, discordUid)
            .UpdateAsync();

        return res > -1;
    }

    public static async Task<bool> InsertOsuUser(int uid, long oid)
    {
        using var db = getInstance();
        var d = new BindOSU()
        {
            UserId = uid,
            OSUserId = oid,
            OSUMode = "osu",
            CustomInfoEngineVersion = 2,
            InfoV2Mode = 1
        };
        try
        {
            await db.InsertAsync(d);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> InsertUser(string email, string password, string username)
    {
        using var db = getInstance();
        var d = new User()
        {
            Email = email,
            Password = password,
            PermissionGroupID = ???,
            UserName = username
        };
        try
        {
            await db.InsertAsync(d);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> UpdatePassword(long uid, string password)
    {
        using var db = getInstance();
        var res = await db.Users
            .Where(it => it.ID == uid)
            .Set(it => it.Password, password)
            .UpdateAsync();
        return res > -1;
    }

    public static async Task<bool> UpdatePassword(string mailAddr, string password)
    {
        using var db = getInstance();
        var res = await db.Users
            .Where(it => it.Email == mailAddr)
            .Set(it => it.Password, password)
            .UpdateAsync();
        return res > -1;
    }
}

