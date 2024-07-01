using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Entities.Security;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc;

namespace Vaiona.Web.Security.Management
{
    public class FeatureManager
    {
        private IPersistenceManager persistenceManager = PersistenceFactory.GetPersistenceManager();

        public int GetRuleCount()
        {
            int count = 0;
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                count = repo.Query().Count();
            }
            return (count);
        }

        public List<AccessRuleEntity> GetRoots()
        {
            List<AccessRuleEntity> nodes = null;
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                nodes = repo.Get(p => p.Parent == null).ToList();
            }
            return (nodes);
        }

        public AccessRuleEntity GetRule(Int64 id)
        {
            AccessRuleEntity rule = null;
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                rule = repo.Get(id);
            }
            return (rule);
        }

        public List<AccessRuleEntity> GetRulesHavingRole(string role)
        {
            List<AccessRuleEntity> rules = null;
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                rules = repo.Get(p => p.RuleBody.Contains(role)).ToList();
            }
            return (rules);
        }

        public void UpdateRule(AccessRuleEntity rule)
        {
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                repo.Put(rule);
                uow.Commit();
            }
        }

        public void UpdateRules(List<AccessRuleEntity> rules)
        {
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                repo.Put(rules);
                uow.Commit();
            }
        }

        public List<AccessRuleEntity> GetSecurityTree()
        {
            List<AccessRuleEntity> nodes = null;
            List<AccessRuleEntity> roots = null;
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                nodes = repo.Get().ToList();
                roots = nodes.Where(p => p.Parent == null).ToList();
                if (roots == null)
                    roots = new List<AccessRuleEntity>();
            }
            return (roots);
        }

        public List<AccessRuleEntity> GetSecuritySubTree(Int64 parentId)
        {
            List<AccessRuleEntity> nodes = null;
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                //where item.Parent.Id == parentId || (parentId == null && item.Parent == null)
                nodes = repo.Query(item => item.Parent.Id == parentId).ToList();
                if (nodes == null)
                    nodes = new List<AccessRuleEntity>();
            }
            return (nodes);
        }

        /// <summary>
        /// Get the rule associated with the provided Id plus all its parents to the root.
        /// Order them in a way that the root is the first in the list
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public List<AccessRuleEntity> GetSecuritySuperTree(Int64 ruleId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("ruleId", ruleId);

            List<AccessRuleEntity> rules = null;
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                rules = repo.Get("GetRuleWithParents", parameters).ToList();
                if (rules == null)
                    rules = new List<AccessRuleEntity>();
            }
            return (rules);
        }

        public List<AccessRuleEntity> GetSecuritySubTree(Int64? parentId)
        {
            List<AccessRuleEntity> nodes = null;
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();
                //where item.Parent.Id == parentId || (parentId == null && item.Parent == null)
                nodes = repo.Query(item => item.Parent.Id == parentId || (parentId == null && item.Parent == null)).ToList();
                if (nodes == null)
                    nodes = new List<AccessRuleEntity>();
            }
            return (nodes);
        }

        public void ExportSecuirtyFeaturesToDatabase()
        {
            foreach (var area in ModuleHelper.Areas)
            {
                // complete feature key + display name
                // add the area to the DB
                AccessRuleEntity areaEntity = addFeatureToDB(area.AreaName, area.AreaName, null);
                foreach (var controller in ModuleHelper.GetControllersByArea(area))
                {
                    // add area.controller to the DB
                    string controllerName = controller.Name.Replace("Controller", "");
                    string controllerKey = string.Format("{0}.{1}", area.AreaName, controllerName);
                    AccessRuleEntity controllerEntity = addFeatureToDB(controllerKey, controllerName, areaEntity);
                    var securedActions = ModuleHelper.GetActionsByController(controller).Where(p => p.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Count() <= 0).ToList();
                    var openActions = ModuleHelper.GetActionsByController(controller).Where(p => p.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Count() > 0).ToList();
                    foreach (var action in securedActions)
                    {
                        // add the area.controller.action to the DB
                        AccessRuleEntity actionEntity = addFeatureToDB(string.Format("{0}.{1}", controllerKey, action.Name), action.Name, controllerEntity);
                    }
                }
            }
        }

        private AccessRuleEntity addFeatureToDB(string featureKey, string displayName, AccessRuleEntity parent)
        {
            //IUnitOfWork uow = persistenceManager.CreateUnitOfWork(false, true, false);
            using (IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true))
            {
                IRepository<AccessRuleEntity> repo = uow.GetRepository<AccessRuleEntity>();

                AccessRuleEntity rule = repo.Query(p => p.SecurityKey.Equals(featureKey)).FirstOrDefault();
                if (rule == null)
                {
                    rule = new AccessRuleEntity()
                    {
                        SecurityKey = featureKey,
                        DisplayName = displayName,
                        SecurityObjectType = SecurityObjectType.Feature,
                        RuleBody = string.Empty,
                        Parent = parent,
                    };
                    repo.Put(rule);
                    uow.Commit();
                }
                return (rule);
            }
        }
    }
}