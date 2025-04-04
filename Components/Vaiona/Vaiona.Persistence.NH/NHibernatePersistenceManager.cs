using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Vaiona.Persistence.Api;

namespace Vaiona.Persistence.NH
{
    public class NHibernatePersistenceManager : IPersistenceManager
    {
        private static ISessionFactory sessionFactory;
        private static Configuration cfg;
        private static string configFile = "";
        private static List<string> mappingFolders = new List<string>();

        public object Factory => throw new NotImplementedException();

        public IUnitOfWorkFactory UnitOfWorkFactory => throw new NotImplementedException();

        public int PreferredPushSize => throw new NotImplementedException();

        public void Configure(string configFilePath, List<string> mappingFolders, string databaseDilect, string connectionString = "", bool useNeutralMapping = false)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(databaseDilect));
            Contract.Requires(!string.IsNullOrWhiteSpace(configFile));
            Contract.Requires(mappingFolders != null && mappingFolders.Count() > 0);

            if (sessionFactory != null)
                return;

            mappingFolders.ForEach(p => p = p.TrimEnd(@"\".ToCharArray()));
            NHibernatePersistenceManager.mappingFolders = mappingFolders;

            configFile = string.Format(@"{0}\{1}.hibernate.cfg.xml", configFilePath.TrimEnd(@"\".ToCharArray()), databaseDilect);
            cfg = new Configuration();
            cfg.Configure(configFile);

            // in case of having specific queries or mappings for different dialects, it is better (and possible)
            // to develop different maaping files and externalizing queries
            foreach (var mappingFolder in mappingFolders)
            {
                string folder = mappingFolder.TrimEnd(@"\".ToCharArray());
                folder = string.Format(@"{0}\Mappings\{1}", folder, useNeutralMapping ? "Default" : databaseDilect);
                cfg.AddDirectory(new DirectoryInfo(folder));
            }
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
                //cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, Util.Decrypt(connectionString));
            }

            Contract.Ensures(cfg != null);
        }

        public void ExportSchema(bool generateScript = false, bool executeAgainstTargetDB = true, bool justDrop = false)
        {
            new SchemaExport(cfg).Execute(generateScript, executeAgainstTargetDB, justDrop);
            foreach (var mappingFolder in mappingFolders)
            {
                string postInstallationScript = string.Format(@"{0}\postInstallationScript.sql", mappingFolder);
                if (File.Exists(postInstallationScript))
                {
                    executePostInstallationScript(postInstallationScript);
                }
            }
        }

        private void executePostInstallationScript(string postInstallationScript)
        {
            string sql;

            using (FileStream strm = File.OpenRead(postInstallationScript))
            {
                var reader = new StreamReader(strm);
                sql = reader.ReadToEnd();
            }

            var regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(sql);

            this.Start();
            ISession session = sessionFactory.OpenSession();
            session.BeginTransaction();
            try
            {
                foreach (string line in lines)
                {
                    IQuery query = session.CreateSQLQuery(line);
                    query.ExecuteUpdate();
                }
                session.GetCurrentTransaction().Commit();
            }
            catch
            {
                session.GetCurrentTransaction().Rollback();
            }
            finally
            {
                this.Shutdown();
            }
        }

        public void Start()
        {
            // may need locking for concurrent calls!
            if (sessionFactory != null)
                return;
            sessionFactory = cfg.BuildSessionFactory();

            Contract.Ensures(sessionFactory != null);
        }

        public void Shutdown()
        {
            EndContext();
            // may need locking for concurrent calls!
            if (sessionFactory != null)
            {
                sessionFactory.Close();
            }
        }

        public IUnitOfWork CreateUnitOfWork(bool autoCommit = false, bool throwExceptionOnError = true, bool allowMultipleCommit = false
            , EventHandler beforeCommit = null, EventHandler afterCommit = null, EventHandler beforeIgnore = null, EventHandler afterIgnore = null)
        {
            ISession session = getSession();
            NHibernateUnitOfWork u = new NHibernateUnitOfWork(this, session, autoCommit, throwExceptionOnError, allowMultipleCommit);
            u.BeforeCommit += beforeCommit;
            u.AfterCommit += afterCommit;
            u.BeforeIgnore += beforeIgnore;
            u.AfterIgnore += afterIgnore;
            return (u);
        }

        private ISession getSession()
        {
            ISession session = null;
            try
            {
                session = sessionFactory.GetCurrentSession();
            }
            catch
            { }

            if (session == null)
            {   //strat a new session
                StartConversation();
            }
            return (session);
        }

        public object GetCurrentConversation()
        {
            return (getSession());
        }

        public void StartConversation()
        {
            foreach (var sessionFactory in getSessionFactories())
            {
                var localFactory = sessionFactory;

                NHibernateSessionProvider.Bind(new Lazy<ISession>(() => beginSession(localFactory)), sessionFactory);
            }
        }

        public void EndConversation()
        {
            foreach (var sessionfactory in getSessionFactories())
            {
                var session = NHibernateSessionProvider.UnBind(sessionfactory);
                if (session == null) continue;
                endSession(session);
            }
        }

        /// <summary>
        /// Retrieves all ISessionFactory instances via IoC
        /// </summary>
        private IEnumerable<ISessionFactory> getSessionFactories()
        {
            var sessionFactories = new List<ISessionFactory>() { sessionFactory };

            if (sessionFactories == null || !sessionFactories.Any())
                throw new TypeLoadException("There should be at least one ISessionFactory registered");
            return sessionFactories;
        }

        private static ISession beginSession(ISessionFactory sessionFactory)
        {
            var session = sessionFactory.OpenSession();
            session.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            return session;
        }

        private static void endSession(ISession session, bool commitTransaction = false)
        {
            try
            {
                if (session.Transaction != null && session.Transaction.IsActive)
                {
                    if (commitTransaction)
                    {
                        try
                        {
                            session.Transaction.Commit();
                        }
                        catch
                        {
                            session.Transaction.Rollback();
                            throw;
                        }
                    }
                    else
                    {
                        session.Transaction.Rollback();
                    }
                }
            }
            finally
            {
                if (session.IsOpen)
                    session.Close();

                session.Dispose();
            }
        }

        public void EndContext()
        {
            foreach (var sessionfactory in getSessionFactories())
            {
                var session = NHibernateSessionProvider.UnBind(sessionfactory);
                if (session == null) continue;
                endSession(session, false);
            }
        }

        public void Configure(string connectionString = "", string databaseDilect = "DB2Dialect", string fallbackFoler = "Default", bool showQueries = false, bool configureModules = false)
        {
            throw new NotImplementedException();
        }

        public void UpdateSchema(bool generateScript = false, bool executeAgainstTargetDB = true)
        {
            throw new NotImplementedException();
        }

        public string GetProperty(string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}