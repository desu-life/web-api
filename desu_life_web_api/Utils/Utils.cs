using WebAPI.Mail;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using LinqToDB.Tools;
using WebAPI;

namespace WebAPI;

public static partial class Utils
{
    private static Config.Base config = Config.Inner!;

    public static string GetDesc(object? value)
    {
        FieldInfo? fieldInfo = value!.GetType().GetField(value.ToString()!);
        if (fieldInfo == null)
            return string.Empty;
        DescriptionAttribute[] attributes = (DescriptionAttribute[])
            fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }

    public static byte[] GenerateRandomKey(int bits)
    {
        if (bits % 8 != 0)
        {
            throw new ArgumentException("Key size must be a multiple of 8.");
        }

        byte[] keyBytes = new byte[bits / 8];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }

        return keyBytes;
    }

    public static string GenerateRandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
    {
        if (length <= 0)
        {
            throw new ArgumentException("Length must be greater than zero.");
        }

        byte[] randomBytes = new byte[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            int index = randomBytes[i] % allowedChars.Length;
            chars[i] = allowedChars[index];
        }

        return new string(chars);
    }

    public static string GetCurrentTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
