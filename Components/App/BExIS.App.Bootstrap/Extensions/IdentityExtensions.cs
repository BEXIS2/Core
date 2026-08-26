using System.Security.Principal;
using System.Security.Claims;

namespace BExIS.App.Bootstrap.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetDisplayName(this IPrincipal principal)
        {
            if (principal == null)
                return null;

            var identity = principal.Identity;
            if (identity == null || !identity.IsAuthenticated)
                return null;

            // Sicherer Weg: als ClaimsIdentity casten
            if (identity is ClaimsIdentity claimsIdentity)
            {
                var displayNameClaim = claimsIdentity.FindFirst("DisplayName");
                if (displayNameClaim != null)
                {
                    return displayNameClaim.Value;
                }

                // Fallback auf andere bekannte Claim-Typen (falls du mal umbenennst)
                var nameClaim = claimsIdentity.FindFirst(ClaimTypes.Name)
                               ?? claimsIdentity.FindFirst("name")
                               ?? claimsIdentity.FindFirst(ClaimTypes.GivenName);

                return nameClaim?.Value ?? identity.Name ?? string.Empty;
            }

            // Sehr alter oder nicht-Claims-basierter Principal
            return identity.Name ?? string.Empty;
        }
    }
}
