using LinqToDB.Mapping;

namespace desu_life_web_api.Database.Models;

[Table("badge_cdk")]
public class BadgeCDK
{
    [PrimaryKey, Identity]
    public int id { get; set; }

    [Column]
    public int badge_id { get; set; }

    [Column]
    public string? code { get; set; }

    [Column]
    public bool repeatable { get; set; }

    [Column]
    public DateTimeOffset gen_time { get; set; }

    [Column]
    public DateTimeOffset redeem_time { get; set; }

    [Column]
    public int redeem_count { get; set; }

    [Column]
    public string? redeem_user { get; set; }

    [Column]
    public DateTimeOffset expire_at { get; set; }

    [Association(ThisKey = "badge_id", OtherKey = "id", CanBeNull = false)]
    public BadgeList? badge_list { get; set; }
}