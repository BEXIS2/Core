using BExIS.Dlm.Entities.Administration;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;
using DataStructureEntity = BExIS.Dlm.Entities.DataStructure.DataStructure;

namespace BExIS.Dlm.Services.Administration
{
    public class ResearchPlanManager : IDisposable
    {
        private IUnitOfWork guow = null;

        public ResearchPlanManager() //: base(false, true, true)
        {
            guow = this.GetIsolatedUnitOfWork(); // Javad commented this line. bring it back with the new Data Access Pattern
            this.Repo = guow.GetReadOnlyRepository<ResearchPlan>();
        }

        private bool isDisposed = false;

        ~ResearchPlanManager()
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

        public IReadOnlyRepository<ResearchPlan> Repo { get; private set; }

        #endregion Data Readers

        #region ResearchPlan

        public ResearchPlan Create(string title, string description)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));

            Contract.Ensures(Contract.Result<ResearchPlan>() != null && Contract.Result<ResearchPlan>().Id >= 0);

            ResearchPlan e = new ResearchPlan()
            {
                Title = title,
                Description = description,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ResearchPlan> repo = uow.GetRepository<ResearchPlan>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool Delete(ResearchPlan entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ResearchPlan> repo = uow.GetRepository<ResearchPlan>();

                entity = repo.Reload(entity);

                // delete all links to other entities
                entity.Datasets.ToList().ForEach(a => a.ResearchPlan = null);
                // data structures have n-m relationship via a coupling table. deleting the research plan will delete entries in that table but not the data structures.
                //Data structures, metadata structures, execution units, etc

                //delete the entity
                repo.Delete(entity);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<ResearchPlan> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (ResearchPlan e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (ResearchPlan e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ResearchPlan> repo = uow.GetRepository<ResearchPlan>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);

                    // remove all associations between current unit and the conversions
                    latest.Datasets.ToList().ForEach(a => a.ResearchPlan = null);

                    //delete the entity
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        public ResearchPlan Update(ResearchPlan entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<ResearchPlan>() != null && Contract.Result<ResearchPlan>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ResearchPlan> repo = uow.GetRepository<ResearchPlan>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        #endregion ResearchPlan

        #region Associations

        public bool AddDataStructure(ResearchPlan end1, DataStructureEntity end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(end2 != null && end2.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ResearchPlan> repo = uow.GetRepository<ResearchPlan>();

                end1 = repo.Reload(end1);
                repo.LoadIfNot(end1.DataStructures);
                if (!end1.DataStructures.Contains(end2))
                {
                    end1.DataStructures.Add(end2);
                    end2.ResearchPlans.Add(end1);
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        public bool RemoveDataStructure(ResearchPlan end1, DataStructureEntity end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(end2 != null && end2.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ResearchPlan> end1Repo = uow.GetRepository<ResearchPlan>();
                IRepository<DataStructureEntity> end2Repo = uow.GetRepository<DataStructureEntity>();

                end1 = end1Repo.Reload(end1);
                end1Repo.LoadIfNot(end1.DataStructures);

                end2 = end2Repo.Reload(end2);
                end2Repo.LoadIfNot(end2.ResearchPlans);

                if (end1.DataStructures.Contains(end2) || end2.ResearchPlans.Contains(end1))
                {
                    end1.DataStructures.Remove(end2);
                    end2.ResearchPlans.Remove(end1);
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        //public bool AddMetadataStructure(ResearchPlan end1, MetadataStructure end2)
        //{
        //    Contract.Requires(end1 != null && end1.Id >= 0);
        //    Contract.Requires(end2 != null && end2.Id >= 0);

        //    return false;
        //}

        //public bool RemoveMetadataStructure(ResearchPlan end1, MetadataStructure end2)
        //{
        //    Contract.Requires(end1 != null && end1.Id >= 0);
        //    Contract.Requires(end2 != null && end2.Id >= 0);
        //    return false;
        //}

        #endregion Associations
    }
}