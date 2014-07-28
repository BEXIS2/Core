using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/// <summary>
///
/// </summary>        
namespace BExIS.DCM.UploadWizard
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
    }
}
