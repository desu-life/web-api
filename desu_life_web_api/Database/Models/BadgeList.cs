using LinqToDB.Mapping;

namespace WebAPI.Database.Models;

[Table("badge_list")]
public class BadgeList
{
    [PrimaryKey, Identity]
    [Column("id"), NotNull]
    public int ID { get; set; }

    [Column("name")]
    public required string Name { get; set; }

    [Column("name_chinese")]
    public required string NameChinese { get; set; }

    [Column("description")]
    public required string Description { get; set; }

    [Column("expire_at")]
    public DateTimeOffset? ExpireAt { get; set; }

    [Association(ThisKey = "id", OtherKey = "badge_id", CanBeNull = false)]
    public IEnumerable<BadgeCDK>? BadgeCdk { get; set; }

    [Association(ThisKey = "id", OtherKey = "badge_id", CanBeNull = false)]
    public IEnumerable<UserBadges>? UserBadges { get; set; }
}
