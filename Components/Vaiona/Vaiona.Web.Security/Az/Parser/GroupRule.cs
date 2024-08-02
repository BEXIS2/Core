/*	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Contributed by and used with permission from Nick Muhonen of
 * Useable Concepts Inc. (http://www.useableconcepts.com/).
 * Copyright of this code is incorporated under the license terms
 * indicated in the AssemblyInfo.cs file of this project.
 *	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

namespace Vaiona.Web.Security.Az.Parser
{
    internal class GroupRule : IAccessRule
    {
        public IAccessRule InnerRule;

        public bool Evaluate(Func<string, bool> roleMatcher, Func<string, bool> userMatcher)
        {
            return InnerRule.Evaluate(roleMatcher, userMatcher);
        }

        public string ShowRule(int pad)
        {
            String padStr = new string(' ', pad * 4);

            return
                 padStr + "Begin Group\r\n" +
                 InnerRule.ShowRule(pad + 1) +
                 padStr + "End Group\r\n";
        }
    }
}