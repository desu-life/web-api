using System.Text.Json.Serialization;
using LinqToDB.Mapping;

namespace WebAPI.Response;

public class Log
{

}

public class QQVerifyResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}

public class UserResponse
{
    [JsonPropertyName("uid")]
    public long? UserId { get; set; }
    [JsonPropertyName("username")]
    public string? UserName { get; set; }
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("osu_id")]
    public long? OSUserId { get; set; }
    [JsonPropertyName("qq_id")]
    public string? QQID { get; set; }
    [JsonPropertyName("qq_guild_uid")]
    public string? QQGuildUid { get; set; }
    [JsonPropertyName("kook_uid")]
    public string? KOOKUid { get; set; }
    [JsonPropertyName("discord_uid")]
    public string? DiscordUid { get; set; }
    [JsonPropertyName("permissions")]
    public string? Permissions { get; set; }
    [JsonPropertyName("displayed_badge_ids")]
    public string? DisplayedBadgeIds { get; set; }
    [JsonPropertyName("owned_badge_ids")]
    public string? OwnedBadgeIds { get; set; }
}
