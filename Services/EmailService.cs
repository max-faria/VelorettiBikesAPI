using MailKit.Net.Smtp;
using MimeKit;

namespace VelorettiAPI.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Veloretti Bikes", _configuration["EmailSettings:FromEmail"]));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("plain") { Text = message };

        using (var client = new SmtpClient())
        {
            var emailPort = _configuration["EmailSettings:Port"];
            if(string.IsNullOrEmpty(emailPort))
            {
                throw new ArgumentNullException("EmailSettings:Port", "The Port cannot be null");
            }
            
            await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], int.Parse(emailPort), true);
            await client.AuthenticateAsync(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }

    }
}
