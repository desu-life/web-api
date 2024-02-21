using LinqToDB.Mapping;

namespace WebAPI.Database.Models;

[Table("user_badges")]
public class UserBadges
{
    [PrimaryKey, Identity]
    [Column("id"), NotNull]
    public int ID { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("badge_id")]
    public int BadgeId { get; set; }

    [Column("is_displayed")]
    public bool IsDisplayed { get; set; }

    [Column("expire_at")]
    public DateTimeOffset? ExpireAt { get; set; }

    [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
    public User? User { get; set; }

    [Association(ThisKey = "badge_id", OtherKey = "id", CanBeNull = false)]
    public BadgeList? Badge { get; set; }
}
