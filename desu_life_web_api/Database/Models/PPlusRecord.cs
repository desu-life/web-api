using LinqToDB.Mapping;

namespace WebAPI.Database.Models;

[Table("pp_plus_record")]
public class PPlusRecord
{
    [PrimaryKey, Identity]
    [Column("id"), NotNull]
    public int ID { get; set; }

    [Column("osu_id")]
    public long OSUserId { get; set; }

    [Column("pp")]
    public float PP { get; set; }

    [Column("jump")]
    public int Jump { get; set; }

    [Column("flow")]
    public int Flow { get; set; }

    [Column("pre")]
    public int Pre { get; set; }

    [Column("acc")]
    public int Acc { get; set; }

    [Column("spd")]
    public int Spd { get; set; }

    [Column("sta")]
    public int Sta { get; set; }

    [Association(ThisKey = "osu_id", OtherKey = "osu_uid", CanBeNull = false)]
    public BindOSU? OSUAccount { get; set; }
}
