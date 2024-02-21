using LinqToDB.Mapping;

namespace WebAPI.Database.Models;

[Table("badge_cdk")]
public class BadgeCDK
{
    [PrimaryKey, Identity]
    [Column("id"), NotNull]
    public int ID { get; set; }

    [Column("badge_id")]
    public int BadgeId { get; set; }

    [Column("code")]
    public required string Code { get; set; }

    [Column("repeatable")]
    public bool? Repeatable { get; set; }

    [Column("gen_time")]
    public DateTimeOffset? GenTime { get; set; }

    [Column("redeem_time")]
    public DateTimeOffset? RedeemTime { get; set; }

    [Column("redeem_count")]
    public int RedeemCount { get; set; }

    [Column("redeem_user")]
    public string? RedeemUser { get; set; }

    [Column("expire_at")]
    public DateTimeOffset? ExpireAt { get; set; }

    [Association(ThisKey = "badge_id", OtherKey = "id", CanBeNull = false)]
    public BadgeList? Badge { get; set; }
}
