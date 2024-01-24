using LinqToDB;
using LinqToDB.Mapping;

using static desu_life_web_backend.Database.Models;

namespace desu_life_web_backend.Database;


public class Connection : LinqToDB.Data.DataConnection
{
    public Connection(DataOptions options)
        : base(options) { }

    public ITable<User> Users => this.GetTable<User>();
    public ITable<UserOSU> UsersOSU => this.GetTable<UserOSU>();
    public ITable<UserVerify> UserVerify => this.GetTable<UserVerify>();

}

