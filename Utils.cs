using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;

namespace desu_life_web_backend;

public static partial class Utils
{
    public static Config.Base config = Config.inner!;

    public static string GetDesc(object? value)
    {
        FieldInfo? fieldInfo = value!.GetType().GetField(value.ToString()!);
        if (fieldInfo == null)
            return string.Empty;
        DescriptionAttribute[] attributes = (DescriptionAttribute[])
            fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }

    public static string HideMailAddr(string mailAddr)
    {
        try
        {
            var t1 = mailAddr.Split('@');
            string[] t2 = new string[t1[0].Length];
            for (int i = 0; i < t1[0].Length; i++)
            {
                t2[i] = "*";
            }
            t2[0] = t1[0][0].ToString();
            t2[t1[0].Length - 1] = t1[0][^1].ToString();
            string ret = "";
            foreach (string s in t2)
            {
                ret += s;
            }
            ret += "@";
            t2 = new string[t1[1].Length];
            for (int i = 0; i < t1[1].Length; i++)
            {
                t2[i] = "*";
            }
            t2[0] = t1[1][0].ToString();
            t2[t1[1].Length - 1] = t1[1][^1].ToString();
            t2[t1[1].IndexOf(".")] = ".";
            foreach (string s in t2)
            {
                ret += s;
            }
            return ret;
        }
        catch
        {
            return mailAddr;
        }
    }

    public static void SendMail(string mailto, string subject, string body, bool isBodyHtml)
    {
        var mailContent = new Mail.MailContent(new List<string> { mailto }, subject, body, isBodyHtml);
        try
        {
            Mail.Send(mailContent);
        }
        catch { }
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
}
