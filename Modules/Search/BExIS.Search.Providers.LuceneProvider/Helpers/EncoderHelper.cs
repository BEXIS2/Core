using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BExIS.Search.Providers.LuceneProvider.Helpers
{

    /// <summary>
    /// Lucene use some spacial Characters for query
    /// This Encoder find and replace this special Character for searching
    /// </summary>
    /// <remarks></remarks>        
    public static class EncoderHelper
    {
        static string[] specialCharacterArray = new string[] {"\\","\"", "+", "-","&&","||","!","(",")","{","}","[","]","~","*","?",":", };

        public static string Encode(string value)
        {
            if (SpecialCharactrersInValue(value))
            {
                return ReplaceSpecialCharacters(value);

            }

            return value;
        }


        /// <summary>
        /// compare the list of Special Characters with the
        /// incoming value and add a \ before it
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>       
        private static string ReplaceSpecialCharacters(string value)
        {

            foreach (string specailCharacter in specialCharacterArray)
            {
                if (value.Contains(specailCharacter)) value = value.Replace(specailCharacter, "\\" + specailCharacter);
            }


            return value;
        }

        /// <summary>
        /// Check if SpecialCharacter is in the incoming value string
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>   
        private static bool SpecialCharactrersInValue(string value)
        {
            string regExPattern = @"[+\-&&||!(){}[\]~*?:\\""]";

            Regex rgx = new Regex(regExPattern, RegexOptions.IgnoreCase);
            Match m = rgx.Match(value);
            if (m.Success)
            {
                return true;
            }

            return false;
        }

    }
}
