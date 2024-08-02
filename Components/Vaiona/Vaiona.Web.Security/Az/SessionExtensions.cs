using System.Runtime.Caching;

namespace System.Web.SessionState
{
    public static class SessionExtensions
    {
        public static MemoryCache GetActionCache(this HttpSessionStateBase session)
        {
            MemoryCache actionCache = null;
            actionCache = session["ActionCache"] as MemoryCache;
            if (actionCache == null)
            {
                actionCache = new MemoryCache("ActionCache");
                session["ActionCache"] = actionCache;
            }
            return (actionCache);
        }

        public static void ClearActionCache(this HttpSessionStateBase session)
        {
            session["ActionCache"] = null;
        }
    }
}