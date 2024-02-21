using Org.BouncyCastle.Asn1.X509;
using System.Net;
using System.Net.Mail;
using static desu_life_web_api.Mail.Sender;

namespace desu_life_web_api.Mail;

public static class Service
{
    public static async Task<bool> SendVerificationMail(string mailAddr, string verifyCode, string platform, string op)
    {
        string read_html = System.IO.File.ReadAllText("./predefine_files/mail_desu_life_mailaddr_verify_template.txt");
        string mail_subject = "[来自desu.life自动发送的邮件]请验证您的邮箱";

        // need to redirect to the front-end page instead of this link
        read_html = read_html.Replace("{{{{mailaddress}}}}", mailAddr).Replace("{{{{veritylink}}}}",
                    $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&op={op}");
        try
        {
            await SendMail(mailAddr, mail_subject, read_html, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.ToString());
            return false;
        }
        return true;
    }
}