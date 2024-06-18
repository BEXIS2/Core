using BExIS.Utils.Models;
using System.Collections.Generic;

namespace BExIS.Ddm.Api
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public interface ISearchDesigner
    {
        List<SearchAttribute> Get();

        void Set(List<SearchAttribute> SearchAttributeList);

        void Set(List<SearchAttribute> SearchAttributeList, bool includePrimaryData);

        void Reset();

        void Reload();

        void Dispose();

        List<SearchMetadataNode> GetMetadataNodes();

        bool IsPrimaryDataIncluded();
    }

    public enum IndexingAction
    { CREATE, UPDATE, DELETE }
}