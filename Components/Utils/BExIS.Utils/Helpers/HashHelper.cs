using System.Security.Cryptography;
using System.Text;

namespace BExIS.Utils.Helpers
{
    public class HashHelper
    {
        /// <summary>
        /// create a hash of the incoming list of strings
        /// </summary>
        /// <param name="list"></param>
        /// <returns>hash string</returns>
        public static string CreateMD5Hash(params string[] list)
        {
            if (list == null || list.Length == 0) return "";

            string input = string.Join(",", list);

            if (string.IsNullOrEmpty(input)) return "";

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Step 2, convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}