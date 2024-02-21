using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace WebAPI.Security;

public static partial class JWT
{
    public static SigningCredentials Creds { get; set; }
    public static TokenValidationParameters ValidationParameters { get; set; }

    static JWT()
    {
        ValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = Checker.Key,
            ValidIssuer = "desu.life",
            ValidAudience = "desu.life"
        };
        Creds = new SigningCredentials(Checker.Key, SecurityAlgorithms.HmacSha256);
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
            signingCredentials: Creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static bool ValidateJWTToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, ValidationParameters, out var validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
