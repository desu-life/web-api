using System.ComponentModel;
using System.Reflection;

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
}
