using LinqToDB;
using WebAPI.Database.Models;

namespace WebAPI.Database;

public class Connection(DataOptions options) : LinqToDB.Data.DataConnection(options)
{
    public ITable<User> Users => this.GetTable<User>();
    public ITable<BindQQ> BindQQ => this.GetTable<BindQQ>();
    public ITable<BindOSU> BindOSU => this.GetTable<BindOSU>();
    public ITable<BadgeCDK> BadgeCDK => this.GetTable<BadgeCDK>();
    public ITable<BadgeList> BadgeList => this.GetTable<BadgeList>();
    public ITable<UserBadges> UsersBadges => this.GetTable<UserBadges>();
    public ITable<BindDiscord> BindDiscord => this.GetTable<BindDiscord>();
    public ITable<PPlusRecord> PPlusRecord => this.GetTable<PPlusRecord>();
    public ITable<PermissionGroup> PermissionGroup => this.GetTable<PermissionGroup>();
}
