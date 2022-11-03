using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Authorization
{
    /// <summary>
    /// This enumeration reflects the set of different rights used in BExIS.
    /// We tried keep it as simple as possible and use only 3 (4) types.
    ///
    /// Read, Write, Delete - e.g. OS specific rights on files/folders
    /// Grant - specific right to allow transfer of the other three rights
    /// to other users
    ///
    /// In general, it could be solved by using some other concepts like
    /// "roles" or "rules" for users.
    /// </summary>
    public enum RightType
    {
        Read = 1,

        // The right was removed, as reading rights already include the possibility to copy data.
        // In the system there are the table data but also files. as soon as you have read-only files,
        // you have to open the file but then you can also save it directly.
        // Download = 2, REMOVED
        Write = 4,

        Delete = 8,
        Grant = 16
    }

    /// <summary>
    ///
    /// </summary>
    public class EntityPermission : BaseEntity
    {
        public virtual Entity Entity { get; set; }
        public virtual long Key { get; set; }
        public virtual int Rights { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual DateTime CreationDate { get; set; }

        public EntityPermission()
        {
            CreationDate = DateTime.Now;
        }
    }
}