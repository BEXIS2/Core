﻿using BExIS.Security.Entities.Subjects;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Models
{
    public class CreateGroupModel
    {
        public string Description { get; set; }

        [Required]
        [Remote("ValidateGroupname", "Groups")]
        public string Name { get; set; }

        public int Type { get; set; }
    }

    public class DeleteGroupModel
    {
    }

    public class GroupFeaturePermissionGridRowModel
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public bool HasFeaturePermission { get; set; }
        public long Id { get; set; }

        public static GroupFeaturePermissionGridRowModel Convert(Group group)
        {
            return new GroupFeaturePermissionGridRowModel()
            {
                Description = group.Description,
                Name = group.Name,
                Id = group.Id,
            };
        }
    }

    public class GroupGridRowModel
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public long Id { get; set; }

        public static GroupGridRowModel Convert(Group group)
        {
            return new GroupGridRowModel()
            {
                Description = group.Description,
                Name = group.Name,
                Id = group.Id
            };
        }
    }

    public class GroupMembershipGridRowModel
    {
        public string Description { get; set; }
        public long Id { get; set; }
        public bool IsUserInGroup { get; set; }
        public string Name { get; set; }

        public static GroupMembershipGridRowModel Convert(Group group, long userId)
        {
            return new GroupMembershipGridRowModel()
            {
                Description = group.Description,
                Name = group.Name,
                Id = group.Id,
                IsUserInGroup = group.Users.Any(u => u.Id == userId)
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

        [Required]
        [Remote("ValidateGroupname", "Groups", AdditionalFields = "Id")]
        public string Name { get; set; }

        public static UpdateGroupModel Convert(Group group)
        {
            return new UpdateGroupModel()
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description
            };
        }
    }
}