using LinqToDB;
using LinqToDB.Mapping;

using static desu_life_web_api.Database.Models;

namespace desu_life_web_api.Database;


public class Connection(DataOptions options) : LinqToDB.Data.DataConnection(options)
{
    public ITable<User> Users => this.GetTable<User>();
    public ITable<UserOSU> UsersOSU => this.GetTable<UserOSU>();
    public ITable<UserVerify> UserVerify => this.GetTable<UserVerify>();
    public ITable<UserQQGuild> UserQQGuild => this.GetTable<UserQQGuild>();
}

