using System;

namespace Vaiona.Entities.Common
{
    public interface ISystemVersionedEntity
    {
        //EntityVersionInfo VersionInfo { get; set; }
        Int32 VersionNo { get; set; }

        //DateTime? TimeStamp { get; set; }
    }
}