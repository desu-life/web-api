using LinqToDB.Mapping;

namespace desu_life_web_api.Database.Models;

[Table("bind_discord")]
public class BindDiscord
{
    [PrimaryKey, Identity]
    public int id { get; set; }

    [Column]
    public int user_id { get; set; }

    [Column]
    public string? discord_id { get; set; }

    [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
    public User? User { get; set; }
}