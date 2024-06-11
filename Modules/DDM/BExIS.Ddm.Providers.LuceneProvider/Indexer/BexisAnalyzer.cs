using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.SynonymEngine;
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
    internal class BexisAnalyzer : Analyzer
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

        public ISynonymEngine SynonymEngine { get; private set; }

        /// <summary>
        /// Contains the stopwords used with the StopFilter.
        /// </summary>
        private ISet<string> stoptable;

        /// <summary>
        /// Contains words that should be indexed but not stemmed.
        /// </summary>
        private ISet<string> excltable;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public BexisAnalyzer()
        {
            stoptable = StopFilter.MakeStopSet(GERMAN_STOP_WORDS);
            SynonymEngine = new BexisSynonymEngine();
        }

        /// <summary>
        /// Builds an exclusionlist from an array of Strings.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="exclusionlist"></param>
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
        public void SetStemExclusionTable(ISet<string> exclusionlist)
        {
            excltable = exclusionlist;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>
        /// <returns></returns>
        public int getPositionIncrementGap(String fieldName)
        {
            return 100;
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
            result = new StopFilter(true, result, stoptable);
            //result = new GermanStemFilter(result, excltable);
            //result = new PorterStemFilter(result);
            result = new SynonymFilter(result, SynonymEngine); // injects the synonyms. #

            return result;
        }

        //  public override int getPositionIncrementGap(System.String fieldName)
        //{
        //return 100;
        //}
    }
}