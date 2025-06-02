using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Natiga.Services;

public class EmailService(ILogger<EmailService> logger, IConfiguration configuration)
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Read SMTP settings from configuration
        var smtpSection = configuration.GetSection("Smtp");
        var host = smtpSection["Host"];
        var port = smtpSection.GetValue<int>("Port");
        var username = smtpSection["Username"];
        var password = smtpSection["Password"];
        var from = smtpSection["From"];
        var enableSsl = smtpSection.GetValue<bool>("EnableSsl", true);

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(from))
        {
            logger.LogError("SMTP configuration is missing or incomplete.");
            throw new InvalidOperationException("SMTP configuration is missing or incomplete.");
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder()
        {
            HtmlBody = body,
            
        };

        message.Body = bodyBuilder.ToMessageBody();
        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
            logger.LogInformation("Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}
