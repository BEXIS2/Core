using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Helpers.EntityReference
{
    public class EntityReferenceHelper
    {
        public string GetEntityTitle(long id, long typeId)
        {
            EntityManager entityManager = new EntityManager();

            try
            {
                var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(typeId).EntityStoreType);
                return instanceStore.GetTitleById(id);
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public SelectList GetEntityTypes()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            EntityManager entityManager = new EntityManager();

            try
            {
                entityManager.Entities.ToList().ForEach(e => list.Add(new SelectListItem() { Text = e.Name, Value = e.Id.ToString() }));

                return new SelectList(list, "Value", "Text");
            }
            finally
            {
                entityManager.Dispose();
            }
        }
    }
}