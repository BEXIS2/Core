using System;

/// <summary>
///
/// </summary>
namespace Lucene.Net.SynonymEngine
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public interface ISynonymEngine
    {
        String[] GetSynonyms(string word);
    }
}