using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class GroupCreateModel
    {
        [Display(Name = "Group Name")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The group name is invalid.")]
        [Remote("ValidateGroupName", "Groups")]
        [Required]
        [StringLength(50, ErrorMessage = "The group name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string GroupName { get; set; }

        [Display(Name = "Description")]
        [RegularExpression("^[^\\s]+(\\s+[^\\s]+)*", ErrorMessage = "The description must start and end with no space.")]
        [StringLength(250, ErrorMessage = "The description must be less than {1} characters long.")]
        public string Description { get; set; }
    }

    public class GroupUpdateModel
    {
        public long Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        public static GroupUpdateModel Convert(Group group)
        {
            return new GroupUpdateModel()
            {
                Id = group.Id,
                GroupName = group.Name,
                Description = group.Description
            };

        }
    }

    public class GroupReadModel
    {
        public long Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        public static GroupReadModel Convert(Group group)
        {
            return new GroupReadModel()
            {
                Id = group.Id,
                GroupName = group.Name,
                Description = group.Description
            };

        }
    }

    public class GroupDeleteModel
    {
        public long Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        public static GroupDeleteModel Convert(Group group)
        {
            return new GroupDeleteModel()
            {
                Id = group.Id,
                GroupName = group.Name,
                Description = group.Description
            };
        }
    }

    public class GroupGridRowModel
    {
        [Display(Name = "Group ID")]
        [Editable(false)]
        [Required]
        public long Id { get; set; }

        [Display(Name = "Group Name")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The group name is invalid.")]
        [Remote("ValidateGroupName", "Groups", AdditionalFields = "Id")]
        [Required]
        [StringLength(50, ErrorMessage = "The group name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string GroupName { get; set; }

        [Display(Name = "Description")]
        [StringLength(250, ErrorMessage = "The description must be less than {1} characters long.")]
        public string Description { get; set; }

        public static GroupGridRowModel Convert(Group group)
        {
            return new GroupGridRowModel()
            {
                Id = group.Id,
                GroupName = group.Name,
                Description = group.Description
            };
        }
    }

    public class GroupMembershipGridRowModel
    {
        public long GroupId { get; set; }

        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public bool IsUserInGroup { get; set; }

        public static GroupMembershipGridRowModel Convert(long groupId, User user, bool isUserInGroup)
        {
            return new GroupMembershipGridRowModel()
            {
                GroupId = groupId,

                Id = user.Id,
                UserName = user.Name,
                Email = user.Email,

                IsUserInGroup = isUserInGroup
            };
        }
    }

    public class GroupSelectListItemModel
    {

    }

    public class GroupSelectListModel
    {

    }
}