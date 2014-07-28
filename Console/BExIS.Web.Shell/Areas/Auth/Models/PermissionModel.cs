using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using Vaiona.Entities.Common;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class PermissionTypeModel
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class FeaturePermissionModel
    {
        public long FeatureId { get; set; }
        public string FeatureName { get; set; }
        public string FeatureDescription { get; set; }

        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public bool ExistsPermission { get; set; }
        public bool EffectiveRight { get; set; }

        public static FeaturePermissionModel Convert(Feature feature, Subject subject, bool existsPermission, bool effectiveRight)
        {
            return new FeaturePermissionModel()
            {
                FeatureId = feature.Id,
                FeatureName = feature.Name,
                FeatureDescription = feature.Description,

                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectType = subject is User ? "User" : "Role",

                ExistsPermission = existsPermission,
                EffectiveRight = effectiveRight
            };
        }
    }
}