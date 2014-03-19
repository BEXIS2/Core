using System.Collections.Generic;
using BExIS.Ddm.Model;

namespace BExIS.Ddm.Api
{
    public interface ISearchDesigner
    {
        List<SearchAttribute> Get();
        void Set(List<SearchAttribute> SearchAttributeList);

        void Reset();
        void Reload();

        List<string> GetMetadataNodes();


    }
}
