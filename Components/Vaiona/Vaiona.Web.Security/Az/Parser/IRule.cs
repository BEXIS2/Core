/*	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Contributed by and used with permission from Nick Muhonen of
 * Useable Concepts Inc. (http://www.useableconcepts.com/).
 * Copyright of this code is incorporated under the license terms
 * indicated in the AssemblyInfo.cs file of this project.
 *	* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

namespace Vaiona.Web.Security.Az.Parser
{
    public interface IAccessRule
    {
        bool Evaluate(Func<string, bool> roleMatcher, Func<string, bool> userMatcher);

        string ShowRule(int pad);
    }

    public interface IComparisonRule : IAccessRule
    {
        IAccessRule LValue { get; set; }
        IAccessRule RValue { get; set; }
    }
}