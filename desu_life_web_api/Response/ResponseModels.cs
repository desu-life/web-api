using LinqToDB.Mapping;

namespace desu_life_web_api.Response;

public class Log
{

}

public class QQVerifyResponse
{
    public string? token { get; set; } 
}

public class UserResponse
{
    public long? uid { get; set; }

    public string? username { get; set; }

    public string? email { get; set; }

    public long? osu_uid { get; set; }

    public long? qq_id { get; set; }

    public string? qq_guild_uid { get; set; }

    public string? kook_uid { get; set; }

    public string? discord_uid { get; set; }

    public string? permissions { get; set; }

    public string? displayed_badge_ids { get; set; }

    public string? owned_badge_ids { get; set; }
}
