namespace desu_life_web_api.Mail;

public static class MailUtils
{
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