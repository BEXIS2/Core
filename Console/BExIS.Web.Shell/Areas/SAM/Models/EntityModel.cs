using System.Collections.Generic;
using System.Linq;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;

namespace BExIS.Modules.Sam.UI.Models
{
    public class EntityItemModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public static EntityItemModel Convert(Entity entity)
        {
            return new EntityItemModel()
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }
    }

    public class EntitySelectListItemModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public static EntitySelectListItemModel Convert(Entity entity)
        {
            return new EntitySelectListItemModel()
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }
    }

    public class EntitySelectListModel
    {
        public long Id { get; set; }

        public List<EntitySelectListItemModel> EntityList { get; set; }

        public EntitySelectListModel()
        {
            EntityManager entityManager = new EntityManager();

            EntityList = entityManager.GetAllEntities().Select(e => EntitySelectListItemModel.Convert(e)).ToList<EntitySelectListItemModel>();
        }
    }
}