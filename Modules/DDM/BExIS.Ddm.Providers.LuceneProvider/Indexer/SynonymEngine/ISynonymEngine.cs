using System;

namespace Lucene.Net.SynonymEngine
{
    public interface ISynonymEngine
    {
        String[] GetSynonyms(string word);
    }
}
