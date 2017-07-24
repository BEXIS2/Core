using BExIS.Security.Entities.Subjects;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Sam.UI.Models
{
    public class CreateGroupModel
    {
        public string Description { get; set; }

        [Required]
        public string GroupName { get; set; }

        [Required]
        public int GroupType { get; set; }
    }

    public class DeleteGroupModel
    {
    }

    public class GroupFeaturePermissionGridRowModel
    {
        public string Description { get; set; }
        public string GroupName { get; set; }
        public GroupType GroupType { get; set; }
        public bool HasFeaturePermission { get; set; }
        public long Id { get; set; }

        public static GroupFeaturePermissionGridRowModel Convert(Group group)
        {
            return new GroupFeaturePermissionGridRowModel()
            {
                Description = group.Description,
                GroupType = group.GroupType,
                GroupName = group.Name,
                Id = group.Id,
            };
        }
    }

    public class GroupGridRowModel
    {
        public string Description { get; set; }
        public string GroupName { get; set; }
        public GroupType GroupType { get; set; }
        public long Id { get; set; }

        public static GroupGridRowModel Convert(Group group)
        {
            return new GroupGridRowModel()
            {
                Description = group.Description,
                GroupName = group.Name,
                GroupType = group.GroupType,
                Id = group.Id
            };
        }
    }

    public class GroupMembershipGridRowModel
    {
        public string Description { get; set; }
        public GroupType GroupType { get; set; }
        public long Id { get; set; }
        public bool IsUserInGroup { get; set; }
        public string Name { get; set; }

        public static GroupMembershipGridRowModel Convert(Group group, List<long> test)
        {
            return new GroupMembershipGridRowModel()
            {
                Description = group.Description,
                Name = group.Name,
                GroupType = group.GroupType,
                Id = group.Id,
                IsUserInGroup = test.Contains(group.Id)
            };
        }
    }

    public class ReadGroupModel
    {
    }

    public class UpdateGroupModel
    {
        public string Description { get; set; }
        public int GroupType { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }

        public static UpdateGroupModel Convert(Group group)
        {
            return new UpdateGroupModel()
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                GroupType = (int)group.GroupType
            };
        }
    }
}