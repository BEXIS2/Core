using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public class LocalComponentSnapshot
    {
        public int GlobalId { get; set; }
        public DataTypeId DataTypeId { get; set; }
        public IReadOnlyList<string> MetadataNodes { get; set; } = new List<string>();

        public LocalComponentSnapshot(int globalId, DataTypeId dataTypeId, IReadOnlyList<string> metadataNodes)
        {
            GlobalId = globalId;
            DataTypeId = dataTypeId;
            MetadataNodes = metadataNodes;
        }

        public static LocalComponentSnapshot FromDto(LocalComponent dto)
        {
            return new LocalComponentSnapshot(
                dto.GlobalId,
                dto.DataTypeId,
                dto.MetadataNodes.ToList().AsReadOnly()
            );

        }
    }
}
