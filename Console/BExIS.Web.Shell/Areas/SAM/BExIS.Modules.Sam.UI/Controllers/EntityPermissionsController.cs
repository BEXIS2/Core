using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.Utils.NH.Querying;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class EntityPermissionsController : BaseController
    {
        public async Task AddInstanceToPublic(long entityId, long instanceId)
        {
            var entityPermissionManager = new EntityPermissionManager();

            try
            {
                var entityPermission = await entityPermissionManager.FindAsync(entityId, instanceId);

                if (entityPermission == null)
                {
                    await entityPermissionManager.CreateAsync(null, entityId, instanceId, (int)RightType.Read);

                    if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                    {
                        var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", instanceId } });
                    }

                    var es = new EmailService();
                    es.Send(MessageHelper.GetSetPublicHeader(instanceId, typeof(Dataset).Name),
                        MessageHelper.GetSetPublicMessage(getPartyNameOrDefault(), instanceId, typeof(Dataset).Name),
                        ConfigurationManager.AppSettings["SystemEmail"]
                        );
                }
            }
            finally
            {
                entityPermissionManager.Dispose();
            }
        }

        public async Task AddRightToEntityPermission(long subjectId, long entityId, long instanceId, int rightType)
        {
            var entityPermissionManager = new EntityPermissionManager();

            try
            {
                var entityPermission = await entityPermissionManager.FindAsync(subjectId, entityId, instanceId);

                if (entityPermission == null)
                {
                    await entityPermissionManager.CreateAsync(subjectId, entityId, instanceId, rightType);
                }
                else
                {
                    if ((entityPermission.Rights & rightType) != 0) return;
                    entityPermission.Rights += rightType;
                    await entityPermissionManager.UpdateAsync(entityPermission);
                }
            }
            finally
            {
                entityPermissionManager.Dispose();
            }
        }

        public ActionResult Index()
        {
            var entityManager = new EntityManager();

            try
            {
                ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Entity Permissions", Session.GetTenant());

                var entities = entityManager.Entities.Select(e => EntityTreeViewItemModel.Convert(e, e.Parent.Id)).ToList();

                foreach (var entity in entities)
                {
                    entity.Children = entities.Where(e => e.ParentId == entity.Id).ToList();
                }

                return View(entities.AsEnumerable());
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public ActionResult Instances(long entityId)
        {
            return PartialView("_Instances", entityId);
        }

        [GridAction(EnableCustomBinding = false)]
        public async Task<ActionResult> Instances_Select(long entityId)
        {
            var entityManager = new EntityManager();
            var entityPermissionManager = new EntityPermissionManager();

            try
            {
                var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);
                var instances = (await Task.WhenAll(instanceStore.GetEntities().Select(async i => EntityInstanceGridRowModel.Convert(i, await entityPermissionManager.ExistsAsync(entityId, i.Id))))).ToList();

                return View(new GridModel<EntityInstanceGridRowModel> { Data = instances });
            }
            finally
            {
                entityManager.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        public ActionResult Permissions(long subjectId, long entityId, long instanceId)
        {
            return PartialView("_Permissions", new SubjectInstanceModel() { SubjectId = subjectId, EntityId = entityId, InstanceId = instanceId });
        }

        [GridAction]
        public async Task<ActionResult> Permissions_Select(long subjectId, long entityId, long instanceId)
        {
            using (var entityPermissionManager = new EntityPermissionManager())
            using (var subjectManager = new SubjectManager())
            using (var partyManager = new PartyManager())
            using (var entityManager = new EntityManager())
            {
                var subject = subjectManager.SubjectRepository.Get(subjectId);

                var entityPermissions = new List<ReferredEntityPermissionGridRowModel>();

                // User & Group Permissions
                if (subject is User)
                {
                    var user = subject as User;

                    // Group Permissions
                    foreach (var group in user.Groups)
                    {
                        entityPermissions.Add(ReferredEntityPermissionGridRowModel.Convert(group.Name, "Group", await entityPermissionManager.GetRightsAsync(group.Id, entityId, instanceId)));
                    }

                    // Party Relationships
                    var userParty = partyManager.GetPartyByUser(user.Id);

                    if (userParty != null)
                    {
                        var entityParty = partyManager.Parties.FirstOrDefault(m => m.PartyType.Title == entityManager.FindById(entityId).Name && m.Name == instanceId.ToString());

                        if (entityParty != null)
                        {
                            var partyRelationships = partyManager.PartyRelationships.Where(m => m.SourceParty.Id == userParty.Id && m.TargetParty.Id == entityParty.Id);

                            foreach (var partyRelationship in partyRelationships)
                            {
                                entityPermissions.Add(ReferredEntityPermissionGridRowModel.Convert("Party Relationship", partyRelationship.Title, partyRelationship.Permission));
                            }
                        }
                    }
                }

                // Public Permission
                var publicRights = await entityPermissionManager.GetRightsAsync(entityId, instanceId);
                if (publicRights > 0)
                {
                    entityPermissions.Add(ReferredEntityPermissionGridRowModel.Convert("Public Dataset", "", publicRights));
                }

                return View(new GridModel<ReferredEntityPermissionGridRowModel> { Data = entityPermissions });
            }
        }

        public async Task RemoveInstanceFromPublic(long entityId, long instanceId)
        {
            using (var entityPermissionManager = new EntityPermissionManager())
            {
                var entityPermission = await entityPermissionManager.FindAsync(entityId, instanceId);

                if (entityPermission == null) return;
                await entityPermissionManager.DeleteAsync(entityPermission);

                if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                {
                    var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", instanceId } });
                }

                var es = new EmailService();
                es.Send(MessageHelper.GetUnsetPublicHeader(instanceId, typeof(Dataset).Name),
                    MessageHelper.GetUnsetPublicMessage(getPartyNameOrDefault(), instanceId, typeof(Dataset).Name),
                    ConfigurationManager.AppSettings["SystemEmail"]
                    );
            }
        }

        public async Task RemoveRightFromEntityPermission(long subjectId, long entityId, long instanceId, int rightType)
        {
            using (var entityPermissionManager = new EntityPermissionManager())
            {
                var entityPermission = await entityPermissionManager.FindAsync(subjectId, entityId, instanceId);

                if (entityPermission == null) return;

                if (entityPermission.Rights == rightType)
                {
                    await entityPermissionManager.DeleteAsync(entityPermission);
                }
                else
                {
                    if ((entityPermission.Rights & rightType) == 0) return;
                    entityPermission.Rights -= rightType;
                    await entityPermissionManager.UpdateAsync(entityPermission);
                }
            }
        }

        public ActionResult Subjects(long entityId, long instanceId)
        {
            return PartialView("_Subjects", new EntityInstanceModel() { EntityId = entityId, InstanceId = instanceId });
        }

        [GridAction(EnableCustomBinding = true)]
        public async Task<ActionResult> Subjects_Select(GridCommand command, long entityId, long instanceId)
        {
            var subjectManager = new SubjectManager();
            var entityPermissionManager = new EntityPermissionManager();

            try
            {
                var subjectsDb = new List<Subject>();
                var count = 0;
                if (command != null)// filter subjects based on grid filter settings
                {
                    FilterExpression filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                    OrderByExpression orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());

                    subjectsDb = subjectManager.GetSubjects(filter, orderBy, command.Page, command.PageSize, out count);
                }
                else
                {
                    subjectsDb = subjectManager.Subjects.ToList();
                    count = subjectsDb.Count();
                }

                var subjects = new List<EntityPermissionGridRowModel>();
                //using (PartyManager partyManager = new PartyManager())

                //foreach (var subject in subjectsDb)
                //{
                //    var rights = entityPermissionManager.GetRights(subject.Id, entityId, instanceId);
                //    var effectiveRights = entityPermissionManager.GetEffectiveRights(subject.Id, entityId, instanceId);

                //    subjects.Add(EntityPermissionGridRowModel.Convert(subject, rights, effectiveRights));
                //}

                var rightsDic = await entityPermissionManager.GetRightsAsync(subjectsDb.Select(s => s.Id).ToList(), entityId, instanceId);
                var effectiveRightsDic = await entityPermissionManager.GetEffectiveRightsAsync(subjectsDb.Select(s => s.Id).ToList(), entityId, instanceId);

                foreach (var item in rightsDic)
                {
                    var subject = subjectsDb.Where(s => s.Id.Equals(item.Key)).FirstOrDefault();
                    var rights = item.Value;
                    var effectiveRights = effectiveRightsDic[item.Key];

                    subjects.Add(EntityPermissionGridRowModel.Convert(subject, rights, effectiveRights));
                }

                return View(new GridModel<EntityPermissionGridRowModel> { Data = subjects, Total = count });
            }
            finally
            {
                subjectManager.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        private string getPartyNameOrDefault()
        {
            var userName = string.Empty;
            try
            {
                userName = HttpContext.User.Identity.Name;
            }
            catch { }

            if (userName != null)
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var userRepository = uow.GetReadOnlyRepository<User>();
                    var user = userRepository.Query(s => s.Name.ToUpperInvariant() == userName.ToUpperInvariant()).FirstOrDefault();

                    if (user != null)
                    {
                        return user.DisplayName;
                    }
                }
            }
            return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
        }
    }
}