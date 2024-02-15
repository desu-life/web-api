using LanguageExt.ClassInstances;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static desu_life_web_backend.JWT;

namespace desu_life_web_backend;

public static partial class Security
{
    public static SymmetricSecurityKey? key { get; set; }
    public static string? Salt { get; set; }

    public static void KeyChecker()
    {
        if (!File.Exists("secretKey"))
        {
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

        if (!File.Exists("Salt"))
        {
            Salt = Utils.GenerateRandomString(256);
            byte[] SaltBytes = System.Text.Encoding.UTF8.GetBytes(Salt);
            File.WriteAllBytes("Salt", SaltBytes);
            Console.WriteLine("New Salt saved & loaded.");
        }
        else
        {
            Salt = File.ReadAllBytes("Salt").ToString();
            Console.WriteLine("Salt loaded from file.");
        }
    }

    public static string SetLoginToken(long uid, string mailAddr)
    {
        Claim[] claim_set = [new(ClaimTypes.NameIdentifier, $"{uid}"), new(ClaimTypes.Email, mailAddr.ToLower())];
        return CreateJWTToken(claim_set, 60);
    }

    public static bool GetUserInfoFromToken(IRequestCookieCollection cookies, out long UserId, out string mailAddr, out string? Token)
    {
        UserId = 0;
        mailAddr = "";
        Token = null;

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(cookies["token"] ?? "", validationParameters, out SecurityToken validatedToken);
            // foreach (var claim in principal.Claims) Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            var x = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (x != null) UserId = long.Parse(x.Value);
            x = principal.FindFirst(ClaimTypes.Email);
            if (x != null) mailAddr = x.Value;
            x = principal.FindFirst(ClaimTypes.UserData);
            if (x != null) Token = x.Value;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
        }
        return false;
    }

    public static string UpdateVerifyTokenFromToken(IRequestCookieCollection cookies, string VerifyToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(cookies["token"] ?? "", validationParameters, out SecurityToken validatedToken);
            var claim_set = principal.Claims.ToList();
            var existingUserDataClaim = claim_set.FirstOrDefault(c => c.Type == ClaimTypes.UserData);
            if (existingUserDataClaim != null)
                claim_set.Remove(existingUserDataClaim);
            claim_set.Add(new Claim(ClaimTypes.UserData, VerifyToken));
            return CreateJWTToken([.. claim_set], 60);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
        }
        return "";
    }

    public static string GenerateVerifyToken(long timestamp, string email, string op)
    {
        string toBeHashed = $"{timestamp}[%*#]{email}[%*#]{op}[%*#]{Salt}";

        byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(toBeHashed));
        StringBuilder sb = new();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("x2"));
        }
        return $"{timestamp}[%*#]{sb}";
    }
}
