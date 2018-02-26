using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
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
        Download = 2,
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
    }
}