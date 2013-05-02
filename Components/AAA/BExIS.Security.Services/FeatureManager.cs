using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities;
using BExIS.Core.Persistence.Api;
using BExIS.Security.Entities.Info;

namespace BExIS.Security.Services
{
    public sealed class FeatureManager
    {
        public FeatureManager()
        {
            this.Repo = this.GetUnitOfWork().GetReadOnlyRepository<Feature>();
        }

        #region Data Readers

        public IReadOnlyRepository<Feature> Repo { get; private set; }

        #endregion

        #region Feature

        // C
        public Feature Create(string name, ActionInfo actionInfo, Feature parent)
        {
            // TODO: SECURITY CHECK

            Feature f = new Feature()
            {
                Name = name,
                LowerCaseName = name.ToLower(),
                ActionInfo = actionInfo,
                Parent = parent
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> repo = uow.GetRepository<Feature>();
                repo.Put(f);
                uow.Commit();
            }
            return (f);
        }

        // D
        public bool Delete(Feature entity)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> repo = uow.GetRepository<Feature>();

                entity = repo.Reload(entity);
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }
        public bool Delete(IEnumerable<Feature> entities)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> repo = uow.GetRepository<Feature>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        // U
        public Feature Update(Feature entity)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = entity.GetUnitOfWork())
            {
                IRepository<Feature> repo = uow.GetRepository<Feature>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region Associations

        #endregion
    }
}
