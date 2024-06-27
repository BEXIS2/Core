/*	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Contributed by and used with permission from Nick Muhonen of
 * Useable Concepts Inc. (http://www.useableconcepts.com/).
 * Copyright of this code is incorporated under the license terms
 * indicated in the AssemblyInfo.cs file of this project.
 *	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

namespace Vaiona.Web.Security.Az.Parser
{
    internal class Or : IComparisonRule
    {
        public IAccessRule LValue { get; set; }
        public IAccessRule RValue { get; set; }

        public bool Evaluate(Func<string, bool> roleMatcher, Func<string, bool> userMatcher)
        {
            return LValue.Evaluate(roleMatcher, userMatcher) || RValue.Evaluate(roleMatcher, userMatcher);
        }

        public string ShowRule(int pad)
        {
            String padStr = new string(' ', pad * 4);

            return
                 padStr + "Begin Or\r\n" +
                 LValue.ShowRule(pad + 1) +
                 RValue.ShowRule(pad + 1) +
                 padStr + "End Or\r\n";
        }
    }
}