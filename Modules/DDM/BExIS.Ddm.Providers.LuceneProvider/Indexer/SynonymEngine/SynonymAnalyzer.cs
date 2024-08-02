using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

/// <summary>
///
/// </summary>
namespace Lucene.Net.SynonymEngine
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class SynonymAnalyzer : Analyzer
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public ISynonymEngine SynonymEngine { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="engine"></param>
        /// <return>NA</return>
        public SynonymAnalyzer(ISynonymEngine engine)
        {
            SynonymEngine = engine;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="fieldName"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader)
        {
            //create the tokenizer
            TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_30, reader);

            //add in filters
            result = new StandardFilter(result); // first normalize the StandardTokenizer
            result = new LowerCaseFilter(result);// makes sure everything is lower case
            result = new StopFilter(true, result, StopAnalyzer.ENGLISH_STOP_WORDS_SET); // use the default list of Stop Words, provided by the StopAnalyzer class.
            result = new SynonymFilter(result, SynonymEngine); // injects the synonyms.

            //return the built token stream.
            return result;
        }
    }
}