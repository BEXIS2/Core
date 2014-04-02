using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class DataPermissionCreationModel
    {
        public string EntityName { get; set; }
    }

    public class DataPermissionModel
    {
        public string EntityName { get; set; }

        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public Statement Statement { get; set; }
    }

    public class Statement
    {

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

    public class PermissionModel
    {
        public StartExpression StartExpression { get; set; }
        public List<Expression> Expressions { get; set; }
    }

    public class Expression
    {
        public string Linker { get; set; }
        public string Property { get; set; }
        public string Comparator { get; set; }
        public string Value { get; set; }
    }

    public class StartExpression
    {
        public string Property { get; set; }
        public string Comparator { get; set; }
        public string Value { get; set; }
    }
}