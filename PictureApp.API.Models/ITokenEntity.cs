using System;
using System.Collections.Generic;
using System.Text;

namespace PictureApp.API.Models
{
    public interface ITokenEntity : IEntity
    {
        int UserId { get; set; }

        User User { get; set; }

        string Token { get; set; }
    }
}
