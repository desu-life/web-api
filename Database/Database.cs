using LinqToDB;
using LinqToDB.Common;
using MySqlConnector;
using static desu_life_web_backend.Database.Connection;
using static desu_life_web_backend.Database.Models;

namespace desu_life_web_backend.Database
{
    public class Client
    {
        private static Config.Base config = Config.inner!;

        private static Connection GetInstance()
        {
            var options = new DataOptions().UseMySqlConnector(
                new MySqlConnectionStringBuilder
                {
                    Server = config.database!.host,
                    Port = (uint)config.database.port,
                    UserID = config.database.user,
                    Password = config.database.password,
                    Database = config.database.db,
                    CharacterSet = "utf8mb4",
                    CancellationTimeout = 5,
                }.ConnectionString
            );
            // 暂时只有Mysql
            return new Connection(options);
        }

        public static async Task<User?> GetUser(string mailAddr)
        {
            using var db = GetInstance();
            return await db.Users.Where(it => it.email == mailAddr).FirstOrDefaultAsync();
        }



        public static async Task<bool> AddVerifyToken(long uid, string op, string platform, DateTimeOffset time, string token)
        {
            using var db = GetInstance();
            var newverify = new UserVerify()
            {
                uid = uid,
                op = op,
                platform = platform,
                time = time,
                token = token
            };

            try
            {
                await db.InsertAsync(newverify);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> OSUCheckUserHasLinkedByOthers(string osu_uid)
        {
            return await OSUCheckUserHasLinkedByOthers(long.Parse(osu_uid));
        }

        public static async Task<bool> OSUCheckUserHasLinkedByOthers(long osu_uid)
        {
            using var db = GetInstance();
            var li = db.UsersOSU.Where(it => it.osu_uid == osu_uid).Select(it => it.uid);
            if (await li.CountAsync() > 0)
                return true;
            return false;
        }

        public static async Task<bool> DiscordCheckUserHasLinkedByOthers(string discord_uid)
        {
            using var db = GetInstance();
            var li = db.Users.Where(it => it.discord_uid == discord_uid).Select(it => it.uid);
            if (await li.CountAsync() > 0)
                return true;
            return false;
        }

        public static async Task<bool> CheckCurrentUserHasLinkedDiscord(long uid)
        {
            using var db = GetInstance();
            var li = await db.Users.Where(it => it.uid == uid).Select(it => it.discord_uid).FirstOrDefaultAsync();
            if (li != null)
                return true;
            return false;
        }

        public static async Task<bool> CheckCurrentUserHasLinkedOSU(long uid)
        {
            using var db = GetInstance();
            var li = db.UsersOSU.Where(it => it.uid == uid).Select(it => it.uid);
            if (await li.CountAsync() > 0)
                return true;
            return false;
        }

        public static async Task<bool> CheckUserAccessbility(long uid, string token, string op, string platform)
        {
            using var db = GetInstance();
            var li = db.UserVerify.Where(it => it.uid == uid)
                                  .Where(it => it.token == token)
                                  .Where(it => it.op == op)
                                  .Where(it => it.platform == platform)
                                  .Select(it => it.uid);
            if (await li.CountAsync() > 0)
                return true;
            return false;
        }

        public static async Task<bool> LinkDiscordAccount(long uid, string discord_uid)
        {
            using var db = GetInstance();
            var res = await db.Users
                .Where(it => it.uid == uid)
                .Set(it => it.discord_uid, discord_uid)
                .UpdateAsync();

            return res > -1;
        }

        public static async Task<bool> InsertOsuUser(long uid, long osu_uid)
        {
            using var db = GetInstance();
            var d = new UserOSU()
            {
                uid = uid,
                osu_uid = osu_uid,
                osu_mode = "osu",
                customInfoEngineVer = 2,
                InfoPanelV2_Mode = 1
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
    }
}

