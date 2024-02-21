using LinqToDB.Mapping;

namespace WebAPI.Database.Models;

[Table("bind_qq")]
public class BindQQ
{
    [PrimaryKey, Identity]
    [Column("id"), NotNull]
    public int ID { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("qq_id")]
    public required string QQID { get; set; }

    [Association(ThisKey = "user_id", OtherKey = "id", CanBeNull = false)]
    public User? User { get; set; }
}
