using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using System;
using System.Collections.Generic;
using AttributeSource = Lucene.Net.Util.AttributeSource;

/// <summary>
///
/// </summary>
namespace Lucene.Net.SynonymEngine
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class SynonymFilter : TokenFilter
    {
        private Queue<Token> synonymTokenQueue
            = new Queue<Token>();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public ISynonymEngine SynonymEngine { get; private set; }

        public static readonly String TOKEN_TYPE_SYNONYM = "SYNONYM";
        private Stack<String> synonymStack;
        private ISynonymEngine engine;
        private AttributeSource.State current;
        private readonly ITermAttribute termAtt;
        private readonly IPositionIncrementAttribute posIncrAtt;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="input"></param>
        /// <param name="synonymEngine"></param>
        /// <return></return>
        public SynonymFilter(TokenStream input, ISynonymEngine synonymEngine)
            : base(input)
        {
            synonymStack = new Stack<String>();
            this.engine = synonymEngine;
            this.termAtt = AddAttribute<ITermAttribute>();
            this.posIncrAtt = AddAttribute<IPositionIncrementAttribute>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public override bool IncrementToken()
        {
            if (synonymStack.Count > 0)
            {
                String syn = synonymStack.Pop();
                RestoreState(current);
                termAtt.SetTermBuffer(syn);
                posIncrAtt.PositionIncrement = 0;
                return true;
            }
            if (!input.IncrementToken())
                return false;
            if (addAliasesToStack())
            {
                current = CaptureState();
            }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>
        /// <returns></returns>
        private bool addAliasesToStack()
        {
            String[] synonyms = engine.GetSynonyms(termAtt.Term);
            if (synonyms == null)
            {
                return false;
            }

            foreach (String synonym in synonyms)
            {
                synonymStack.Push(synonym);
            }
            return true;
        }
    }
}