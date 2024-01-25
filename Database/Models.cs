using LinqToDB;
using LinqToDB.Mapping;

namespace desu_life_web_backend.Database
{
    public static class Models
    {

        [Table("users")]
        public class User
        {
            [PrimaryKey, Identity]
            public long uid { get; set; }

            [PrimaryKey]
            public string? email { get; set; }

            [Column]
            public string? passwd { get; set; }

            [Column]
            public long qq_id { get; set; }

            [Column]
            public string? qq_guild_uid { get; set; }

            [Column]
            public string? kook_uid { get; set; }

            [Column]
            public string? discord_uid { get; set; }

            [Column]
            public string? permissions { get; set; }

            [Column]
            public string? last_login_ip { get; set; }

            [Column]
            public string? last_login_time { get; set; }

            [Column]
            public int status { get; set; }

            [Column]
            public string? displayed_badge_ids { get; set; }

            [Column]
            public string? owned_badge_ids { get; set; }
        }

        [Table("users_osu")]
        public class UserOSU
        {
            [PrimaryKey]
            public long uid { get; set; }

            [PrimaryKey]
            public long osu_uid { get; set; }

            [Column]
            public string? osu_mode { get; set; }

            [Column]
            public int customInfoEngineVer { get; set; } // 1=v1 2=v2

            [Column]
            public string? InfoPanelV2_CustomMode { get; set; }

            [Column]
            public int InfoPanelV2_Mode { get; set; }
        }

        [Table("user_verify")]
        public class UserVerify
        {
            [Column]
            public string? email { get; set; }

            [PrimaryKey]
            public string? token { get; set; }

            [Column]
            public string? op { get; set; }

            [Column]
            public string? platform { get; set; }

            [Column]
            public DateTimeOffset gen_time { get; set; }
        }
    }
}
