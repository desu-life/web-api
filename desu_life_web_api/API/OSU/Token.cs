using Flurl.Http;
using Newtonsoft.Json.Linq;
using static Org.BouncyCastle.Math.EC.ECCurve;
using WebAPI.Response;

namespace WebAPI.API.OSU;

public partial class ApiV2
{
    public async Task<bool> GetTokenAsync()
    {
        var requestData = new JObject
        {
            { "grant_type", "client_credentials" },
            { "client_id", Config.Inner!.Osu?.ClientId },
            { "client_secret", Config.Inner!.Osu?.ClientSecret },
            { "scope", "public" },
            { "code", "kanon" },
        };

        JObject responseBody = new();

        try
        {
            var response = await "https://osu.ppy.sh/oauth/token".PostJsonAsync(requestData);
            responseBody = await response.GetJsonAsync<JObject>();

            token = responseBody["access_token"]?.ToString() ?? "";
            tokenExpireTime =
                DateTimeOffset.Now.ToUnixTimeSeconds()
                + (
                    long.TryParse(responseBody["expires_in"]?.ToString(), out var expiresIn)
                        ? expiresIn
                        : 0
                );

            return true;
        }
        catch (Exception ex) // 指定具体的异常类型
        {
            logger.LogError(
                $"获取token失败: {ex.Message}, 返回Body: \n({responseBody?.ToString() ?? "无"})"
            );
            return false;
        }
    }

    public async Task CheckTokenAsync()
    {
        if (tokenExpireTime <= DateTimeOffset.Now.ToUnixTimeSeconds())
        {
            string tokenStatus =
                tokenExpireTime == 0 ? "正在获取OSUApiV2_Token" : "OSUApiV2_Token已过期, 正在重新获取";
            logger.LogInformation(tokenStatus);

            if (await GetTokenAsync())
            {
                // 避免在日志中显示完整的令牌信息
                logger.LogInformation(
                    $"获取成功, Token: {token.Substring(0, Math.Min(token.Length, 3))}..."
                );
                logger.LogInformation(
                    $"Token过期时间: {DateTimeOffset.FromUnixTimeSeconds(tokenExpireTime).DateTime.ToLocalTime()}"
                );
            }
        }
    }
}

