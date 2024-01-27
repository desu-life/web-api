﻿using LanguageExt.Pretty;
using LinqToDB;
using LinqToDB.Common;
using MySqlConnector;
using static desu_life_web_backend.Config;
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

        public static async Task<User?> GetUser(long userId)
        {
            using var db = GetInstance();
            return await db.Users.Where(it => it.uid == userId).FirstOrDefaultAsync();
        }

        public static async Task<bool> AddVerifyToken(string mailAddr, string op, string platform, DateTimeOffset time, string token)
        {
            using var db = GetInstance();

            // token唯一性
            try
            {
                await db.UserVerify
                .Where(it => it.email == mailAddr)
                .DeleteAsync();
            }
            catch
            {
                return false;
            }

            var newverify = new UserVerify()
            {
                email = mailAddr,
                op = op,
                platform = platform,
                token = token,
                gen_time = time
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

        public static async Task<long> GetOsuUID(long uid)
        {
            using var db = GetInstance();
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
            using var db = GetInstance();
            var li = db.Users.Where(it => it.email == email).Select(it => it.uid);
            if (await li.CountAsync() > 0)
                return true;
            return false;
        }

        public static async Task<long> CheckUserIsValidity(string email, string password)
        {
            using var db = GetInstance();
            var li = db.Users.Where(it => it.email == email && it.passwd == password).Select(it => it.uid);
            if (await li.CountAsync() > 0)
            {
                return await li.FirstOrDefaultAsync();
            }
            return -1;
        }

        public static async Task<long> CheckUserIsExsit(string email)
        {
            using var db = GetInstance();
            var li = db.Users.Where(it => it.email == email).Select(it => it.uid);
            if (await li.CountAsync() > 0)
            {
                return await li.FirstOrDefaultAsync();
            }
            return -1;
        }

        public static async Task<bool> CheckUserTokenValidity(string email, string token, string op, string platform)
        {
            using var db = GetInstance();
            var li = db.UserVerify.Where(it => it.email == email)
                                  .Where(it => it.token == token)
                                  .Where(it => it.op == op)
                                  .Where(it => it.platform == platform)
                                  .Select(it => it.email);
            if (await li.CountAsync() > 0)
            {
                try
                {
                    await db.UserVerify
                    .Where(it => it.email == email)
                    .DeleteAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
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

        public static async Task<string?> GetEmailAddressByVerifyToken(string token, string op, string platform)
        {
            using var db = GetInstance();
            var uv = await db.UserVerify
                .Where(it => it.token == token)
                .Where(it => it.op == op)
                .Where (it => it.platform == platform)
                .FirstOrDefaultAsync();
            if (uv != null)
            {
                await db.UserVerify.Where(it => it.email == uv.email).DeleteAsync();
                if (uv.gen_time < DateTimeOffset.Now)
                    return null;
                return uv.email;
            }
            return null;
        }

        public static async Task<bool> InsertUser(string email, string password)
        {
            using var db = GetInstance();
            var d = new User()
            {
                email = email,
                passwd = password,
                permissions = "user",
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
            using var db = GetInstance();
            var res = await db.Users
                .Where(it => it.uid == uid)
                .Set(it => it.passwd, password)
                .UpdateAsync();
            return res > -1;
        }

        public static async Task<bool> UpdatePassword(string mailAddr, string password)
        {
            using var db = GetInstance();
            var res = await db.Users
                .Where(it => it.email == mailAddr)
                .Set(it => it.passwd, password)
                .UpdateAsync();
            return res > -1;
        }
    }
}

