using LinqToDB.Mapping;

namespace WebAPI.Database.Models;

[Table("bind_osu")]
public class BindOSU
{
    [PrimaryKey, Identity]
    [Column("id"), NotNull]
    public int ID { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("osu_uid")]
    public long OSUserId { get; set; }

    [Column("player_name")]
    public string? PlayerName { get; set; }

    [Column("osu_mode")]
    public required string OSUMode { get; set; }

    [Column("custom_info_engine_ver")]
    public int CustomInfoEngineVersion { get; set; }

    [Column("info_v2_mode")]
    public int InfoV2Mode { get; set; }

    [Column("info_v2_custom_mode")]
    public string? InfoV2CustomMode { get; set; }

    [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
    public User? User { get; set; }
}
