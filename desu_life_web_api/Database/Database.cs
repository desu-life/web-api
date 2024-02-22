using LanguageExt.Pretty;
using LinqToDB;
using LinqToDB.Common;
using MySqlConnector;
using static WebAPI.Database.Connection;
using WebAPI.Database.Models;

namespace WebAPI.Database;

public partial class Client
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
}

