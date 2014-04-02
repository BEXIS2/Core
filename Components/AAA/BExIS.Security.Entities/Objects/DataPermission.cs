using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BExIS.Security.Entities.Objects
{
    public class DataPermission : Permission
    {
        // Entity
        public virtual string EntityName { get; set; }
        
        // Rights
        public virtual bool Create { get; set; }
        public virtual bool Read { get; set; }
        public virtual bool Update { get; set; }
        public virtual bool Delete { get; set; }

        // Statement
        public virtual Expression Statement { get; set; } 
    }
}
