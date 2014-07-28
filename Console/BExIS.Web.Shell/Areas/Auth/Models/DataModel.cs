using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class DataModel
    {
        public List<EntityItemModel> EntitiesList { get; set; }

        public DataModel()
        {
            PermissionManager permissionManager = new PermissionManager();

            IQueryable<Entity> entites = permissionManager.GetAllEntities();
            EntitiesList = new List<EntityItemModel>();
            entites.ToList().ForEach(e => EntitiesList.Add(EntityItemModel.Convert(e)));
        }
    }

    public class DataSubjectModel
    {
        public long DataId { get; set; }
        public string EntityId { get; set; }

        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        public static DataSubjectModel Convert(long DataId, string EntityId, Subject subject, bool read, bool update, bool delete)
        {
            return new DataSubjectModel()
            {
                DataId = DataId,
                EntityId = EntityId,

                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectType = subject is User ? "User" : "Role",

                Read = read,
                Update = update,
                Delete = delete
            };
        }
    }
}