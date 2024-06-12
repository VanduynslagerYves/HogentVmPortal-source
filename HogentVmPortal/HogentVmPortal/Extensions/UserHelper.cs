using System.Security.Claims;
using System.Security.Principal;

namespace HogentVmPortal.Extensions
{
    public static class UserHelper
    {
        public static string GetId(this IPrincipal principal)
        {
            ClaimsIdentity? claimsIdentity = principal.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            //if (string.IsNullOrEmpty(claim?.Value)) throw new KeyNotFoundException();
            return claim?.Value ?? string.Empty;
        }
    }
}
