using System;

namespace Vaiona.IoC
{
    public interface IoCContainer
    {
        object Resolve(Type t);

        void RegisterHeirarchical(Type from, Type to);

        void Register(Type from, Type to);

        void RegisterPerRequest(Type from, Type to);

        void StartRequestLevelContainer();

        void ShutdownRequestLevelContainer();

        T ResolveForRequest<T>();

        object ResolveForRequest(Type t);

        bool IsRegistered(Type t, string name);

        //IEnumerable<object> ResolveAll(Type t);

        //object Resolve<T>();
        T Resolve<T>();

        T Resolve<T>(string name);

        bool IsRegistered<T>(string name);

        //IEnumerable<T> ResolveAll<T>();

        void Teardown(object obj);

        void StartSessionLevelContainer();

        void ShutdownSessionLevelContainer();

        T ResolveForSession<T>();

        object ResolveForSession(Type t);
    }
}