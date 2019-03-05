using System;
using System.Collections.Generic;
using System.Text;

namespace PictureApp.API.Models
{
    public class ResetPasswordToken : Entity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
    }
}
