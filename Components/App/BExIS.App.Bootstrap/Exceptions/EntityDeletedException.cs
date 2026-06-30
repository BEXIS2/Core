using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.App.Bootstrap.Exceptions
{
    public class EntityDeletedException : Exception
    {
        public EntityDeletedException(string message) : base(message) { }
    }

}
