using LinqToDB.Mapping;

namespace WebAPI.Database.Models;

[Table("bind_discord")]
public class BindDiscord
{
    [PrimaryKey, Identity]
    [Column("id"), NotNull]
    public int ID { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("discord_id")]
    public required string DiscordId { get; set; }

    [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
    public User? User { get; set; }
}
