using BExIS.Dim.Entities.Publications;
using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Services
{
    public class PublicationManager : IDisposable
    {
        private IUnitOfWork guow = null;

        public PublicationManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.PublicationRepo = guow.GetReadOnlyRepository<Publication>();
            this.BrokerRepo = guow.GetReadOnlyRepository<Broker>();
            this.RepositoryRepo = guow.GetReadOnlyRepository<Repository>();
            this.MetadataStructureToRepositoryRepo = guow.GetReadOnlyRepository<MetadataStructureToRepository>();
        }

        private bool isDisposed = false;

        ~PublicationManager()
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

        /// <summary>
        /// Provides read-only querying and access to publicationss
        /// </summary>
        public IReadOnlyRepository<Publication> PublicationRepo { get; private set; }

        /// <summary>
        /// Provides read-only querying and access to brokers
        /// </summary>
        public IReadOnlyRepository<Broker> BrokerRepo { get; private set; }

        /// <summary>
        /// Provides read-only querying and access to repositories
        /// </summary>
        public IReadOnlyRepository<Repository> RepositoryRepo { get; private set; }

        /// <summary>
        /// Provides read-only querying and access to repositories
        /// </summary>
        public IReadOnlyRepository<MetadataStructureToRepository> MetadataStructureToRepositoryRepo { get; private set; }

        #endregion Data Readers

        #region publication

        /// <summary>
        /// Retrieves the publication object having identifier <paramref name="Id"/> from the database.
        /// </summary>
        /// <param name="Id">The identifier of the publication.</param>
        /// <returns>The semi-populated publication entity if exists, or null.</returns>
        /// <remarks>The object based attributes of the entity that are persisted as XML are not populated by default. In order to fully populate the entity, call the <see cref="Materialize"/> method.</remarks>
        public Publication GetPublication(Int64 Id)
        {
            Publication p = PublicationRepo.Get(Id);

            return (p);
        }

        /// <summary>
        /// Retrieves all publication object <paramref name="Id"/> from the database.
        /// </summary>
        /// <param name="Id">The identifier of the publication.</param>
        /// <returns>The semi-populated publication entity if exists, or null.</returns>
        /// <remarks>The object based attributes of the entity that are persisted as XML are not populated by default. In order to fully populate the entity, call the <see cref="Materialize"/> method.</remarks>
        public List<Publication> GetPublication()
        {
            return PublicationRepo.Query().ToList();
        }

        /// <summary>
        /// Creates an publication.
        /// </summary>
        /// <returns>A publication entities.</returns>
        //[MeasurePerformance]
        public Publication CreatePublication(DatasetVersion datasetVersion,
            Broker broker,
            string name,
            long researchObjectId,
            string filePath = "",
            string externalLink = "",
            string status = "")
        {
            Contract.Ensures(Contract.Result<Publication>() != null && Contract.Result<Publication>().Id >= 0);

            Publication publication = new Publication();
            publication.ResearchObjectId = researchObjectId;
            publication.Broker = broker;
            publication.DatasetVersion = datasetVersion;
            publication.Repository = broker.Repository;
            publication.Timestamp = DateTime.Now;
            publication.Status = status;
            publication.FilePath = filePath;
            publication.ExternalLink = externalLink;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Publication> repo = uow.GetRepository<Publication>();
                repo.Put(publication);
                uow.Commit();
            }
            return (publication);
        }

        public Publication CreatePublication(DatasetVersion datasetVersion,
            Broker broker,
            Repository repository,
            string name,
            long researchObjectId,
            string filePath = "",
            string externalLink = "",
            string status = "")
        {
            Contract.Ensures(Contract.Result<Publication>() != null && Contract.Result<Publication>().Id >= 0);

            Publication publication = new Publication();
            publication.ResearchObjectId = researchObjectId;
            publication.Broker = broker;
            publication.DatasetVersion = datasetVersion;
            publication.Repository = repository;
            publication.Timestamp = DateTime.Now;
            publication.Status = status;
            publication.FilePath = filePath;
            publication.ExternalLink = externalLink;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Publication> repo = uow.GetRepository<Publication>();
                repo.Put(publication);
                uow.Commit();
            }
            return (publication);
        }

        /// <summary>
        /// In cases that the publication's attributes are changed, this method persists the changes.
        /// </summary>
        /// <param name="publication">A publication instance containing the changes</param>
        /// <returns>The publication instance with the changes applied</returns>
        public Publication UpdatePublication(Publication publication)
        {
            Contract.Requires(publication != null);
            Contract.Requires(publication.Id >= 0);

            Contract.Ensures(Contract.Result<Publication>() != null && Contract.Result<Publication>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Publication> repo = uow.GetRepository<Publication>();
                repo.Merge(publication);
                var merged = repo.Get(publication.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (publication);
        }

        public bool DeletePublication(Publication publication)
        {
            Contract.Requires(publication != null);
            Contract.Requires(publication.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Publication> repo = uow.GetRepository<Publication>();

                var latest = repo.Reload(publication);
                repo.Delete(latest);

                uow.Commit();
            }
            return (true);
        }

        #endregion publication

        #region broker

        public List<Broker> GetBroker()
        {
            return BrokerRepo.Query().ToList();
        }

        /// <summary>
        /// Retrieves the broker object having identifier <paramref name="Id"/> from the database.
        /// </summary>
        /// <param name="Id">The identifier of the broker.</param>
        /// <returns>The semi-populated broker entity if exists, or null.</returns>
        /// <remarks>The object based attributes of the entity that are persisted as XML are not populated by default. In order to fully populate the entity, call the <see cref="Materialize"/> method.</remarks>
        public Broker GetBroker(Int64 Id)
        {
            Broker b = BrokerRepo.Get(Id);

            return (b);
        }

        /// <summary>
        /// Creates an Broker and persists it in the database.
        /// </summary>
        /// <param name="name">The name of the data structure</param>
        /// <param name="url">A free text describing the purpose, usage, and/or the domain of the data structure usage.</param>
        /// <param name="broker"></param>
        ///
        public Broker CreateBroker(string name, string server, string username, string password, string metadataFormat, string mimeType)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(server));
            Contract.Requires(!string.IsNullOrWhiteSpace(username));
            Contract.Requires(!string.IsNullOrWhiteSpace(password));

            Broker e = new Broker()
            {
                Name = name,
                Server = server,
                UserName = username,
                Password = password,
                MetadataFormat = metadataFormat,
                PrimaryDataFormat = mimeType
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Broker> repo = uow.GetRepository<Broker>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        /// <summary>
        /// Creates an Broker and persists it in the database.
        /// </summary>
        /// <param name="name">The name of the data structure</param>
        /// <param name="url">A free text describing the purpose, usage, and/or the domain of the data structure usage.</param>
        /// <param name="broker"></param>
        ///
        public Broker CreateBroker(Broker broker)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(broker.Name));
            Contract.Requires(!string.IsNullOrWhiteSpace(broker.Server));
            Contract.Requires(!string.IsNullOrWhiteSpace(broker.UserName));
            Contract.Requires(!string.IsNullOrWhiteSpace(broker.Password));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Broker> repo = uow.GetRepository<Broker>();
                repo.Put(broker);
                uow.Commit();
            }
            return (broker);
        }

        /// <summary>
        /// In cases that the broker's attributes are changed, this method persists the changes.
        /// </summary>
        /// <param name="broker">A broker instance containing the changes</param>
        /// <returns>The broker instance with the changes applied</returns>
        public Broker UpdateBroker(Broker broker)
        {
            Contract.Requires(broker != null);
            Contract.Requires(broker.Id >= 0);

            Contract.Ensures(Contract.Result<Broker>() != null && Contract.Result<Broker>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Broker> repo = uow.GetRepository<Broker>();
                repo.Merge(broker);
                var merged = repo.Get(broker.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (broker);
        }

        public bool DeleteBroker(Broker broker)
        {
            Contract.Requires(broker != null);
            Contract.Requires(broker.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Broker> repo = uow.GetRepository<Broker>();

                var latest = repo.Reload(broker);
                repo.Delete(latest);

                uow.Commit();
            }
            return true;
        }

        #endregion broker

        #region repository

        public List<Repository> GetRepository()
        {
            return RepositoryRepo.Query().ToList();
        }

        /// <summary>
        /// Retrieves the repository object having identifier <paramref name="Id"/> from the database.
        /// </summary>
        /// <param name="Id">The identifier of the repository.</param>
        /// <returns>The semi-populated repository entity if exists, or null.</returns>
        /// <remarks>The object based attributes of the entity that are persisted as XML are not populated by default. In order to fully populate the entity, call the <see cref="Materialize"/> method.</remarks>
        public Repository GetRepository(Int64 Id)
        {
            Repository r = RepositoryRepo.Get(Id);

            return (r);
        }

        /// <summary>
        /// Creates an Repository and persists it in the database.
        /// </summary>
        /// <param name="name">The name of the data structure</param>
        /// <param name="url">A free text describing the purpose, usage, and/or the domain of the data structure usage.</param>
        /// <param name="broker"></param>
        ///
        public Repository CreateRepository(string name, string url)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            Repository e = new Repository()
            {
                Name = name,
                Url = url
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Repository> repo = uow.GetRepository<Repository>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        /// <summary>
        /// In cases that the repository's attributes are changed, this method persists the changes.
        /// </summary>
        /// <param name="repository">A repository instance containing the changes</param>
        /// <returns>The repository instance with the changes applied</returns>
        public Repository UpdateRepository(Repository repository)
        {
            Contract.Requires(repository != null);
            Contract.Requires(repository.Id >= 0);

            Contract.Ensures(Contract.Result<Repository>() != null && Contract.Result<Repository>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Repository> repo = uow.GetRepository<Repository>();
                repo.Merge(repository);
                var merged = repo.Get(repository.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (repository);
        }

        public bool DeleteRepository(Repository entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Repository> repo = uow.GetRepository<Repository>();

                var latest = repo.Reload(entity);
                repo.Delete(latest);

                uow.Commit();
            }
            return true;
        }

        #endregion repository

        #region MetadataStructureToRepository

        public MetadataStructureToRepository GetMetadataStructureToRepository(long metadataStrutcureId, long repositoryId)
        {
            Contract.Requires(metadataStrutcureId > 0);
            Contract.Requires(repositoryId > 0);

            MetadataStructureToRepository b = MetadataStructureToRepositoryRepo.Get().FirstOrDefault(m => m.MetadataStructureId.Equals(metadataStrutcureId) &&
            m.RepositoryId.Equals(repositoryId));

            return (b);
        }

        public IEnumerable<MetadataStructureToRepository> GetAllMetadataStructureToRepository(long metadataStrutcureId)
        {
            Contract.Requires(metadataStrutcureId > 0);

            return MetadataStructureToRepositoryRepo.Get().Where(m => m.MetadataStructureId.Equals(metadataStrutcureId));
        }

        public MetadataStructureToRepository CreateMetadataStructureToRepository(long metadataStrutcureId, long repositoryId)
        {
            Contract.Requires(metadataStrutcureId > 0);
            Contract.Requires(repositoryId > 0);

            MetadataStructureToRepository e = new MetadataStructureToRepository()
            {
                MetadataStructureId = metadataStrutcureId,
                RepositoryId = repositoryId
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataStructureToRepository> repo = uow.GetRepository<MetadataStructureToRepository>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool DeleteMetadataStructureToRepository(MetadataStructureToRepository metadataStructureToRepository)
        {
            Contract.Requires(metadataStructureToRepository != null);
            Contract.Requires(metadataStructureToRepository.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataStructureToRepository> repo = uow.GetRepository<MetadataStructureToRepository>();

                var latest = repo.Reload(metadataStructureToRepository);
                repo.Delete(latest);

                uow.Commit();
            }
            return (true);
        }

        #endregion MetadataStructureToRepository
    }
}