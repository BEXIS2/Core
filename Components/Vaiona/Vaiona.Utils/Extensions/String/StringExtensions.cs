﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        #region Default Rules

        static StringExtensions()
        {
            AddPlural("$", "s");
            AddPlural("s$", "s");
            AddPlural("(ax|test)is$", "$1es");
            AddPlural("(octop|vir|alumn|fung)us$", "$1i");
            AddPlural("(alias|status)$", "$1es");
            AddPlural("(bu)s$", "$1ses");
            AddPlural("(buffal|tomat|volcan)o$", "$1oes");
            AddPlural("([ti])um$", "$1a");
            AddPlural("sis$", "ses");
            AddPlural("(?:([^f])fe|([lr])f)$", "$1$2ves");
            AddPlural("(hive)$", "$1s");
            AddPlural("([^aeiouy]|qu)y$", "$1ies");
            AddPlural("(x|ch|ss|sh)$", "$1es");
            AddPlural("(matr|vert|ind)ix|ex$", "$1ices");
            AddPlural("([m|l])ouse$", "$1ice");
            AddPlural("^(ox)$", "$1en");
            AddPlural("(quiz)$", "$1zes");

            AddSingular("s$", "");
            AddSingular("(n)ews$", "$1ews");
            AddSingular("([ti])a$", "$1um");
            AddSingular("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)ses$", "$1$2sis");
            AddSingular("(^analy)ses$", "$1sis");
            AddSingular("([^f])ves$", "$1fe");
            AddSingular("(hive)s$", "$1");
            AddSingular("(tive)s$", "$1");
            AddSingular("([lr])ves$", "$1f");
            AddSingular("([^aeiouy]|qu)ies$", "$1y");
            AddSingular("(s)eries$", "$1eries");
            AddSingular("(m)ovies$", "$1ovie");
            AddSingular("(x|ch|ss|sh)es$", "$1");
            AddSingular("([m|l])ice$", "$1ouse");
            AddSingular("(bus)es$", "$1");
            AddSingular("(o)es$", "$1");
            AddSingular("(shoe)s$", "$1");
            AddSingular("(cris|ax|test)es$", "$1is");
            AddSingular("(octop|vir|alumn|fung)i$", "$1us");
            AddSingular("(alias|status)es$", "$1");
            AddSingular("^(ox)en", "$1");
            AddSingular("(vert|ind)ices$", "$1ex");
            AddSingular("(matr)ices$", "$1ix");
            AddSingular("(quiz)zes$", "$1");

            AddIrregular("person", "people");
            AddIrregular("man", "men");
            AddIrregular("child", "children");
            AddIrregular("sex", "sexes");
            AddIrregular("move", "moves");
            AddIrregular("goose", "geese");
            AddIrregular("alumna", "alumnae");

            AddUncountable("equipment");
            AddUncountable("information");
            AddUncountable("rice");
            AddUncountable("money");
            AddUncountable("species");
            AddUncountable("series");
            AddUncountable("fish");
            AddUncountable("sheep");
            AddUncountable("deer");
            AddUncountable("aircraft");
        }

        #endregion Default Rules

        private class Rule
        {
            private readonly Regex _regex;
            private readonly string _replacement;

            public Rule(string pattern, string replacement)
            {
                _regex = new Regex(pattern, RegexOptions.IgnoreCase);
                _replacement = replacement;
            }

            public string Apply(string word)
            {
                if (!_regex.IsMatch(word))
                {
                    return null;
                }

                return _regex.Replace(word, _replacement);
            }
        }

        private static void AddIrregular(string singular, string plural)
        {
            AddPlural("(" + singular[0] + ")" + singular.Substring(1) + "$", "$1" + plural.Substring(1));
            AddSingular("(" + plural[0] + ")" + plural.Substring(1) + "$", "$1" + singular.Substring(1));
        }

        private static void AddUncountable(string word)
        {
            _uncountables.Add(word.ToLower());
        }

        private static void AddPlural(string rule, string replacement)
        {
            _plurals.Add(new Rule(rule, replacement));
        }

        private static void AddSingular(string rule, string replacement)
        {
            _singulars.Add(new Rule(rule, replacement));
        }

        private static readonly List<Rule> _plurals = new List<Rule>();
        private static readonly List<Rule> _singulars = new List<Rule>();
        private static readonly List<string> _uncountables = new List<string>();

        public static string Pluralize(this string word)
        {
            return ApplyRules(_plurals, word);
        }

        public static string Singularize(this string word)
        {
            return ApplyRules(_singulars, word);
        }

        private static string ApplyRules(List<Rule> rules, string word)
        {
            string result = word;

            if (!_uncountables.Contains(word.ToLower()))
            {
                for (int i = rules.Count - 1; i >= 0; i--)
                {
                    if ((result = rules[i].Apply(word)) != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public static string Titleize(this string word)
        {
            return Regex.Replace(Humanize(Underscore(word)), @"\b([a-z])",
                                 delegate (Match match)
                                 {
                                     return match.Captures[0].Value.ToUpper();
                                 });
        }

        public static string Humanize(this string lowercaseAndUnderscoredWord)
        {
            return Capitalize(Regex.Replace(lowercaseAndUnderscoredWord, @"_", " "));
        }

        public static string Pascalize(this string lowercaseAndUnderscoredWord)
        {
            return Regex.Replace(lowercaseAndUnderscoredWord, "(?:^|_)(.)",
                                 delegate (Match match)
                                 {
                                     return match.Groups[1].Value.ToUpper();
                                 });
        }

        public static string Camelize(this string lowercaseAndUnderscoredWord)
        {
            return Uncapitalize(Pascalize(lowercaseAndUnderscoredWord));
        }

        public static string Underscore(this string pascalCasedWord)
        {
            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])",
                    "$1_$2"), @"[-\s]", "_").ToLower();
        }

        public static string Capitalize(this string word)
        {
            return word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower();
        }

        /// <summary>
        /// Capitalize the first letter and do not touch the rest, it is not like PascalCase!
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string CapitalizeShort(this string word)
        {
            return word.Substring(0, 1).ToUpper() + word.Substring(1);
        }

        public static string Uncapitalize(this string word)
        {
            return word.Substring(0, 1).ToLower() + word.Substring(1);
        }

        public static string Ordinalize(this string numberString)
        {
            return Ordinalize(int.Parse(numberString), numberString);
        }

        public static string Ordinalize(this int number)
        {
            return Ordinalize(number, number.ToString());
        }

        private static string Ordinalize(int number, string numberString)
        {
            int nMod100 = number % 100;

            if (nMod100 >= 11 && nMod100 <= 13)
            {
                return numberString + "th";
            }

            switch (number % 10)
            {
                case 1:
                    return numberString + "st";

                case 2:
                    return numberString + "nd";

                case 3:
                    return numberString + "rd";

                default:
                    return numberString + "th";
            }
        }

        public static string Dasherize(this string underscoredWord)
        {
            return underscoredWord.Replace('_', '-');
        }

        // subject to more tests
        public static string Truncate(this string input, int length)
        {
            if (input.Length <= length) return input;
            else return input.Substring(0, length) + "..."; // length -3 ?? Javad
        }

        // subject to more tests
        private static string keywordPattern(string searchKeyword)
        {
            var keywords = searchKeyword.Split(',').Select(k => k.Trim()).Where(k => k != "").Select(k => Regex.Escape(k));

            return @"\b(" + string.Join("|", keywords) + @")\b";
        }

        // subject to more tests
        public static bool ContainsExact(this string input, string searchKeywords)
        {
            var pattern = keywordPattern(searchKeywords);
            Regex exp = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return exp.IsMatch(input);
        }
    }
}