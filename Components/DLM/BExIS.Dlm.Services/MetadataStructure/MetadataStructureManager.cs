using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

using MDS = BExIS.Dlm.Entities.MetadataStructure;

namespace BExIS.Dlm.Services.MetadataStructure
{
    public class MetadataStructureManager : IDisposable
    {
        private IUnitOfWork guow = null;

        public MetadataStructureManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.Repo = guow.GetReadOnlyRepository<MDS.MetadataStructure>();
            this.PackageUsageRepo = guow.GetReadOnlyRepository<MDS.MetadataPackageUsage>();
        }

        private bool isDisposed = false;

        ~MetadataStructureManager()
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
        public IReadOnlyRepository<MDS.MetadataStructure> Repo { get; private set; }

        public IReadOnlyRepository<MDS.MetadataPackageUsage> PackageUsageRepo { get; private set; }

        #endregion Data Readers

        #region MetadataStructure

        public MDS.MetadataStructure GetMetadataStructureById(long id)
        {
            return Repo.Get(id);
        }

        public List<MetadataPackageUsage> GetEffectivePackages(MDS.MetadataStructure structure)
        {
            return GetEffectivePackages(structure.Id);
        }

        public List<MetadataPackageUsage> GetEffectivePackages(Int64 structureId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IReadOnlyRepository<MDS.MetadataPackageUsage> repo = uow.GetReadOnlyRepository<MDS.MetadataPackageUsage>();

                /*PostgreSQL82Dialect, DB2Dialect*/
                if (AppConfiguration.DatabaseDialect.Equals("DB2Dialect"))
                {
                    return GetPackages(structureId);
                }
                else //if (AppConfiguration.DatabaseDialect.Equals("PostgreSQL82Dialect"))
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("metadataStructureId", structureId);
                    List<MetadataPackageUsage> usages = repo.Get("GetEffectivePackageUsages", parameters).ToList();
                    return usages; // structure.MetadataPackageUsages.ToList(); // plus all the packages of the parents
                }
            }
        }

        public List<Int64> GetEffectivePackageIds(Int64 structureId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IReadOnlyRepository<MDS.MetadataPackageUsage> repo = uow.GetReadOnlyRepository<MDS.MetadataPackageUsage>();

                /*PostgreSQL82Dialect, DB2Dialect*/
                if (AppConfiguration.DatabaseDialect.Equals("DB2Dialect"))
                {
                    return GetPackages(structureId).Select(p => p.Id).ToList();
                }
                else //if (AppConfiguration.DatabaseDialect.Equals("PostgreSQL82Dialect"))
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("metadataStructureId", structureId);
                    List<Int64> usages = uow.ExecuteList<Int64>("GetEffectivePackageUsageIds", parameters);
                    return usages; // structure.MetadataPackageUsages.ToList(); // plus all the packages of the parents
                }
            }
        }

        private List<MetadataPackageUsage> GetPackages(Int64 structureId)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IReadOnlyRepository<MDS.MetadataStructure> repo = uow.GetReadOnlyRepository<MDS.MetadataStructure>();
                List<MetadataPackageUsage> list = new List<MetadataPackageUsage>();
                MDS.MetadataStructure metadataStructure = repo.Get(structureId);

                if (metadataStructure.Parent != null)
                {
                    list.AddRange(GetPackages(metadataStructure.Parent.Id));
                }

                list.AddRange(metadataStructure.MetadataPackageUsages);

                return list;
            }
        }

        public MDS.MetadataStructure Create(string name, string description, string xsdFileName, string xslFileName, MDS.MetadataStructure parent)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(Contract.Result<MDS.MetadataStructure>() != null && Contract.Result<MDS.MetadataStructure>().Id >= 0);

            MDS.MetadataStructure u = new MDS.MetadataStructure()
            {
                Name = name,
                Description = description,
                XsdFileName = xsdFileName,
                XslFileName = xslFileName,
                Parent = parent, // if parent is null, current node will be a root
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MDS.MetadataStructure> repo = uow.GetRepository<MDS.MetadataStructure>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
        }

        public bool Delete(MDS.MetadataStructure entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);
            IReadOnlyRepository<Dataset> datasetRepo = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>();
            if (datasetRepo.Query(p => p.MetadataStructure.Id == entity.Id).Count() > 0)
                throw new Exception(string.Format("Metadata structure {0} is used by datasets. Deletion Failed", entity.Id));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MDS.MetadataStructure> repo = uow.GetRepository<MDS.MetadataStructure>();
                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        public bool Delete(IEnumerable<MDS.MetadataStructure> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (MDS.MetadataStructure e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (MDS.MetadataStructure e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MDS.MetadataStructure> repo = uow.GetRepository<MDS.MetadataStructure>();
                IReadOnlyRepository<Dataset> datasetRepo = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>();
                foreach (var entity in entities)
                {
                    if (datasetRepo.Query(p => p.MetadataStructure.Id == entity.Id).Count() > 0)
                    {
                        uow.Ignore();
                        throw new Exception(string.Format("Metadata structure {0} is used by datasets. Deletion Failed", entity.Id));
                    }
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public MDS.MetadataStructure Update(MDS.MetadataStructure entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<MDS.MetadataStructure>() != null && Contract.Result<MDS.MetadataStructure>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MDS.MetadataStructure> repo = uow.GetRepository<MDS.MetadataStructure>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);

                uow.Commit();
            }
            return (entity);
        }

        #endregion MetadataStructure

        #region Associations

        /// <summary>
        /// Adds end2 object to the list of children of end1 object, if not already there
        /// </summary>
        /// <param name="end1">The parent object</param>
        /// <param name="end2">The child object</param>
        /// <returns></returns>
        public bool AddChild(MDS.MetadataStructure end1, MDS.MetadataStructure end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(end2 != null && end2.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MDS.MetadataStructure> repo = uow.GetRepository<MDS.MetadataStructure>();

                end1 = repo.Reload(end1);
                repo.LoadIfNot(end1.Children);
                if (!end1.Children.Contains(end2))
                {
                    //needs loop prevention control, so that end2 is in the set of {end1 and its parents}
                    end1.Children.Add(end2);
                    end2.Parent = end1;
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        /// <summary>
        /// Removes end2 from end1 without deleting either end1 or end 2.
        /// </summary>
        /// <param name="end1">The parent object</param>
        /// <param name="end2">The child object</param>
        /// <returns></returns>
        public bool RemoveChild(MDS.MetadataStructure end1, MDS.MetadataStructure end2)
        {
            Contract.Requires(end1 != null && end1.Id >= 0);
            Contract.Requires(end2 != null && end2.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MDS.MetadataStructure> repo = uow.GetRepository<MDS.MetadataStructure>();

                end1 = repo.Reload(end1);
                repo.LoadIfNot(end1.Children);

                end2 = repo.Reload(end2);
                repo.LoadIfNot(end2.Parent);

                if (end1.Children.Contains(end2) || end2.Parent.Equals(end1))
                {
                    end1.Children.Remove(end2);
                    end2.Parent = null;
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        public MetadataPackageUsage AddMetadataPackageUsage(MDS.MetadataStructure structure, MetadataPackage package, string label, string description, int minCardinality, int maxCardinality, XmlDocument extra = null)
        {
            Contract.Requires(package != null && package.Id >= 0);
            Contract.Requires(structure != null && structure.Id >= 0);

            Contract.Ensures(Contract.Result<MetadataPackageUsage>() != null && Contract.Result<MetadataPackageUsage>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataPackageUsage> repo = uow.GetRepository<MetadataPackageUsage>();
                IRepository<MDS.MetadataStructure> repo2 = uow.GetRepository<MDS.MetadataStructure>();
                repo2.Reload(structure);
                repo2.LoadIfNot(structure.MetadataPackageUsages);
                int count = 0;
                try
                {
                    count = (from v in structure.MetadataPackageUsages
                             where v.MetadataPackage.Id.Equals(package.Id)
                             select v
                             )
                             .Count();
                }
                catch { }

                MetadataPackageUsage usage = new MetadataPackageUsage()
                {
                    MetadataPackage = package,
                    MetadataStructure = structure,
                    // if no label is provided, use the package name and a sequence number calculated by the number of occurrences of that package in the current structure
                    Label = !string.IsNullOrWhiteSpace(label) ? label : (count <= 0 ? package.Name : string.Format("{0} ({1})", package.Name, count)),
                    Description = description,
                    MinCardinality = minCardinality,
                    MaxCardinality = maxCardinality,
                    Extra = extra
                };
                structure.MetadataPackageUsages.Add(usage);
                package.UsedIn.Add(usage);

                repo.Put(usage);
                uow.Commit();
                return (usage);
            }
        }

        public void RemoveMetadataPackageUsage(MetadataPackageUsage usage)
        {
            Contract.Requires(usage != null && usage.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataPackageUsage> repo = uow.GetRepository<MetadataPackageUsage>();
                repo.Delete(usage);
                uow.Commit();
            }
        }

        #endregion Associations
    }
}