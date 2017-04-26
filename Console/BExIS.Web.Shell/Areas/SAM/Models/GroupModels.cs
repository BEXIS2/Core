using BExIS.Security.Entities.Subjects;

namespace BExIS.Modules.Sam.UI.Models
{
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