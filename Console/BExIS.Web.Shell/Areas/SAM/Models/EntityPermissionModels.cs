using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Utils.Extensions;
using System.Collections.Generic;

namespace BExIS.Modules.Sam.UI.Models
{
    public static class EntityPermissionExtensions
    {
    }

    public class CreateEntityPermissionModel
    {
    }

    public class EntityPermissionGridRowModel
    {
        public long Id { get; set; }
        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }
        public bool[] Rights { get; set; }

        public static EntityPermissionGridRowModel Convert(Subject subject, List<RightType> rights)
        {
            return new EntityPermissionGridRowModel()
            {
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectType = subject is User ? "User" : "Group",
                Rights = rights.ToBoolArray()
            };
        }
    }
}