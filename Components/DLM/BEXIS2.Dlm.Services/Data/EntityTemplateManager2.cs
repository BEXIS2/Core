using BEXIS2.DB.Persistence.Api;
using BEXIS2.Dlm.Entities.Data;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Transactions;

namespace BEXIS2.Dlm.Services.Data
{
    public class EntityTemplate2Manager
    {
        private readonly ISession _session;

        public EntityTemplate2Manager(ISession session)
        {
            _session = session;
        }

        public List<EntityTemplate> GetAllAsync()
        {
            return _session.Query<EntityTemplate>().ToList();
        }

        public EntityTemplate Get(long id)
        {
            return _session.Query<EntityTemplate>().FirstOrDefault(et => et.Id == id);
        }

        public EntityTemplate Create(EntityTemplate entityTemplate)
        {
            if (entityTemplate == null) throw new ArgumentNullException("Entity template must not be null.");
            if (entityTemplate.EntityType == null) throw new ArgumentNullException("Entity type must not be null.");
            if (entityTemplate.MetadataStructure == null) throw new ArgumentNullException("MetadataStructure must not be null.");

            // Schreibvorgänge benötigen eine Transaktion
            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    var etid = _session.Save(entityTemplate);
                   
                    transaction.Commit(); // Speichert die Änderung in der DB
                    return _session.Get<EntityTemplate>(etid);
                }
                catch (Exception ex)
                {
                    throw new Exception("EntityTemplate creation failed.", ex);
                }
            }
        }

        public EntityTemplate Update(EntityTemplate entityTemplate)
        {
            if (entityTemplate == null) throw new ArgumentNullException("Entity template must not be null.");

            Contract.Ensures(Contract.Result<EntityTemplate>() != null && Contract.Result<EntityTemplate>().Id >= 0);

            using (var transaction = _session.BeginTransaction())
            {

                try
                {
                    _session.Update(entityTemplate);
                    transaction.Commit(); // Speichert die Änderung in der DB

                    return (entityTemplate);
                }
                catch (Exception ex)
                {
                    throw new Exception("EntityTemplate creation failed.", ex);
                }
            }
        }

        public bool Delete(long id)
        {
            if (id == 0) throw new ArgumentException("Entity template must not be null.");

            Contract.Ensures(Contract.Result<EntityTemplate>() != null && Contract.Result<EntityTemplate>().Id >= 0);

            using (var transaction = _session.BeginTransaction())
            {

                var e =  _session.Get<EntityTemplate>(id);
                if (e != null)
                {
                    _session.Delete(e);
                    transaction.Commit();

                    return true;
                }
                else
                {
                    throw new ArgumentException(string.Format("the entity template with the id {0} does not exist", id));
                }
            }
        }

        public bool Delete(EntityTemplate entityTemplate)
        {
            if (entityTemplate == null) throw new ArgumentNullException("Entity template must not be null.");

            Contract.Ensures(Contract.Result<EntityTemplate>() != null && Contract.Result<EntityTemplate>().Id >= 0);

            using (var transaction = _session.BeginTransaction())
            {
                _session.Delete(entityTemplate);
                transaction.Commit();

                return true;
            }
        }
    }
}