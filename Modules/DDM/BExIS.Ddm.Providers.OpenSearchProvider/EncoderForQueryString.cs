using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearchProvider
{
    public static class EncoderForQueryString
    {
        // Nur relevant für query_string
        private static readonly string[] SpecialChars =
            { "\\", "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]",
          "^", "\"", "~", "*", "?", ":", "/" , "<", ">" /* neu in ES */ };

        public static string EncodeForQueryString(string text)
        {
            foreach (var ch in SpecialChars)
                text = text.Replace(ch, "\\" + ch);

            return text;
        }
    }
}
