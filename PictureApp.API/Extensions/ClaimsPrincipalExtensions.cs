using System.Linq;
using System.Security.Claims;

namespace PictureApp.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        }
    }
}
