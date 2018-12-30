using System.Collections.Generic;
using PictureApp.API.Data;
using PictureApp.API.Models;

namespace PictureApp.API.SeedData
{
    public class Seed
    {
     private readonly DataContext context;
        public Seed(DataContext context)
        {
            this.context = context;
        }

        public void SeedUsers()
        {
            var userData = System.IO.File.ReadAllText("SeedData/20181119_UserSeedData.json");

            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(userData);

            foreach (var user in users)
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash("password", out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Username = user.Username.ToLower();

                context.Users.Add(user);
           }
            context.SaveChanges();
        }

        public void SeedNotificationTemplates()
        {
            var notificationTemplate = new NotificationTemplate
            {
                Name = "Activation of an account after registration",
                Abbreviation = "ARR",
                Description = "This template is using for notify users who has just registered their account and need a full activation.",
                Body = "Dear user {UserName}, <br /> Here is your activation link: <a href=\"{ActivationUri}\">{ActivationUri}</a>.",
                Subject = "Account activation"
            };

            context.Add(notificationTemplate);
            context.SaveChanges();
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }               
    }
}