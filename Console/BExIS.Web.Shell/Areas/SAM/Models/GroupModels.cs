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
        public long Id { get; set; }
        public bool HasFeaturePermission { get; set; }

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
        public string GroupName { get; set; }
        public GroupType GroupType { get; set; }
        public long Id { get; set; }
        public bool IsMember { get; set; }

        public static GroupMembershipGridRowModel Convert(Group group, List<long> memberships)
        {
            return new GroupMembershipGridRowModel()
            {
                Description = group.Description,
                GroupName = group.Name,
                GroupType = group.GroupType,
                Id = group.Id,
                IsMember = memberships.Contains(group.Id)
            };
        }
    }

    public class ReadGroupModel
    {
    }

    public class UpdateGroupModel
    {
        public string Description { get; set; }

        [Required]
        public string GroupName { get; set; }

        [Required]
        public int GroupType { get; set; }
    }
}