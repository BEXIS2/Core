using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using AttributeSource = Lucene.Net.Util.AttributeSource;

namespace Lucene.Net.SynonymEngine
{
    public class SynonymFilter : TokenFilter
    {
        private Queue<Token> synonymTokenQueue
            = new Queue<Token>();

        public ISynonymEngine SynonymEngine { get; private set; }


        public static readonly String TOKEN_TYPE_SYNONYM = "SYNONYM";
private Stack<String> synonymStack;
private ISynonymEngine engine;
private AttributeSource.State current;
private readonly ITermAttribute termAtt;
private readonly IPositionIncrementAttribute posIncrAtt;




        public SynonymFilter(TokenStream input, ISynonymEngine synonymEngine)
            : base(input)
        {

            synonymStack = new Stack<String>();
this.engine = synonymEngine;
this.termAtt = AddAttribute<ITermAttribute>();
this.posIncrAtt = AddAttribute<IPositionIncrementAttribute>();


        }


        public override bool IncrementToken()
        {
if (synonymStack.Count > 0) {
String syn = synonymStack.Pop();
RestoreState(current);
termAtt.SetTermBuffer(syn);
posIncrAtt.PositionIncrement=0;
return true;
}
if (!input.IncrementToken())
return false;
if (addAliasesToStack()) {
current = CaptureState();
}
return true;
}


        private bool addAliasesToStack() {
String[] synonyms = engine.GetSynonyms(termAtt.Term);
if (synonyms == null) {
return false;
}
foreach (String synonym in synonyms) {
synonymStack.Push(synonym);
}
return true;
}




       
    }
}
