using BExIS.Dim.Entities.Mappings;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Services.Mappings
{
    public class ConceptManager : IDisposable
    {
        private IUnitOfWork guow = null;
        private bool isDisposed = false;

        public ConceptManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.MappingConceptRepo = guow.GetReadOnlyRepository<MappingConcept>();
            this.MappingKeyRepo = guow.GetReadOnlyRepository<MappingKey>();
        }

        ~ConceptManager()
        {
            Dispose(true);
        }

        public IQueryable<MappingConcept> MappingConcepts => MappingConceptRepo.Query();

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
        public IReadOnlyRepository<MappingConcept> MappingConceptRepo { get; private set; }

        public IReadOnlyRepository<MappingKey> MappingKeyRepo { get; private set; }

        #endregion Data Readers

        #region Concept

        public MappingConcept CreateMappingConcept(string name, string description, string url, string xsd)
        {
            MappingConcept concept = new MappingConcept();
            concept.Name = name;
            concept.Description = description;
            concept.Url = url;
            concept.XSD = xsd;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MappingConcept> repo = uow.GetRepository<MappingConcept>();
                repo.Put(concept);
                uow.Commit();
            }

            return (concept);
        }

        public bool DeleteMappingConcept(MappingConcept entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MappingConcept> repo = uow.GetRepository<MappingConcept>();

                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool UpdateMappingConcept(MappingConcept entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MappingConcept> repo = uow.GetRepository<MappingConcept>();

                repo.Put(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        #endregion Concept

        #region Key

        public MappingKey CreateMappingKey(string name, string description, string url, bool optional, bool isComplex, string xpath = "", MappingConcept concept = null, MappingKey parent = null)
        {
            MappingKey entity = new MappingKey();
            entity.Name = name;
            entity.Description = description;
            entity.Url = url;
            entity.Optional = optional;
            entity.IsComplex = isComplex;
            entity.Concept = concept;
            entity.Parent = parent;
            entity.XPath = xpath;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MappingKey> repo = uow.GetRepository<MappingKey>();
                repo.Put(entity);
                uow.Commit();
            }

            return (entity);
        }

        public bool DeleteMappingKey(MappingKey entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MappingKey> repo = uow.GetRepository<MappingKey>();

                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool UpdateMappingKey(MappingKey entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MappingKey> repo = uow.GetRepository<MappingKey>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        #endregion Key
    }
}