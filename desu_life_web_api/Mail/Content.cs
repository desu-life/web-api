namespace desu_life_web_api.Mail;

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
        CC = cc ?? [];
    }
}
