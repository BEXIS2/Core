using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>        
namespace BExIS.Security.Entities.Objects
{

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class DataPermission : Permission
    {
        public virtual long DataId { get; set; }
        public virtual string EntityId { get; set; }

        public virtual RightType RightType { get; set; }
    }

    /// <param RightType></param>        
    public enum RightType
    {
        Create = 0,
        Read = 1,
        Update = 2,
        Delete = 3
    }
}