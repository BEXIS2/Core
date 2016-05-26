using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
///
/// </summary>        
namespace BExIS.Ddm.Providers.LuceneProvider.Helpers
{
    /// <summary>
    /// Lucene use some spacial Characters for query
    /// This Encoder find and replace this special Character for searching
    /// </summary>
    /// <remarks></remarks>        
    public static class EncoderHelper
    {
        static string[] specialCharacterArray = new string[] {"\\","\"", "+", "-","&&","||","!","(",")","{","}","[","]","~","*","?",":","<" };

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Encode(string value, bool encodeSpace= false)
        {
            string encodedValue = value;

            // has special characters
            if (SpecialCharactrersInValue(value))
            {
                encodedValue = ReplaceSpecialCharacters(value);
            }

            // has whitespace
            if (encodeSpace)
            {
                encodedValue = encodedValue.Replace(" ", @"\ ");
            }

            return encodedValue;
        }

        /// <summary>
        /// compare the list of Special Characters with the
        /// incoming value and add a \ before it
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>    
        /// <returns></returns>
        private static string ReplaceSpecialCharacters(string value,bool encodeSpace= false)
        {

            foreach (string specailCharacter in specialCharacterArray)
            {
                if (value.Contains(specailCharacter)) value = value.Replace(specailCharacter, @"\" + specailCharacter);
            }

            Debug.WriteLine(value);

            return value;
        }

        /// <summary>
        /// Check if SpecialCharacter is in the incoming value string
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>   
        /// <returns></returns>
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
