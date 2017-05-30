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
        public string AssemblyPath { get; set; }
        public string ClassPath { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class EntityTreeViewItemModel
    {
        public string AssemblyPath { get; set; }
        public List<EntityTreeViewItemModel> Children { get; set; }
        public string ClassPath { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }

        public static EntityTreeViewItemModel Convert(Entity entity)
        {
            return new EntityTreeViewItemModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                AssemblyPath = entity.AssemblyPath,
                ClassPath = entity.ClassPath,
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