using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Search.Model;

namespace BExIS.Search.Api
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
