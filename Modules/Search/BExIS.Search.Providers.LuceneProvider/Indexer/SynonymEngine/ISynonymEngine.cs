using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucene.Net.SynonymEngine
{
    public interface ISynonymEngine
    {
        String[] GetSynonyms(string word);
    }
}
