using LanguageExt.ClassInstances;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace WebAPI.Security;

public static class Checker
{
    public static SymmetricSecurityKey? Key { get; set; }
    public static string? Salt { get; set; }

    public static void Check()
    {
        if (!File.Exists("secretKey"))
        {
            Key = new SymmetricSecurityKey(Utils.GenerateRandomKey(256));
            byte[] keyBytes = Key.Key;
            File.WriteAllBytes("secretKey", keyBytes);
            Console.WriteLine("New secret key saved & loaded.");
        }
        else
        {
            byte[] keyBytes = File.ReadAllBytes("secretKey");
            Key = new SymmetricSecurityKey(keyBytes);
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
}
