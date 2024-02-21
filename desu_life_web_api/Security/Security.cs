using LanguageExt.ClassInstances;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace desu_life_web_api.Security;

public static class Key
{
    public static SymmetricSecurityKey? key { get; set; }
    public static string? Salt { get; set; }

    public static void Checker()
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
}
