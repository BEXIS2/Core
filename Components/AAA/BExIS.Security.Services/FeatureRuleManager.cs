using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Persistence.Api;
using BExIS.Security.Entities;
using BExIS.Security.Entities.Types;

namespace BExIS.Security.Services
{
    public sealed class FeatureRuleManager
    {
        public FeatureRuleManager()
        {
            this.Repo = this.GetUnitOfWork().GetReadOnlyRepository<FeatureRule>();
        }

        #region Data Readers

        public IReadOnlyRepository<FeatureRule> Repo { get; private set; }

        #endregion

        #region FeatureRule

        // C
        public FeatureRule Create(RuleType ruleType, Subject subject, Feature feature)
        {
            // TODO: SECURITY CHECK

            FeatureRule fr = new FeatureRule()
            {
                RuleType = ruleType,
                Subject = subject,
                Feature = feature
            };

            subject.FeatureRules.Add(fr);
            feature.FeatureRules.Add(fr);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<FeatureRule> repo = uow.GetRepository<FeatureRule>();
                repo.Put(fr);
                uow.Commit();
            }
            return (fr);
        }

        // D
        public bool Delete(FeatureRule entity)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<FeatureRule> repo = uow.GetRepository<FeatureRule>();

                entity = repo.Reload(entity);
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }
        public bool Delete(IEnumerable<FeatureRule> entities)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<FeatureRule> repo = uow.GetRepository<FeatureRule>();

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
        public FeatureRule Update(FeatureRule entity)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = entity.GetUnitOfWork())
            {
                IRepository<FeatureRule> repo = uow.GetRepository<FeatureRule>();
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
