using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

/// <summary>
///
/// </summary>
namespace BExIS.Utils.Upload
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class Utility
    {
        /// <summary>
        /// Convert a list of strings to a md5 byte array
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ComputeKey(params string[] values)
        {
            string input = string.Join(",", values);

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}