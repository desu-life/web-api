using Org.BouncyCastle.Asn1.X509;
using System.Net;
using System.Net.Mail;

namespace desu_life_web_backend
{
    public static partial class Mail
    {
        static Mail()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private static readonly Config.Base config = Config.inner!;

        public class MailContent
        {
            public List<string> Recipients { get; } // 收件人列表
            public List<string> CC { get; } // 抄送列表
            public string Subject { get; }
            public string Body { get; }
            public bool IsBodyHtml { get; }

            public MailContent(List<string> recipients, string subject, string body, bool isBodyHtml, List<string>? cc = null)
            {
                if (recipients == null || recipients.Count == 0)
                    throw new ArgumentException("Recipients cannot be empty.", nameof(recipients));

                Recipients = recipients;
                Subject = subject ?? throw new ArgumentNullException(nameof(subject));
                Body = body ?? throw new ArgumentNullException(nameof(body));
                IsBodyHtml = isBodyHtml;
                CC = cc ?? new List<string>();
            }
        }

        public static void Send(MailContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using var message = new MailMessage
            {
                From = new MailAddress(config.mail!.userName!), // 设置发件人
                Subject = content.Subject,
                Body = content.Body,
                IsBodyHtml = content.IsBodyHtml
            };

            content.Recipients.ForEach(recipient => message.To.Add(recipient));
            content.CC.ForEach(cc => message.CC.Add(cc));

            using var client = new SmtpClient(config.mail.smtpHost, config.mail.smtpPort)
            {
                Credentials = new NetworkCredential(config.mail.userName, config.mail.passWord),
                EnableSsl = true
            };

            client.Send(message);
        }


        public static async Task<bool> SendVerificationMail(string mailAddr, string verifyCode, string platform, string op)
        {
            return await Task.Run(bool () =>
            {
                string read_html = System.IO.File.ReadAllText("./predefine_files/mail_desu_life_mailaddr_verify_template.txt");
                string mail_subject = "[来自desu.life自动发送的邮件]请验证您的邮箱";

                // need to redirect to the front-end page instead of this link
                read_html = read_html.Replace("{{{{mailaddress}}}}", mailAddr).Replace("{{{{veritylink}}}}",
                            $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&op={op}");
                try
                {
                    Utils.SendMail(mailAddr, mail_subject, read_html, true);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.ToString());
                    return false;
                }
            });
        }
    }
}
