using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vaiona.Persistence.Api
{
    public interface ISessionProvider
    {
        object getSession();
    }
}
