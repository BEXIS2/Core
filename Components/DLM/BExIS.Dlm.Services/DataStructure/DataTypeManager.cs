using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public class DataTypeManager : IDisposable
    {
        private IUnitOfWork guow = null;

        public DataTypeManager()
        {
            //// define aggregate paths
            ////AggregatePaths.Add((Unit u) => u.ConversionsIamTheSource);
            guow = this.GetIsolatedUnitOfWork();
            this.Repo = guow.GetReadOnlyRepository<DataType>();
        }

        private bool isDisposed = false;

        ~DataTypeManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (guow != null)
                        guow.Dispose();
                    isDisposed = true;
                }
            }
        }

        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<DataType> Repo { get; private set; }

        #endregion Data Readers

        #region DataType

        public DataType Create(string name, string description, System.TypeCode systemType)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(Contract.Result<DataType>() != null && Contract.Result<DataType>().Id >= 0);

            DataType u = new DataType()
            {
                Name = name,
                Description = description,
                SystemType = systemType.ToString()
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataType> repo = uow.GetRepository<DataType>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
        }

        public bool Delete(DataType entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataType> repo = uow.GetRepository<DataType>();

                entity = repo.Reload(entity);

                // remove all associations
                entity.ApplicableUnits.ToList().ForEach(u => u.AssociatedDataTypes.Remove(entity));
                entity.ApplicableUnits.Clear();
                entity.DataContainers.Clear();

                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<DataType> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (DataType e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (DataType e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataType> repo = uow.GetRepository<DataType>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);

                    // remove all associations
                    latest.ApplicableUnits.ToList().ForEach(u => u.AssociatedDataTypes.Clear());
                    latest.ApplicableUnits.Clear();
                    latest.DataContainers.Clear();

                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public DataType Update(DataType entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<DataType>() != null && Contract.Result<DataType>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = entity.GetUnitOfWork())
            {
                IRepository<DataType> repo = uow.GetRepository<DataType>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        #endregion DataType
    }
}