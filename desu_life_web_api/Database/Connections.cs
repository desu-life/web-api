using LinqToDB;
using desu_life_web_api.Database.Models;

namespace desu_life_web_api.Database;

public class Connection(DataOptions options) : LinqToDB.Data.DataConnection(options)
{ 
    public ITable<User> Users => this.GetTable<User>();
    public ITable<BindQQ> Bind_QQ => this.GetTable<BindQQ>();
    public ITable<BindOsu> Bind_Osu => this.GetTable<BindOsu>();
    public ITable<BadgeCDK> Badge_CDK => this.GetTable<BadgeCDK>();
    public ITable<BadgeList> Badge_List => this.GetTable<BadgeList>();
    public ITable<UserBadges> Users_Badges => this.GetTable<UserBadges>();
    public ITable<BindDiscord> Bind_Discord => this.GetTable<BindDiscord>();
    public ITable<PP_PlusRecord> PP_Plus_Record => this.GetTable<PP_PlusRecord>();
    public ITable<PermissionGroup> Permission_Group => this.GetTable<PermissionGroup>();
}