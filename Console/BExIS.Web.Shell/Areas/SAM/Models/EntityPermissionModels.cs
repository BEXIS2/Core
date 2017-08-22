using BExIS.Security.Entities.Subjects;
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
        public List<int> Rights { get; set; }


        public static EntityPermissionGridRowModel Convert(Subject subject)
        {
            return new EntityPermissionGridRowModel()
            {
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectType = subject is User ? "User" : "Group",
            };
        }
    }
}