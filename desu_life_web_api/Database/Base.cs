using LinqToDB;
using MySqlConnector;
using WebAPI.Database.Models;

namespace WebAPI.Database;

public partial class Client
{
    public static async Task<bool> InsertUser(string email, string password, string username)
    {
        using var db = getInstance();
        var d = new User() { PermissionGroupID = 2, Email = email, Password = password, UserName = username };
        try { await db.InsertAsync(d); return true; }
        catch { return false; }
    }

    public static async Task<User?> GetUserByEmail(string mailAddr)
    {
        using var db = getInstance();
        return await db.Users.Where(it => it.Email == mailAddr).FirstOrDefaultAsync();
    }

    public static async Task<User?> GetUserByUsername(string username)
    {
        using var db = getInstance();
        return await db.Users.Where(it => it.UserName == username).FirstOrDefaultAsync();
    }

    public static async Task<User?> GetUserByUserID(long userId)
    {
        using var db = getInstance();
        return await db.Users.Where(it => it.ID == userId).FirstOrDefaultAsync();
    }

    public static async Task<long?> ValidateUserCredentials(string email, string password)
    {
        using var db = getInstance();
        var userId = await db.Users.Where(it => it.Email == email && it.Password == password)
                               .Select(it => (long)it.ID)
                               .FirstOrDefaultAsync();
        return userId;
    }

    public static async Task<bool> UpdatePassword(long uid, string password)
    {
        using var db = getInstance();
        var res = await db.Users.Where(it => it.ID == uid).Set(it => it.Password, password).UpdateAsync();
        return res > -1;
    }

    public static async Task<bool> UpdatePassword(string email, string password)
    {
        using var db = getInstance();
        var res = await db.Users.Where(it => it.Email == email).Set(it => it.Password, password).UpdateAsync();
        return res > -1;
    }
}
