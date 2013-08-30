using System;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.SynonymEngine;

namespace BExIS.Search.Providers.LuceneProvider.Indexer
{
    class BexisAnalyzer : Analyzer
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



        public BexisAnalyzer()
        {
            stoptable = StopFilter.MakeStopSet(GERMAN_STOP_WORDS);
            SynonymEngine = new BexisSynonymEngine();
        }



        /// <summary>
        /// Builds an exclusionlist from an array of Strings. 
        /// </summary>
        /// <param name="exclusionlist"></param>
        public void SetStemExclusionTable(String[] exclusionlist)
        {
            excltable = StopFilter.MakeStopSet(exclusionlist);
        }

        /// <summary>
        /// Builds an exclusionlist from a Hashtable. 
        /// </summary>
        /// <param name="exclusionlist"></param>
        public void SetStemExclusionTable(ISet<string> exclusionlist)
        {
            excltable = exclusionlist;
        }


        public int getPositionIncrementGap(String fieldName)
        {

            return 100;
        }


        public override TokenStream TokenStream(String fieldName, System.IO.TextReader reader)
        {
            TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_30, reader);
            result = new StandardFilter(result);
            result = new LowerCaseFilter(result);
            result = new StopFilter(true, result, StopAnalyzer.ENGLISH_STOP_WORDS_SET);
            result = new StopFilter(true, result, stoptable);
            //result = new GermanStemFilter(result, excltable);
            //result = new PorterStemFilter(result);
            result = new SynonymFilter(result, SynonymEngine); // injects the synonyms. 
            return result;
        }

        //  public override int getPositionIncrementGap(System.String fieldName)
        //{
        //return 100;
        //}

    }
}