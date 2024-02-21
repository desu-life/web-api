using LinqToDB.Mapping;

namespace desu_life_web_api.Database.Models;

[Table("user")]
public class User
{
    [PrimaryKey, Identity]
    public int id { get; set; }

    [Column]
    public string? username { get; set; }

    [Column]
    public string? email { get; set; }

    [Column]
    public string? password { get; set; }

    [Column]
    public string? last_login_ip { get; set; }

    [Column]
    public DateTimeOffset? last_login_time { get; set; }

    [Column]
    public int? permission_group { get; set; }

    [Association(ThisKey = "permission_group", OtherKey = "id", CanBeNull = false)]
    public IEnumerable<PermissionGroup>? Permission_group { get; set; }

    [Association(ThisKey = "id", OtherKey = "user_id", CanBeNull = false)]
    public IEnumerable<UserBadges>? UserBadges { get; set; }

    [Association(ThisKey = "id", OtherKey = "user_id", CanBeNull = false)]
    public IEnumerable<BindDiscord>? Bind_discord { get; set; }

    [Association(ThisKey = "id", OtherKey = "user_id", CanBeNull = false)]
    public IEnumerable<BindOsu>? Bind_osu { get; set; }

    [Association(ThisKey = "id", OtherKey = "user_id", CanBeNull = false)]
    public IEnumerable<BindQQ>? Bind_qq { get; set; }
}