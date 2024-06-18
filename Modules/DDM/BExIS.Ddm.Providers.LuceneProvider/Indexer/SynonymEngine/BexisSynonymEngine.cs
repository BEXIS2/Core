using BExIS.Ddm.Providers.LuceneProvider.Helpers;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
///
/// </summary>
namespace Lucene.Net.SynonymEngine
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    internal class BexisSynonymEngine : ISynonymEngine
    {
        private IndexSearcher searcherTranslator;
        private IndexSearcher searcherWordnet;
        private Lucene.Net.Store.Directory fsDir;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public BexisSynonymEngine()
        {
            string wordNetIndexPath = Path.Combine(FileHelper.IndexFolderPath, "WordnetIndex");
            string englishGermanIndexPath = Path.Combine(FileHelper.IndexFolderPath, "EnglishGermanIndex");
            searcherTranslator = new IndexSearcher(FSDirectory.Open(new System.IO.DirectoryInfo(englishGermanIndexPath)), true);
            searcherWordnet = new IndexSearcher(FSDirectory.Open(new System.IO.DirectoryInfo(wordNetIndexPath)), true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public void close()
        {
            searcherWordnet.Dispose();
            searcherTranslator.Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="theWord"></param>
        /// <returns></returns>
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