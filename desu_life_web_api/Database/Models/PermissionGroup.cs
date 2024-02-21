using LinqToDB.Mapping;

namespace WebAPI.Database.Models;

[Table("permission_group")]
public class PermissionGroup
{
    [PrimaryKey, Identity]
    [Column("id"), NotNull]
    public int ID { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("admin")]
    public bool IsAdmin { get; set; }

    [Association(ThisKey = "id", OtherKey = "permission_group", CanBeNull = true)]
    public IEnumerable<User>? User { get; set; }
}
