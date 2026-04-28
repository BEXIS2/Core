using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Utils.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetDisplayName(this IIdentity identity)
        {
            // Hier kannst du eigene Logik implementieren
            // Beispiel: Casting auf eine konkrete User-Klasse, um Zugriff auf eigene Eigenschaften
            if (identity is User user)
            {
                return $"{user.DisplayName ?? user.Name}";
            }
            return "unknown";
        }
    }
}
