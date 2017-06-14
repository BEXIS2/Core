using BExIS.Security.Entities.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Sam.UI.Models
{
    public class CreateEntityModel
    {
        public string AssemblyPath { get; set; }
        public string ClassPath { get; set; }
        public string Name { get; set; }
    }

    public class DeleteEntityModel
    {
    }

    public class EntityGridRowModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class EntityTreeViewItemModel
    {
        public List<EntityTreeViewItemModel> Children { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }

        public static EntityTreeViewItemModel Convert(Entity entity)
        {
            return new EntityTreeViewItemModel()
            {
                Id = entity.Id,
                Name = entity.EntityType.Name,
                Children = entity.Children.Select(Convert).ToList()
            };
        }
    }

    public class ReadEntityModel
    {
        public string AssemblyPath { get; set; }
        public string ClassPath { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class UpdateEntityModel
    {
    }
}