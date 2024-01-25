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

    public static string SetLoginToken(long uid)
    {
        Claim[] claim_set = [new("userId", $"{uid}")];
        return CreateJWTToken(claim_set, null);
    }

    public static long GetUserIDFromToken(IRequestCookieCollection cookies)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(cookies["token"] ?? "", validationParameters, out SecurityToken validatedToken);
            var x = principal.FindFirst("userId");
            if (x != null)
            {
                var userId = long.Parse(x.Value);
                return userId;
            }
            //else { Console.WriteLine("User ID claim not found in the token."); }
            return -1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
        }
        return -1;
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
            return CreateJWTToken(claim_set.ToArray(), null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
        }
        return "";
    }
}
