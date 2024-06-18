using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Diagnostics.Contracts;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Helpers
{
    /// <summary>
    /// Design decision: should it be a static class?!
    /// </summary>
    /// <remarks>Should not be used directly or by any function outside of the service layer.</remarks>
    public class ExtendedPropertyHelper
    {
        #region ExtendedProperty

        public ExtendedProperty Create(ExtendedProperty property, DataContainer container)
        {
            if (property == null) throw new ArgumentNullException(nameof(property), "property should not be null");
            if (container == null) throw new ArgumentNullException(nameof(container), "container should not be null");

            property.DataContainer = container;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();
                repo.Put(property);
                uow.Commit();
            }
            return (property);
        }

        public bool Delete(ExtendedProperty entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();

                entity = repo.Reload(entity);
                //delete the unit
                repo.Delete(entity);
                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        #endregion ExtendedProperty
    }
}