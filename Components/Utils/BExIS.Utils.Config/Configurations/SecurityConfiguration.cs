using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Utils.Config.Configurations
{
    public class SecurityConfiguration
    {
        [JsonProperty("userValidatorConfiguration")]
        public UserValidatorConfiguration UserValidatorConfiguration { get; set; }

        [JsonProperty("passwordValidatorConfiguration")]
        public PasswordValidatorConfiguration PasswordValidatorConfiguration { get; set; }
    }

    public class UserValidatorConfiguration
    {
        [JsonProperty("allowOnlyAlphanumericUserNames")]
        public bool AllowOnlyAlphanumericUserNames { get; set; }

        [JsonProperty("requireUniqueEmail")]
        public bool RequireUniqueEmail { get; set; }
    }

    public class PasswordValidatorConfiguration
    {
        [JsonProperty("requiredLength")]
        public int RequiredLength { get; set; }

        [JsonProperty("requireNonLetterOrDigit")]
        public bool RequireNonLetterOrDigit { get; set; }

        [JsonProperty("requireDigit")]
        public bool RequireDigit { get; set; }

        [JsonProperty("requireLowercase")]
        public bool RequireLowercase { get; set; }

        [JsonProperty("requireUppercase")]
        public bool RequireUppercase { get; set; }
    }
}
