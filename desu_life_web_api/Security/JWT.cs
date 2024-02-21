using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace desu_life_web_api.Security;

public static partial class JWT
{
    public static SigningCredentials creds { get; set; }
    public static TokenValidationParameters validationParameters { get; set; }

    static JWT()
    {
        validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = Key.key,
            ValidIssuer = "desu.life",
            ValidAudience = "desu.life"
        };
        creds = new SigningCredentials(Key.key, SecurityAlgorithms.HmacSha256);
    }

    public static bool CheckJWTTokenIsVaild(IRequestCookieCollection cookies)
    {
        var x = cookies["token"];
        if (string.IsNullOrEmpty(x))
            return false;
        if (ValidateJWTToken(x))
            return true;
        return false;
    }

    public static string CreateJWTToken(Claim[] claim_set, long expires_min)
    {
        var token = new JwtSecurityToken(
            issuer: "desu.life",
            audience: "desu.life",
            claims: claim_set,
            expires: DateTime.Now.AddMinutes(expires_min),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static bool ValidateJWTToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
