using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class DataPermissionGridRowModel
    {
        public long DataId { get; set; }
        public long EntityId { get; set; }

        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public List<int> Rights { get; set; }

        public static DataPermissionGridRowModel Convert(long dataId, Entity entity, Subject subject, List<int> rights)
        {
            return new DataPermissionGridRowModel()
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