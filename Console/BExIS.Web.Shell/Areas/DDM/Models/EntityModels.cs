using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System.Collections.Generic;

namespace BExIS.Modules.DDM.UI.Models
{
    public class EntityInstanceGridRowModel
    {
        public long Id { get; set; }
        public bool IsPublic { get; set; }
        public string Title { get; set; }

        public static EntityInstanceGridRowModel Convert(EntityStoreItem item, bool isPublic)
        {
            return new EntityInstanceGridRowModel()
            {
                Id = item.Id,
                Title = item.Title,
                IsPublic = isPublic
            };
        }
    }

    public class EntityTreeViewItemModel
    {
        public List<EntityTreeViewItemModel> Children { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public long? ParentId { get; set; }

        public static EntityTreeViewItemModel Convert(Entity entity, long? parentId)
        {
            return new EntityTreeViewItemModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Children = new List<EntityTreeViewItemModel>(),
                ParentId = parentId
            };
        }
    }
}