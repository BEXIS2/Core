using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BExIS.Security.Entities.Security
{
    public class DataPermission<TEntity> where TEntity : class
    {
        public virtual Expression<Func<TEntity, bool>> Expression { get; set; }
    }
}
