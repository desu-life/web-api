using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using static desu_life_web_backend.Security;

namespace desu_life_web_backend;

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
            IssuerSigningKey = key,
            ValidIssuer = "desu.life",
            ValidAudience = "desu.life"
        };
        KeyChecker();
        creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public static bool CheckJWTTokenIsVaild(IRequestCookieCollection cookies)
    {
        if (ValidateJWTToken(cookies["token"] ?? ""))
            return true;
        return false;
    }

    public static string CreateJWTToken(Claim[] claim_set, DateTime? expires)
    {
        if (!expires.HasValue) expires = DateTime.Now.AddMinutes(10);
        var token = new JwtSecurityToken(
            issuer: "desu.life",
            audience: "desu.life",
            claims: claim_set,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static bool ValidateJWTToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
