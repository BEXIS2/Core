using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Utils.Helpers
{
    public class ManualHelper
    {
        public static string GetUrl(string version, string module)
        {
            if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(module)) return "";

            //https://github.com/BEXIS2/Documents/blob/2.15/Manuals/BAM/Manual.md
            return string.Format("{0}://github.com/BEXIS2/Documents/blob/{1}/Manuals/{2}/Manual.md", "https", version, module);
        }
    }
}
