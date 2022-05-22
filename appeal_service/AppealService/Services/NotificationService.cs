using MailKit.Net.Smtp;
using MimeKit;

namespace AppealService.Services;

public class NotificationService
{
    private const string senderEmail = "crypto.transfer.tech@gmail.com";
    private const string senderPassword = "Casper2001!";
    public async Task SendToSeller(string email, CancellationToken token = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Cryptocurrency transfer tech support", senderEmail));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Information about account";
        message.Body = new TextPart("html")
        {
            Text = "<b><i>Dear user! We are sorry to inform but unfortunately your account " +
                   "on cryptotransfer.com was frozen due to suspicions in deception during your P2P transactions. Please, " +
                   "contact our tech support to learn more details.</i></b>"
        };
        using SmtpClient client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, true, token);
        await client.AuthenticateAsync(senderEmail, senderPassword, token);
        await client.SendAsync(message, token);
        await client.DisconnectAsync(true, token);
    }
    
    public async Task SendToBuyer(string email, CancellationToken token = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Cryptocurrency transfer tech support", senderEmail));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Information about appeal";
        message.Body = new TextPart("html")
        {
            Text = "<b><i>Dear user! We are happy to inform you that " +
                   "the wallet of a scammer was frozen, and the money were successfully transferred to your account wallet.\n\n" +
                   "Sincerely, tech support of Cryptocurrency transfer</i></b>"
        };
        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 465, true, token);
        await client.AuthenticateAsync(senderEmail, senderPassword, token);
        await client.SendAsync(message, token);
        await client.DisconnectAsync(true, token);
    }
}