using System;
using System.Net.Mail;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace PictureApp.API.Providers
{
    public class MailKitEmailClientProvider : IEmailClientProvider
    {
        private readonly IConfiguration _configuration;

        public MailKitEmailClientProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task SendAsync(MailAddress to, string subject, string body)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));

            // Prepare a message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration.GetSection("AppSettings:MailServiceMailBoxName").Value,
                _configuration.GetSection("AppSettings:MailServiceMailBoxAddress").Value));
            message.To.Add(new MailboxAddress(to.DisplayName, to.Address));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = body
            };

            // Configure a SMTP client and send a message
            var client = new SmtpClient {ServerCertificateValidationCallback = (s, c, h, e) => true};
            client.Connect(_configuration.GetSection("AppSettings:MailServiceHost").Value,
                int.Parse(_configuration.GetSection("AppSettings:MailServicePort").Value), SecureSocketOptions.SslOnConnect);
            client.Authenticate(_configuration.GetSection("AppSettings:MailServiceAuthenticationUserName").Value,
                _configuration.GetSection("AppSettings:MailServiceAuthenticationUserPassword").Value);
            return client.SendAsync(message);
        }
    }
}
