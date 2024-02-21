using LinqToDB.Mapping;

namespace WebAPI.Database.Models;

[Table("user")]
public class User
{
    [PrimaryKey, Identity]
    [Column("id"), NotNull]
    public int ID { get; set; }

    [Column("username")]
    public string? UserName { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("password")]
    public string? Password { get; set; }

    [Column("last_login_ip")]
    public string? LastLoginIp { get; set; }

    [Column("last_login_time")]
    public DateTimeOffset? LastLoginTime { get; set; }

    [Column("permission_group")]
    public int? PermissionGroupID { get; set; }

    [Association(ThisKey = "permission_group", OtherKey = "id", CanBeNull = true)]
    public IEnumerable<PermissionGroup>? PermissionGroup { get; set; }

    [Association(ThisKey = "id", OtherKey = "user_id", CanBeNull = true)]
    public IEnumerable<UserBadges>? UserBadges { get; set; }

    [Association(ThisKey = "id", OtherKey = "user_id", CanBeNull = true)]
    public IEnumerable<BindQQ>? QQAccount { get; set; }

    [Association(ThisKey = "id", OtherKey = "user_id", CanBeNull = true)]
    public BindDiscord? DiscordAccount { get; set; }

    [Association(ThisKey = "id", OtherKey = "user_id", CanBeNull = true)]
    public BindOSU? OSUAccount { get; set; }
}
