using BExIS.Security.Entities.Objects;

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
        Read = 0,
        Write = 1,
        Delete = 2,
        Grant = 3
    }

    /// <summary>
    /// 
    /// </summary>
    public class EntityPermission : Permission
    {
        public virtual Entity Entity { get; set; }
        public virtual long Key { get; set; }
        public virtual short Rights { get; set; }
    }
}