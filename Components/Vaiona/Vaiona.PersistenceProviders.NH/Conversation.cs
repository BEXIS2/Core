using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace Vaiona.PersistenceProviders.NH
{
    public class Conversation
    {
        private static Dictionary<object, List<IUnitOfWork>> attachedUnits = new Dictionary<object, List<IUnitOfWork>>(); // special observer pattern
        private ISessionFactory sessionFactory;
        private Configuration cfg;
        private bool showQueries = false;
        private bool commitTransaction = false;
        private TypeOfUnitOfWork type = TypeOfUnitOfWork.Normal;
        private ISession session = null;
        private IStatelessSession stSession = null;
        private bool statefull = true;

        public Conversation(ISessionFactory sessionFactory, Configuration cfg, TypeOfUnitOfWork type = TypeOfUnitOfWork.Normal, bool commitTransaction = false, bool showQueries = false)
        {
            this.sessionFactory = sessionFactory;
            this.cfg = cfg;
            this.type = type;
            this.commitTransaction = commitTransaction;
            this.showQueries = showQueries;
        }

        public bool IsStatefull()
        {
            return statefull;
        }

        public ISession GetSession()
        {
            if (statefull)
                return session;
            return null;
        }

        public IStatelessSession GetStatelessSession()
        {
            if (!statefull)
                return stSession;
            return null;
        }

        public void Start(IUnitOfWork uow)
        {
            int sessionCode = 0;
            switch (type)
            {
                case TypeOfUnitOfWork.Normal: // add the uow to the observers of the current conversation, so that at closing time, the conversation is disposed with the last uow
                    this.session = getAmbientSession(true);
                    statefull = true;
                    stSession = null;

                    registerUnit(session, uow);
                    if (!AppConfiguration.CacheQueryResults)
                        session.CacheMode = NHibernate.CacheMode.Ignore;
                    else
                        session.CacheMode = NHibernate.CacheMode.Normal;
                    sessionCode = session.GetHashCode();
                    break;

                case TypeOfUnitOfWork.Isolated: // single conversation per uow
                    this.session = createSession();
                    statefull = true;
                    stSession = null;

                    if (!AppConfiguration.CacheQueryResults)
                        session.CacheMode = NHibernate.CacheMode.Ignore;
                    else
                        session.CacheMode = NHibernate.CacheMode.Normal;
                    sessionCode = session.GetHashCode();
                    break;

                case TypeOfUnitOfWork.Bulk: // single conversation per uow
                    this.stSession = createStatelessSession();
                    statefull = false;
                    session = null;

                    sessionCode = stSession.GetHashCode();
                    break;

                default:
                    break;
            }

            if (showQueries)
                Trace.WriteLine("SQL output at:" + DateTime.Now.ToString() + "--> " + "A conversation was opened. ID: " + sessionCode);
        }

        public void Commit(IUnitOfWork uow)
        {
            try
            {
                switch (type)
                {
                    case TypeOfUnitOfWork.Normal: // add the uow to the observers of the current conversation, so that at closing time, the conversation is disposed with the last uow
                        lock (uow) // one commit per uow at a time!
                        {
                            if (!session.IsConnected)
                                session.Reconnect();
                            if (!session.Transaction.IsActive)
                                session.Transaction.Begin(System.Data.IsolationLevel.ReadCommitted);
                            session.Transaction.Commit();
                        }
                        break;

                    case TypeOfUnitOfWork.Isolated: // single conversation per uow
                        lock (uow) // one commit per uow at a time!
                        {
                            if (!session.IsConnected)
                                session.Reconnect();
                            if (!session.Transaction.IsActive)
                                session.Transaction.Begin(System.Data.IsolationLevel.ReadCommitted);
                            session.Transaction.Commit();
                        }
                        break;

                    case TypeOfUnitOfWork.Bulk: // single conversation per uow
                        lock (uow) // one commit per uow at a time!
                        {
                            if (!stSession.IsConnected)
                                stSession = createStatelessSession();
                            if (!stSession.Transaction.IsActive)
                                stSession.Transaction.Begin(System.Data.IsolationLevel.ReadCommitted);
                            stSession.Transaction.Commit();
                        }
                        break;

                    default:
                        break;
                }
            }
            //catch (Exception ex) // check what has happend, and try to commit again.
            //{ }
            finally
            { // no need to reactivate the transactio, it will be done on next commit attempt.
            }
        }

        public void End(IUnitOfWork uow)
        {
            switch (type)
            {
                case TypeOfUnitOfWork.Normal: // add the uow to the observers of the current conversation, so that at closing time, the conversation is disposed with the last uow
                    if (session == null || uow == null)
                        return;
                    if (!attachedUnits.ContainsKey(session) || !attachedUnits[session].Contains(uow))
                        return; // the UoW is not authorized to end the conversation
                    unRegisterUnit(session, uow); // remove the UoW from the list of conversation observers
                    if (!attachedUnits.ContainsKey(session)) // there is no observer, so it's safe to close and collect the session/ resources
                    {
                        //NHibernateCurrentSessionProvider.UnBind(session.SessionFactory);
                        endSession(false);
                    }
                    break;

                case TypeOfUnitOfWork.Isolated: // single conversation per uow
                    if (session == null || uow == null)
                        return;
                    endSession(true);
                    break;

                case TypeOfUnitOfWork.Bulk: // single conversation per uow
                    if (stSession == null || uow == null)
                        return;
                    endStatelessSession();
                    break;

                default:
                    break;
            }
        }

        public void Clear(bool applyChanages = false)
        {
            if (statefull)
            {
                if (applyChanages)
                    session.Flush();
                session.Clear();
            }
        }

        private ISession getAmbientSession(bool openIfNeeded)
        {
            if (AppConfiguration.ConversationIsolationLevel == 1)
                return createSession();
            ISession session = null;
            try
            {
                session = sessionFactory.GetCurrentSession();
                if (session == null && openIfNeeded) // && !AppConfiguration.IsWebContext)
                {
                    session = createSingletonSession(); // it is used in the cases where no HTTP request context is available and session per HTTP request does not work.
                }
                //this flush mode will flush on manual flushes and when transactions are committed.
                if (session != null)
                {
                    session.FlushMode = FlushMode.Commit;
                }
            }
            catch
            { }
            if (session == null)
                throw new Exception("Could not acquire a session to access the data.");
            return (session);
        }

        private ISession createSession()
        {
            var session = sessionFactory.OpenSession(cfg.Interceptor);
            //session.Transaction.Begin(System.Data.IsolationLevel.ReadCommitted);
            return session;
        }

        private static ISession singletonSession = null;

        private ISession createSingletonSession()
        {
            if (singletonSession == null)
            {
                singletonSession = sessionFactory.OpenSession(cfg.Interceptor);
            }
            return singletonSession;
        }

        private IStatelessSession createStatelessSession()
        {
            IStatelessSession session = sessionFactory.OpenStatelessSession(); // No interceptor can be passed!
            return session;
        }

        private void endSession(bool disposeSession = false)
        {
            try
            {
                if (session != null && session.Transaction != null && session.Transaction.IsActive)
                {
                    if (commitTransaction)
                    {
                        try
                        {
                            if (session.IsDirty())
                                session.Transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            //session.Transaction.Rollback();
                            throw new Exception("There were some changes submitted to the system that could not be committed!", ex);
                        }
                    }
                    else
                    {
                        session.Transaction.Rollback();
                    }
                }
            }
            catch { } // do nothing
            finally
            {
                if (disposeSession)
                {
                    if (session.IsOpen)
                        session.Close();
                    if (showQueries) // do this befoire disposing the session and setting it to null
                        Trace.WriteLine("SQL output at:" + DateTime.Now.ToString() + "--> " + "A conversation was closed. ID: " + session.GetHashCode());
                    session.Dispose();
                    session = null;
                    //GC.Collect();
                }
            }
        }

        private void endStatelessSession()
        {
            try
            {
                if (stSession != null && stSession.Transaction != null && stSession.Transaction.IsActive)
                {
                    if (commitTransaction)
                    {
                        try
                        {
                            if (stSession.Transaction.IsActive)
                                stSession.Transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            //stSession.Transaction.Rollback();
                            throw new Exception("There were some changes submitted to the system that could not be committed!", ex);
                        }
                    }
                    else
                    {
                        stSession.Transaction.Rollback();
                    }
                }
            }
            finally
            {
                if (stSession.IsOpen)
                    stSession.Close();
                if (showQueries)
                    Trace.WriteLine("SQL output at:" + DateTime.Now.ToString() + "--> " + "A conversation was closed. ID: " + stSession.GetHashCode());
                stSession.Dispose();
                stSession = null;
                GC.Collect();
            }
        }

        private void registerUnit(object session, IUnitOfWork uow)
        {
            //if(!attachedUnits.ContainsKey(session))
            //{
            //    attachedUnits.Add(session, new List<IUnitOfWork> { uow });
            //}
            //else if(!attachedUnits[session].Contains(uow))
            //{
            //    attachedUnits[session].Add(uow);
            //}
        }

        private void unRegisterUnit(object session, IUnitOfWork uow)
        {
            //if (!attachedUnits.ContainsKey(session))
            //    return;
            //attachedUnits[session].Remove(uow);
            //if (attachedUnits[session].Count() <= 0)
            //    attachedUnits.Remove(session);
        }
    }
}