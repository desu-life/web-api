using LinqToDB.Mapping;

namespace desu_life_web_api.Database.Models;

[Table("bind_osu")]
public class BindOsu
{
    [PrimaryKey, Identity]
    public int id { get; set; }

    [Column]
    public int user_id { get; set; }

    [Column]
    public long osu_uid { get; set; }

    [Column]
    public string? player_name { get; set; }

    [Column]
    public string? osu_mode { get; set; }

    [Column]
    public int customInfoEngineVer { get; set; }

    [Column]
    public int InfoPanelV2_Mode { get; set; }

    [Column]
    public string? InfoPanelV2_CustomMode { get; set; }

    [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
    public User? User { get; set; }

    [Association(ThisKey = "osu_uid", OtherKey = "osu_uid", CanBeNull = false)]
    public IEnumerable<PermissionGroup>? Permission_group { get; set; }
}