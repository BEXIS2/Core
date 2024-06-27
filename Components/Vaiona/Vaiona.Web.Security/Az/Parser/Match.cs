/*	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Contributed by and used with permission from Nick Muhonen of
 * Useable Concepts Inc. (http://www.useableconcepts.com/).
 * Copyright of this code is incorporated under the license terms
 * indicated in the AssemblyInfo.cs file of this project.
 *	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

namespace Vaiona.Web.Security.Az.Parser
{
    internal class Match : IAccessRule
    {
        public string Role { get; set; }

        public bool Evaluate(Func<string, bool> roleMatcher, Func<string, bool> userMatcher)
        {
            if (Role.StartsWith("@")) // its a username
                return (userMatcher(Role.Substring(1)));
            return roleMatcher(Role);
        }

        public string ShowRule(int pad)
        {
            return new String(' ', pad * 4) + "Match: " + Role + "\r\n";
        }
    }
}