using LinqToDB.Mapping;

namespace desu_life_web_api.Database.Models;

[Table("pp_plus_record")]
public class PP_PlusRecord
{
    [PrimaryKey, Identity]
    public int id { get; set; }

    [Column]
    public long osu_id { get; set; }

    [Column]
    public float pp { get; set; }

    [Column]
    public int jump { get; set; }

    [Column]
    public int flow { get; set; }

    [Column]
    public int pre { get; set; }

    [Column]
    public int acc { get; set; }

    [Column]
    public int spd { get; set; }

    [Column]
    public int sta { get; set; }

    [Association(ThisKey = "osu_id", OtherKey = "osu_uid", CanBeNull = false)]
    public BindOsu? Bind_osu { get; set; }
}