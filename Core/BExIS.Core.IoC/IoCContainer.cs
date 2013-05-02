using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Core.IoC
{
    public interface IoCContainer
    {
        object Resolve<T>();
        object Resolve(Type t);
        IoCContainer CreateSessionLevelContainer();
        object ResolveForSession<T>();
        object ResolveForSession(Type t);
    }
}
