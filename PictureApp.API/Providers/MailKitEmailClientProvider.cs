using System;
using MimeKit;

namespace PictureApp.API.Providers
{
    public class MailKitEmailClientProvider : IEmailClientProvider
    {
        public void Send(string to, string subject, string body)
        {
            //throw new NotImplementedException();
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Joey Tribbiani", "joey@friends.com"));
            message.To.Add(new MailboxAddress("Mrs. Chanandler Bong", "chandler@friends.com"));
            message.Subject = "How you doin'?";

            message.Body = new TextPart("plain")
            {
                Text = @"Hey Chandler, I just wanted to let you know that Monica and I were going to go play some paintball, you in? -- Joey"
            };


        }
    }
}
