using LinqToDB.Mapping;

namespace desu_life_web_api.Database.Models;

[Table("user_badges")]
public class UserBadges
{
    [PrimaryKey, Identity]
    public int id { get; set; }

    [Column]
    public int user_id { get; set; }

    [Column]
    public int badge_id { get; set; }

    [Column]
    public uint is_displayed { get; set; }

    [Column]
    public DateTimeOffset expire_at { get; set; }

    [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
    public User? user { get; set; }

    [Association(ThisKey = "badge_id", OtherKey = "id", CanBeNull = false)]
    public BadgeList? badge_list { get; set; }
}