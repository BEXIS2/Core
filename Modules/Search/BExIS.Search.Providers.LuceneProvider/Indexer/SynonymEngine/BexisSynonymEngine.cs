using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using WorldNet.Net;
using System.Web;

using System.Diagnostics;
using BExIS.Search.Providers.LuceneProvider.Helpers;
using System.IO;


namespace Lucene.Net.SynonymEngine
{
    class BexisSynonymEngine : ISynonymEngine
    {

        IndexSearcher searcherTranslator;
        IndexSearcher searcherWordnet;
        Lucene.Net.Store.Directory fsDir;


        public BexisSynonymEngine()
        {
            string wordNetIndexPath = Path.Combine(FileHelper.IndexFolderPath, "WordnetIndex");
            string englishGermanIndexPath = Path.Combine(FileHelper.IndexFolderPath, "EnglishGermanIndex");
            searcherTranslator = new IndexSearcher(FSDirectory.Open(new System.IO.DirectoryInfo(englishGermanIndexPath)), true);
            searcherWordnet = new IndexSearcher(FSDirectory.Open(new System.IO.DirectoryInfo(wordNetIndexPath)), true);
        }

        public void close()
        {
            searcherWordnet.Dispose();
            searcherTranslator.Dispose();
        }

        public String[] GetSynonyms(string theWord)
        {
            List<string> synList = new List<string>();
            TopDocs tdsTranslator = searcherTranslator.Search(new TermQuery(new Term("word", theWord)), 100);
            foreach (ScoreDoc sd in tdsTranslator.ScoreDocs)
            {
                Document doc = searcherTranslator.Doc(sd.Doc);
                String[] values = doc.GetValues("syn");
                for (int j = 0; j < values.Length; j++)
                {
                    synList.Add(values[j]);
                }
            }
            TopDocs tdsWordnet = searcherWordnet.Search(new TermQuery(new Term("word", theWord)), 100);

            foreach (ScoreDoc sd in tdsWordnet.ScoreDocs)
            {
                Document doc = searcherWordnet.Doc(sd.Doc);
                String[] values = doc.GetValues("syn");
                for (int j = 0; j < values.Length; j++)
                {
                    synList.Add(values[j]);
                }
            }
            return synList.ToArray();
        }

    }
}
