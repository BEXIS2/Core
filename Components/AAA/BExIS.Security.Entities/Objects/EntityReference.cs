using System;
using System.ComponentModel.DataAnnotations;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public enum DefaultEntitiyReferenceType
    {
        [Display(Name = "metadata link")]
        MetadataLink
    }

    public enum EntityReferenceXmlAttribute
    {
        entityid,
        entitytype,
        entityversion
    }

    public class EntityReference : BaseEntity
    {
        public virtual long SourceId { get; set; }
        public virtual long SourceEntityId { get; set; }
        public virtual int SourceVersion { get; set; }
        public virtual long TargetId { get; set; }
        public virtual long TargetEntityId { get; set; }
        public virtual int TargetVersion { get; set; }
        public virtual string Context { get; set; }
        public virtual string ReferenceType { get; set; }
        public virtual string LinkType { get; set; }
        public virtual string Category { get; set; }
        public virtual DateTime CreationDate { get; set; }

        public EntityReference()
        {
            SourceId = 0;
            SourceEntityId = 0;
            TargetId = 0;
            TargetEntityId = 0;
            Context = "";
            ReferenceType = "";
            CreationDate = DateTime.Now;
            LinkType = "";
            Category = "";
            SourceVersion = 0;
            TargetVersion = 0;
        }

        public EntityReference(long sourceId, long sourceEntityId, int sourceVersion, long targetId, long targetEntityId, int targetVersion, string context, string type, DateTime creationDate, string linkType , string category)
        {
            SourceId = sourceId;
            SourceEntityId = sourceEntityId;
            SourceVersion = sourceVersion;
            TargetId = targetId;
            TargetEntityId = targetEntityId;
            TargetVersion = targetVersion;
            Context = context;
            ReferenceType = type;
            CreationDate = creationDate;
            Category = category;
            LinkType = linkType;
        }
    }
}