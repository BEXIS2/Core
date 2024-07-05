using BExIS.Dlm.Entities.DataStructure;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public class AggregateFunctionManager
    {
        private IUnitOfWork guow = null;

        public AggregateFunctionManager()
        {
            //// define aggregate paths
            ////AggregatePaths.Add((Unit u) => u.ConversionsIamTheSource);
        }

        #region AggregateFunction

        public AggregateFunction Create(string name, string description)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(Contract.Result<AggregateFunction>() != null && Contract.Result<AggregateFunction>().Id >= 0);

            AggregateFunction u = new AggregateFunction()
            {
                Name = name,
                Description = description,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<AggregateFunction> repo = uow.GetRepository<AggregateFunction>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
        }

        public bool Delete(AggregateFunction entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<AggregateFunction> repo = uow.GetRepository<AggregateFunction>();

                entity = repo.Reload(entity);
                //relation to DataContainer is managed by the other end
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<AggregateFunction> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (AggregateFunction e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (AggregateFunction e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<AggregateFunction> repo = uow.GetRepository<AggregateFunction>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    //relation to DataContainer is managed by the other end
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public AggregateFunction Update(AggregateFunction entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<AggregateFunction>() != null && Contract.Result<AggregateFunction>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = entity.GetUnitOfWork())
            {
                IRepository<AggregateFunction> repo = uow.GetRepository<AggregateFunction>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        #endregion AggregateFunction
    }
}