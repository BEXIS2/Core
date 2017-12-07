using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System.Collections.Generic;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class DatasetPermissionGridRowModel
    {
        public long DataId { get; set; }

        public long EntityId { get; set; }

        public List<int> Rights { get; set; }
        public long SubjectId { get; set; }

        public string SubjectName { get; set; }

        public string SubjectType { get; set; }

        public static DatasetPermissionGridRowModel Convert(long dataId, Entity entity, Subject subject, List<int> rights)
        {
            return new DatasetPermissionGridRowModel()
            {
                DataId = dataId,
                EntityId = entity.Id,

                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectType = subject is User ? "User" : "Group",

                Rights = rights
            };
        }
    }
}