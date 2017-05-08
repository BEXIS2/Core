using BExIS.Security.Entities.Subjects;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace BExIS.Modules.Sam.UI.Models
{
    public static class GroupExtensions
    {
        public static IQueryable<GroupGridRowModel> ToGroupGridRowModel(this IQueryable<Group> source)
        {
            Expression<Func<Group, GroupGridRowModel>> conversion = g =>
                new GroupGridRowModel()
                {
                    Description = g.Description,
                    GroupName = g.Name,
                    Id = g.Id
                };

            return source.Select(conversion);
        }
    }

    public class CreateGroupModel
    {
        public string Description { get; set; }

        [Required]
        public string GroupName { get; set; }
    }

    public class DeleteGroupModel
    {
    }

    public class GroupGridRowModel
    {
        public string Description { get; set; }
        public string GroupName { get; set; }
        public long Id { get; set; }

        public static GroupGridRowModel Convert(Group group)
        {
            return new GroupGridRowModel()
            {
                Description = group.Description,
                GroupName = group.Name,
                Id = group.Id
            };
        }
    }
}