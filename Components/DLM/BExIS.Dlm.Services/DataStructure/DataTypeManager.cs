using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.DataStructure;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using BExIS.Core.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public sealed class DataTypeManager
    {
        public DataTypeManager() 
        {
            //// define aggregate paths
            ////AggregatePaths.Add((Unit u) => u.ConversionsIamTheSource);            
            this.Repo = this.GetUnitOfWork().GetReadOnlyRepository<DataType>();
        }

        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<DataType> Repo { get; private set; }

        #endregion

        #region DataType

        public DataType Create(string name, string description, System.TypeCode systemType)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(Contract.Result<DataType>() != null && Contract.Result<DataType>().Id >= 0);
            
            DataType u = new DataType()
            {
                Name = name,
                Description = description,
                SystemType = systemType.ToString(),
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
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);    
        }

        #endregion

    }
}
