using LinqToDB.Mapping;

namespace desu_life_web_api.Database.Models;

[Table("permission_group")]
public class PermissionGroup 
{
    [PrimaryKey, Identity]
    public int id { get; set; }

    [Column]
    public string? name { get; set; }

    [Column]
    public uint admin { get; set; }

    [Association(ThisKey = "id", OtherKey = "permission_group", CanBeNull = false)]
    public IEnumerable<User>? user { get; set; }
}
