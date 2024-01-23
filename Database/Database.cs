using LinqToDB;
using LinqToDB.Common;
using MySqlConnector;
using static desu_life_web_backend.Database.Models;

namespace desu_life_web_backend.Database
{
    public class Client
    {
        private static Config.Base config = Config.inner!;

        private static DB GetInstance()
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
            return new DB(options);
        }

        public static async Task<User?> GetUser(string mailAddr)
        {
            using var db = GetInstance();
            return await db.User.Where(it => it.email == mailAddr).FirstOrDefaultAsync();
        }

        public static async Task<bool> AddVerifyToken(string mailAddr, string verify)
        {
            using var db = GetInstance();
            var newverify = new UserVerify()
            {
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


    }
}

