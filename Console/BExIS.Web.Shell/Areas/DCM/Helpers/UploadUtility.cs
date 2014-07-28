using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BExIS.Web.Shell.Areas.DCM.Helpers
{
    public class Utility
    {
        public static byte[] ComputeKey(params string[] values)
        {
            string input = string.Join(",",values);

            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.Default.GetBytes(input));
               
            }
        }
    }
}