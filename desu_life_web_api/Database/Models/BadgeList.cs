using LinqToDB.Mapping;

namespace desu_life_web_api.Database.Models;

[Table("badge_list")]
public class BadgeList
{
    [PrimaryKey, Identity]
    public int id { get; set; }

    [Column]
    public string? name { get; set; }

    [Column]
    public string? name_chinese { get; set; }

    [Column]
    public string? description { get; set; }

    [Column]
    public DateTimeOffset expire_at { get; set; }

    [Association(ThisKey = "id", OtherKey = "badge_id", CanBeNull = false)]
    public IEnumerable<BadgeCDK>? badge_cdk { get; set; }

    [Association(ThisKey = "id", OtherKey = "badge_id", CanBeNull = false)]
    public IEnumerable<UserBadges>? user_badges { get; set; }
}