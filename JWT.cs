using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace desu_life_web_backend;

public static partial class JWT
{
    private static SigningCredentials creds { get; set; }
    private static SymmetricSecurityKey key { get; set; }
    private static TokenValidationParameters validationParameters { get; set; }

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
        if (!File.Exists("secretKey"))
        {
            //key = new SymmetricSecurityKey(Guid.NewGuid().ToByteArray());
            key = new SymmetricSecurityKey(Utils.GenerateRandomKey(256));
            byte[] keyBytes = key.Key;
            File.WriteAllBytes("secretKey", keyBytes);
            Console.WriteLine("New secret key saved & loaded.");
        }
        else
        {
            byte[] keyBytes = File.ReadAllBytes("secretKey");
            key = new SymmetricSecurityKey(keyBytes);
            Console.WriteLine("Key loaded from file.");
        }
        creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public static bool CheckJWTTokenIsVaild(IRequestCookieCollection cookies)
    {
        if (ValidateJWTToken(cookies["token"] ?? ""))
            return true;
        return false;
    }

    public static string CreateJWTToken(long uid, DateTime? expires)
    {
        if (!expires.HasValue) expires = DateTime.Now.AddMinutes(10);

        Claim[] claim_set = [new("userId", $"{uid}")];

        var token = new JwtSecurityToken(
            issuer: "desu.life",
            audience: "desu.life",
            claims: claim_set,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string CreateJWTToken(string verify_token, DateTime? expires)
    {
        if (!expires.HasValue) expires = DateTime.Now.AddMinutes(10);

        Claim[] claim_set = [new("verify_token", verify_token)];

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

    public static long GetUserID(IRequestCookieCollection cookies)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(cookies["token"] ?? "", validationParameters, out SecurityToken validatedToken);
            var userIdClaim = principal.FindFirst("userId");
            if (userIdClaim != null)
            {
                var userId = long.Parse(userIdClaim.Value);
                Console.WriteLine($"User ID: {userId}");
                return userId;
            }
            else
            {
                Console.WriteLine("User ID claim not found in the token.");
            }
            return -1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
        }
        return -1;
    }

    // not finished yet
    public static long GetVerifyToken(IRequestCookieCollection cookies)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(cookies["token"] ?? "", validationParameters, out SecurityToken validatedToken);
            var userIdClaim = principal.FindFirst("verify_token");
            if (userIdClaim != null)
            {
                var userId = long.Parse(userIdClaim.Value);
                Console.WriteLine($"User ID: {userId}");
                return userId;
            }
            else
            {
                Console.WriteLine("User ID claim not found in the token.");
            }
            return -1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
        }
        return -1;
    }
}
