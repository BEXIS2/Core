using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class DataPermissionCreationModel
    {
        public long SubjectId { get; set; }

        public string EntityName { get; set; }

        public string Property { get; set; }

        public string Value { get; set; }

        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        public List<SubjectItemModel> SubjectsList { get; set; }
        public List<EntityItemModel> EntitiesList { get; set; }

        public DataPermissionCreationModel()
        {
            SubjectManager subjectManager = new SubjectManager();

            IQueryable<Subject> subjects = subjectManager.GetAllSubjects();
            SubjectsList = new List<SubjectItemModel>();
            subjects.ToList().ForEach(s => SubjectsList.Add(SubjectItemModel.Convert(s)));

            PermissionManager permissionManager = new PermissionManager();

            IQueryable<Entity> entites = permissionManager.GetAllEntities();
            EntitiesList = new List<EntityItemModel>();
            entites.ToList().ForEach(e => EntitiesList.Add(EntityItemModel.Convert(e)));
        }
    }

    public class PermissionTypeModel
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class DataPermissionModel
    {
        public long Id { get; set; }

        public string EntityName { get; set; }

        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public string Property { get; set; }
        public Comparator Comparator { get; set; }
        public object Value { get; set; }

        public string Equation { get; set; }

        public static DataPermissionModel Convert(DataPermission dataPermission)
        {
            return new DataPermissionModel()
            {
                EntityName = dataPermission.EntityName,

                SubjectId = dataPermission.Subject.Id,
                SubjectName = dataPermission.Subject.Name,
                SubjectType = dataPermission.Subject is User ? "User" : "Role",

                Property = dataPermission.Property,
                Comparator = dataPermission.Comparator,
                Value = dataPermission.Value,
                Equation = dataPermission.Equation
            };
        }
    }

    public class FeaturePermissionModel
    {
        public long FeatureId { get; set; }
        public string FeatureName { get; set; }
        public string FeatureDescription { get; set; }

        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public int FeaturePermissionType { get; set; }

        public static FeaturePermissionModel Convert(Feature feature, Subject subject, int featurePermissionType)
        {
            return new FeaturePermissionModel()
            {
                FeatureId = feature.Id,
                FeatureName = feature.Name,
                FeatureDescription = feature.Description,

                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectType = subject is User ? "User" : "Role",

                FeaturePermissionType = featurePermissionType
            };
        }
    }
}