using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static desu_life_web_backend.JWT;

namespace desu_life_web_backend;

public static partial class Security
{
    public static SymmetricSecurityKey? key { get; set; }

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
    }

    public static string SetLoginToken(long uid, string mailAddr)
    {
        Claim[] claim_set = [new("userId", $"{uid}"), new("email", mailAddr)];
        return CreateJWTToken(claim_set, 60);
    }

    public static bool GetUserInfoFromToken(IRequestCookieCollection cookies,out long UserId,out string mailAddr,out string? Token)
    {
        UserId = 0;
        mailAddr = "";
        Token = null;

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(cookies["token"] ?? "", validationParameters, out SecurityToken validatedToken);
            var x = principal.FindFirst("userId");
            if (x != null)
                UserId = long.Parse(x.Value);
            x = principal.FindFirst("email");
            if (x != null)
                mailAddr = x.Value;
            x = principal.FindFirst("token");
            if (x != null)
                Token = x.Value;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
        }
        return false;
    }

    public static string GetVerifyTokenFromToken(IRequestCookieCollection cookies)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(cookies["token"] ?? "", validationParameters, out SecurityToken validatedToken);
            var x = principal.FindFirst("verifyToken");
            if (x != null)
            {
                var verifyToken = x.Value;
                return verifyToken;
            }
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
        }
        return "";
    }

    public static string UpdateVerifyTokenFromToken(IRequestCookieCollection cookies, string VerifyToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(cookies["token"] ?? "", validationParameters, out SecurityToken validatedToken);
            var claim_set = principal.Claims;
            _ = claim_set.Append(new Claim("verifyToken", VerifyToken));
            return CreateJWTToken(claim_set.ToArray(), 60);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
        }
        return "";
    }
}
