using System.IO;
using System.Linq;
using System.Text;


namespace BExIS.Utils.Files
{
    /// <summary>
    /// Provides utility methods for handling and sanitizing file names based on operating system rules.
    /// </summary>
    public static class FileNameUtility
    {
        /// <summary>
        /// Replaces all invalid file name characters with a specified replacement character.
        /// It ensures the resulting string is safe to use as a file name segment on Windows/Unix-like systems.
        /// </summary>
        /// <param name="fileName">The original file name string to sanitize.</param>
        /// <param name="replacementChar">The character to use for replacing invalid characters (default is '-').</param>
        /// <returns>A sanitized string suitable for use as a file name.</returns>
        public static string SanitizeFileName(string fileName, char replacementChar = '-')
        {
            // 1. Check for null or empty input
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            // 2. Get the array of characters forbidden in file names by the operating system.
            // This is the most reliable way to handle OS-specific invalid characters.
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // 3. Use a StringBuilder for efficient string manipulation
            StringBuilder sb = new StringBuilder(fileName.Length);

            // 4. Iterate through the input string and replace invalid characters
            foreach (char c in fileName)
            {
                if (invalidChars.Contains(c))
                {
                    // Replace the invalid character with the specified replacement char
                    sb.Append(replacementChar);
                }
                else
                {
                    // Keep valid characters as they are
                    sb.Append(c);
                }
            }

            string sanitizedName = sb.ToString();

            // 5. Post-sanitization cleanup for specific Windows constraints (ends with '.' or ' ')
            // While replacement should handle most issues, it's good practice to ensure
            // the name doesn't end in characters that Windows implicitly trims or handles poorly.

            // Trim spaces from the end
            sanitizedName = sanitizedName.TrimEnd(' ');

            // If the name ends with the replacement char (e.g., if the original ended with a '.'), 
            // ensure it doesn't leave an ending period (Windows specific) unless it's part of an extension.
            // We'll use a simple check to ensure it doesn't end with a period.
            if (sanitizedName.EndsWith("."))
            {
                // Remove trailing period if it exists
                sanitizedName = sanitizedName.TrimEnd('.');
            }

            // Optional: Replace multiple consecutive replacement characters with a single one.
            // This prevents "file---name--.txt" from becoming "file-name.txt".
            string doubleReplacement = new string(new[] { replacementChar, replacementChar });
            while (sanitizedName.Contains(doubleReplacement))
            {
                sanitizedName = sanitizedName.Replace(doubleReplacement, replacementChar.ToString());
            }

            return sanitizedName;
        }
    }
}
