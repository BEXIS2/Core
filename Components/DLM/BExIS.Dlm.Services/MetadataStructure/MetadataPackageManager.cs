using BExIS.Dlm.Entities.MetadataStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.MetadataStructure
{
    public class MetadataPackageManager : IDisposable
    {

        private IUnitOfWork guow = null;
        public MetadataPackageManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.MetadataPackageRepo = guow.GetReadOnlyRepository<MetadataPackage>();
        }

        private bool isDisposed = false;
        ~MetadataPackageManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (guow != null)
                        guow.Dispose();
                    isDisposed = true;
                }
            }
        }


        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<MetadataPackage> MetadataPackageRepo { get; private set; }

        #endregion

        #region MetadataPackage

        public MetadataPackage Create(string name, string description, bool isEnabled = false)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            Contract.Ensures(Contract.Result<MetadataPackage>() != null && Contract.Result<MetadataPackage>().Id >= 0);

            MetadataPackage e = new MetadataPackage()
            {
                Name = name,
                Description = description,
                IsEnabled = isEnabled,
            };
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataPackage> repo = uow.GetRepository<MetadataPackage>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool Delete(MetadataPackage entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataPackage> repo = uow.GetRepository<MetadataPackage>();
                entity = repo.Reload(entity);
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<MetadataPackage> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (MetadataPackage e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (MetadataPackage e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataPackage> repo = uow.GetRepository<MetadataPackage>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public MetadataPackage Update(MetadataPackage entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<MetadataPackage>() != null && Contract.Result<MetadataPackage>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataPackage> repo = uow.GetRepository<MetadataPackage>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region Associations

        public MetadataAttributeUsage AddMetadataAtributeUsage(MetadataPackage package, MetadataAttribute attribute, string label, string description, int minCardinality, int maxCardinality, string defaultValue)
        {
            Contract.Requires(package != null && package.Id >= 0);
            Contract.Requires(attribute != null && attribute.Id >= 0);

            Contract.Ensures(Contract.Result<MetadataAttributeUsage>() != null && Contract.Result<MetadataAttributeUsage>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                var metadataPackageRepo = uow.GetReadOnlyRepository<MetadataPackage>();
                var attributesRepo = uow.GetReadOnlyRepository<MetadataAttribute>();

                attribute = attributesRepo.Get(attribute.Id);
                metadataPackageRepo.Reload(package);
                metadataPackageRepo.LoadIfNot(package.MetadataAttributeUsages);
                int count = 0;
                try
                {
                    count = (from v in package.MetadataAttributeUsages
                             where v.MetadataAttribute.Id.Equals(attribute.Id)
                             select v
                             )
                             .Count();
                }
                catch { }

                MetadataAttributeUsage usage = new MetadataAttributeUsage()
                {
                    MetadataPackage = package,
                    MetadataAttribute = attribute,
                    // if there is no label provided, use the attribute name and a sequence number calculated by the number of occurrences of that attribute in the current structure
                    Label = !string.IsNullOrWhiteSpace(label) ? label : (count <= 0 ? attribute.Name : string.Format("{0} ({1})", attribute.Name, count)),
                    Description = description,
                    MinCardinality = minCardinality,
                    MaxCardinality = maxCardinality,
                    DefaultValue = defaultValue
                };
                package.MetadataAttributeUsages.Add(usage);
                attribute.UsedIn.Add(usage);


                IRepository<MetadataAttributeUsage> repo = uow.GetRepository<MetadataAttributeUsage>();
                repo.Put(usage);
                uow.Commit();

                return (usage);
            }
        }

        public void RemoveMetadataAtributeUsage(MetadataAttributeUsage usage)
        {
            Contract.Requires(usage != null && usage.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataAttributeUsage> repo = uow.GetRepository<MetadataAttributeUsage>();
                repo.Delete(usage);
                uow.Commit();
            }
        }

        #endregion

    }
}
