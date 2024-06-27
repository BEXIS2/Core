using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using System;
using System.Collections.Generic;

/// <summary>
///
/// </summary>
namespace BExIS.Ddm.Providers.LuceneProvider.Indexer
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    internal class NGramAnalyzer : Analyzer
    {
        private String[] GERMAN_STOP_WORDS =
        {
            "einer", "eine", "eines", "einem", "einen",
            "der", "die", "das", "dass", "daß",
            "du", "er", "sie", "es",
            "was", "wer", "wie", "wir",
            "und", "oder", "ohne", "mit",
            "am", "im", "in", "aus", "auf",
            "ist", "sein", "war", "wird",
            "ihr", "ihre", "ihres",
            "als", "für", "von", "aus",
            "dich", "dir", "mich", "mir",
            "mein", "kein", "schulze",
            "durch", "wegen", "den", "im", "fur", "für",  "mit", "zur", "von"
        };

        /// <summary>
        /// Contains the stopwords used with the StopFilter.
        /// </summary>
        private ICollection<string> stoptable = new List<string>();

        /// <summary>
        /// Contains words that should be indexed but not stemmed.
        /// </summary>
        private ICollection<string> excltable = new List<string>();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public NGramAnalyzer()
        {
            stoptable = StopFilter.MakeStopSet(GERMAN_STOP_WORDS);
        }

        /// <summary>
        /// Builds an exclusionlist from an array of Strings.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="exclusionlist"></param>
        /// <return></return>
        public void SetStemExclusionTable(String[] exclusionlist)
        {
            excltable = StopFilter.MakeStopSet(exclusionlist);
        }

        /// <summary>
        /// Builds an exclusionlist from a Hashtable.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="exclusionlist"></param>
        /// <return></return>
        public void SetStemExclusionTable(ICollection<string> exclusionlist)
        {
            excltable = exclusionlist;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="fieldName"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public override TokenStream TokenStream(String fieldName, System.IO.TextReader reader)
        {
            TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_30, reader);
            result = new StandardFilter(result);
            result = new LowerCaseFilter(result);
            result = new StopFilter(true, result, StopAnalyzer.ENGLISH_STOP_WORDS_SET);
            result = new NGramTokenFilter(result, 1, 30);

            //result = new StopFilter(true, result, stoptable);
            //result = new PorterStemFilter(result);

            //result = new GermanStemFilter(result, excltable);

            return result;
        }

        //  public override int getPositionIncrementGap(System.String fieldName)
        //{
        //return 100;
        //}
    }
}