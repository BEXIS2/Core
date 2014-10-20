using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Security;

namespace BExIS.Web.Shell.Areas.Auth.Models
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
}