using System.Net.Mail;
using System.Net;

namespace WebAPI.Mail;

public static class Sender
{
    private static readonly Config.Base config = Config.Inner!;
    static Sender()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    }

    private static async Task mailSender(MailContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        using var message = new MailMessage
        {
            From = new MailAddress(config.Mail!.UserName!), // 设置发件人
            Subject = content.Subject,
            Body = content.Body,
            IsBodyHtml = content.IsBodyHtml
        };

        content.Recipients.ForEach(recipient => message.To.Add(recipient));
        content.CC.ForEach(cc => message.CC.Add(cc));

        using var client = new SmtpClient(config.Mail!.SmtpHost, config.Mail!.SmtpPort)
        {
            Credentials = new NetworkCredential(config.Mail!.UserName, config.Mail!.PassWord),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }

    public static async Task SendMail(string mailto, string subject, string body, bool isBodyHtml)
    {
        var mailContent = new MailContent([mailto], subject, body, isBodyHtml);
        try
        {
            await mailSender(mailContent);
        }
        catch
        {
            // 不知道写什么
        }
    }
}
