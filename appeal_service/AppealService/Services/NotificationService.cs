﻿using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AppealService.Services;

public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly string SENDER_EMAIL;
    private readonly string SENDER_PASSWORD;
    public NotificationService(ILogger<NotificationService> logger, IOptions<EmailSettings> settings)
    {
        _logger = logger;
        SENDER_EMAIL = settings.Value.Email;
        SENDER_PASSWORD = settings.Value.Password;
    }

    public async Task SendToSeller(string email, CancellationToken token = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Cryptocurrency transfer tech support", SENDER_EMAIL));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Information about account";
        message.Body = new TextPart("html")
        {
            Text = "<b><i>Dear user! We are sorry to inform but unfortunately your account " +
                   "on cryptotransfer.com was frozen due to suspicions in deception during your P2P transactions. Please, " +
                   "contact our tech support to learn more details.</i></b>"
        };
        using SmtpClient client = new SmtpClient();
        try
        {
            await client.ConnectAsync("smtp.gmail.com", 465, true, token);
            await client.AuthenticateAsync(SENDER_EMAIL, SENDER_PASSWORD, token);
            await client.SendAsync(message, token);
            await client.DisconnectAsync(true, token);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        } 
    }
    
    public async Task SendToBuyer(string email, CancellationToken token = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Cryptocurrency transfer tech support", SENDER_EMAIL));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = "Information about appeal";
        message.Body = new TextPart("html")
        {
            Text = "<b><i>Dear user! We are happy to inform you that " +
                   "the wallet of a scammer was frozen, and the money were successfully transferred to your account wallet.\n\n" +
                   "Sincerely, tech support of Cryptocurrency transfer</i></b>"
        };
        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync("smtp.gmail.com", 465, true, token);
            await client.AuthenticateAsync(SENDER_EMAIL, SENDER_PASSWORD, token);
            await client.SendAsync(message, token);
            await client.DisconnectAsync(true, token);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        } 
    }
}