using System.Collections.Generic;
using PictureApp.API.Data;
using PictureApp.API.Models;
using PictureApp.API.Providers;

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

            var passwordProvider = new PasswordProvider();
            
            users.ForEach(user => 
            {
                var computedPassword = passwordProvider.CreatePasswordHash("password");
                user.PasswordHash = computedPassword.Hash;
                user.PasswordSalt = computedPassword.Salt;
                user.Username = user.Username.ToLower();

                 context.Users.Add(user);
            });

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

            notificationTemplate = new NotificationTemplate
            {
                Name = "Reset password of an account",
                Abbreviation = "RPS",
                Description = "This template is using for notify users who want to reset their accounts passwords.",
                Body = "Dear user {UserName}, <br /> In order to reset account password please use following link: <a href=\"{ResetPasswordUri}\">{ResetPasswordUri}</a>.",
                Subject = "Reset account password"
            };

            context.Add(notificationTemplate);

            context.SaveChanges();
        }
    }
}