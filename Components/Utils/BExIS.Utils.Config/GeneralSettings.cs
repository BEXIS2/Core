using BExIS.Utils.Config.Configurations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http.Results;
using Vaiona.IoC;
using Vaiona.Utils.Cfg;

namespace BExIS.Utils.Config
{
    /// <summary>
    /// Contains all the general settings of the shell.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>This is a singleton class that must be instaniated via the IoC only</item>
    ///         <item>This class relies on the AppConfiguration config items such as workspace root, etc., hence those settings must not be moved to the settings.</item>
    ///         <item>Some web.config items such as databse connection string and the IoC condif items are needed before this object is instantiated, thgose config items can not leave web.config.</item>
    ///     </list>
    /// </remarks>
    public class GeneralSettings : Settings
    {
        public GeneralSettings() : base("Shell", Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.json"))
        {
        }

        public string GetApplicationName()
        {
            try
            {
                return GetValueByKey("applicationName").ToString();
            }
            catch (Exception ex)
            {
                return "BEXIS2";
            }
        }

        public SecurityConfiguration GetSecurityConfiguration()
        {
            try
            {
                return GetValueByKey<SecurityConfiguration>("security");
            }
            catch (Exception ex)
            {
                return new SecurityConfiguration();
            }
        }

        public JwtConfiguration GetJwtConfiguration()
        {
            try
            {
                return GetValueByKey<JwtConfiguration>("jwt");
            }
            catch (Exception ex)
            {
                return new JwtConfiguration();
            }
        }

        public SmtpConfiguration GetSmtpConfiguration()
        {
            try
            {
                return GetValueByKey<SmtpConfiguration>("smtp");
            }
            catch (Exception ex)
            {
                return new SmtpConfiguration();
            }
        }

        public List<LdapConfiguration> GetLdapConfigurations()
        {
            try
            {
                return GetValueByKey<List<LdapConfiguration>>("ldaps");
            }
            catch (Exception ex)
            {
                return new List<LdapConfiguration>();
            }
        }

        public static string ApplicationInfo
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ApplicationName))
                {
                    if (!string.IsNullOrWhiteSpace(ApplicationVersion))
                        return (string.Format("{0} {1}", ApplicationName, ApplicationVersion));
                    return (string.Format(ApplicationName));
                }
                return string.Empty;
            }
        }

        public static string ApplicationName
        {
            get
            {
                try
                {
                    return GetValueByKey("applicationName").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static string ApplicationVersion
        {
            get
            {
                try
                {
                    return (ConfigurationManager.AppSettings["ApplicationVersion"]);
                }
                catch { return (string.Empty); }
            }
        }

        public static string FAQ
        {
            get
            {
                try
                {
                    return GetValueByKey("faq").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static SecurityConfiguration SecurityConfiguration
        {
            get
            {
                var json = GetValueByKey("security")?.ToString();

                if (string.IsNullOrWhiteSpace(json))
                    return new SecurityConfiguration();

                try
                {
                    var config = JsonConvert.DeserializeObject<SecurityConfiguration>(json);
                    return config ?? new SecurityConfiguration();
                }
                catch (JsonException ex)
                {

                    return new SecurityConfiguration();
                }
            }
        }

        public static JwtConfiguration JwtConfiguration
        {
            get
            {
                var json = GetValueByKey("jwt")?.ToString();

                if (string.IsNullOrWhiteSpace(json))
                    return new JwtConfiguration(); 

                try
                {
                    var config = JsonConvert.DeserializeObject<JwtConfiguration>(json);
                    return config ?? new JwtConfiguration() { IsActive = false };
                }
                catch (JsonException ex)
                {
                    return new JwtConfiguration() { IsActive = false };
                }
            }
        }

        public static bool CreateIssuerSigningKey()
        {
            GeneralSettings settings = Get();
            JsonSettings jsonSettings = settings.GetAsJsonModel();

            // Finde den Eintrag mit key = "jwt"
            var jwtEntry = jsonSettings.Entries.FirstOrDefault(e => e.Key == "jwt");
            if (jwtEntry == null)
                throw new InvalidOperationException("Kein Eintrag mit key='jwt' gefunden.");

            // Annahme: jwtEntry.Value ist ein JObject
            if (jwtEntry.Value is JObject jwtValueObj)
            {
                // Ändere den Wert direkt im Objekt
                jwtValueObj["issuerSigningKey"] = generateRandomAlphanumericString(48);
            }
            else
            {
                throw new InvalidOperationException("Der Wert von 'jwt' ist kein JObject.");
            }

            // Speichere zurück
            settings.Update(jsonSettings);

            return true;
        }

        private static string generateRandomAlphanumericString(int length = 48)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using (var rng = RandomNumberGenerator.Create())
            {
                var result = new StringBuilder(length);
                var buffer = new byte[16]; // Genügend für mehrere Zeichen

                while (result.Length < length)
                {
                    rng.GetBytes(buffer);
                    for (int i = 0; i < buffer.Length && result.Length < length; i++)
                    {
                        // Nutze den Wert als Index in den Zeichen
                        result.Append(chars[buffer[i] % chars.Length]);
                    }
                }

                return result.ToString();
            }
        }

        public static SmtpConfiguration SmtpConfiguration
        {
            get
            {
                var json = GetValueByKey("smtp")?.ToString();

                if (string.IsNullOrWhiteSpace(json))
                    return new SmtpConfiguration() { IsActive = false };

                try
                {
                    var config = JsonConvert.DeserializeObject<SmtpConfiguration>(json);
                    return config ?? new SmtpConfiguration() { IsActive = false };
                }
                catch (JsonException ex)
                {
                    return new SmtpConfiguration() { IsActive = false };
                }
            }
        }

        public static ExceptionConfiguration ExceptionConfiguration
        {
            get
            {
                var json = GetValueByKey("exception")?.ToString();

                if (string.IsNullOrWhiteSpace(json))
                    return new ExceptionConfiguration();   // leere Instanz mit Defaults

                try
                {
                    var config = JsonConvert.DeserializeObject<ExceptionConfiguration>(json);
                    return config ?? new ExceptionConfiguration();
                }
                catch (JsonException ex)
                {
                    // Logger.Error($"Fehler beim Deserialisieren der SecurityConfiguration: {ex.Message}");
                    return new ExceptionConfiguration();   // Fallback auf Defaults
                }
            }
        }

        public static List<LdapConfiguration> LdapConfigurations
        {
            get
            {
                try
                {
                    var ldapConfigurations = new List<LdapConfiguration>();
                    List<Entry> ldaps = (List<Entry>)GetValueByKey("ldaps");

                    foreach (var ldap in ldaps)
                    {
                        ldapConfigurations.Add(JsonConvert.DeserializeObject<LdapConfiguration>(ldap.Value.ToString()));
                    }

                    return ldapConfigurations;
                }
                catch (Exception ex)
                {
                    return new List<LdapConfiguration>();
                }
            }
        }

        public static string LandingPage
        {
            get
            {
                try
                {
                    return GetValueByKey("landingPage").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static string LandingPageForUsers
        {
            get
            {
                try
                {
                    return GetValueByKey("landingPageForUsers").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static string LandingPageForUsersNoPermission
        {
            get
            {
                try
                {
                    return GetValueByKey("landingPageForUsersNoPermission").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static string PersonEmailAttributeName
        {
            get
            {
                try
                {
                    return GetValueByKey("personEmailAttributeName").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static bool SendExceptions
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(GetValueByKey("sendExceptions"));
                }
                catch { return (false); }
            }
        }

        public static string SystemEmail
        {
            get
            {
                try
                {
                    return GetValueByKey("systemEmail").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static bool UseMultiMediaModule
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(GetValueByKey("useMultiMediaModule"));
                }
                catch { return (false); }
            }
        }

        public static bool UsePersonEmailAttributeName
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(GetValueByKey("usePersonEmailAttributeName"));
                }
                catch { return (false); }
            }
        }

        public static GeneralSettings Get()
        {
            GeneralSettings generalSettings = null;
            try
            {
                generalSettings = IoCFactory.Container.Resolve<GeneralSettings>() as GeneralSettings;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load general settings", ex);
            }
            return (generalSettings);
        }

        public static object GetValueByKey(string entryKey)
        {
            Entry entry = Get().jsonSettings.Entries.Where(p => p.Key.Equals(entryKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (entry == null)
                return null;
            string value = entry.Value.ToString();
            EntryType type = entry.Type;

            switch (type)
            {
                case (EntryType.EntryList):
                    return JsonConvert.DeserializeObject<List<Entry>>(value);

                case (EntryType.JSON):
                    return Convert.ChangeType(value, TypeCode.String);

                default:
                    return Convert.ChangeType(value, (TypeCode)Enum.Parse(typeof(TypeCode), type.ToString()));
            }
        }
    }
}