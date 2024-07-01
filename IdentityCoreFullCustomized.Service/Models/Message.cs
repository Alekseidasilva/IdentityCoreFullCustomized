using MimeKit;

namespace IdentityCoreFullCustomized.Service.Models;

public class Message
{
    public List<MailboxAddress> To { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }

    public Message(List<MailboxAddress> to, string subject, string content)
    {
        To = new List<MailboxAddress>();
        To.AddRange(to.Select(x => new MailboxAddress("email", x.Address)));
        Subject = subject;
        Content = content;
    }


}