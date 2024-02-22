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
    [JsonPropertyName("last_login_ip")]
    public string? LastLoginIP { get; set; }
    [JsonPropertyName("last_login_time")]
    public string? LastLoginTime { get; set; }
    [JsonPropertyName("permission_group")]
    public uint PermissionGroup { get; set; }
    [JsonPropertyName("badges")]
    public List<Database.Models.UserBadges>? Badges { get; set; }
    [JsonPropertyName("bind_qq")]
    public List<Database.Models.BindQQ>? BindQQ { get; set; }
    [JsonPropertyName("bind_discord")]
    public Database.Models.BindDiscord? BindDiscord { get; set; }
    [JsonPropertyName("bind_osu")]
    public Database.Models.BindOSU? BindOsu { get; set; }
}
