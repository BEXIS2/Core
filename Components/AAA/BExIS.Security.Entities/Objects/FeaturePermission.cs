using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Security.Entities.Objects
{
    public class FeaturePermission : Permission
    {
        public virtual Feature Feature { get; set; }
    }
}
