using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class ExternalSourceSnapshot
    {
        public string Source { get; }
        public string LocalPath { get; }
        public string ExternalName { get; }

        private ExternalSourceSnapshot(string source, string localPath, string externalName)
        {
            Source = source;
            LocalPath = localPath;
            ExternalName = externalName;
        }

        public static ExternalSourceSnapshot FromDto(ExternalSource dto)
        {
            return new ExternalSourceSnapshot(
                dto.Source,
                dto.LocalPath,
                dto.ExternalName
            );
        }
    }
}
