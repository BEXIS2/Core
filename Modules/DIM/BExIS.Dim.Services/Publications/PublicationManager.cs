using BExIS.Dim.Entities.Publications;
using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Services.Publications
{
    public class PublicationManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public PublicationManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            PublicationRepository = _guow.GetReadOnlyRepository<Publication>();
        }

        ~PublicationManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Publication> PublicationRepository { get; }
        public IQueryable<Publication> Publications => PublicationRepository.Query();

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }

        public Publication Create(Publication publication)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var publicationRepository = uow.GetRepository<Publication>();
                    publicationRepository.Put(publication);
                    uow.Commit();
                }

                return publication;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool Delete(Publication publication)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var publicationRepository = uow.GetRepository<Publication>();
                    publicationRepository.Delete(publication);
                    uow.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool DeleteById(long publicationId)
        {
            return Delete(PublicationRepository.Get(publicationId));
        }

        public bool ExistsById(long publicationId)
        {
            return false;
        }


        public Task<Publication> FindByIdAsync(long publicationId)
        {
            return Task.FromResult(PublicationRepository.Get(publicationId));

        }
        public Publication FindById(long publicationId)
        {
            return PublicationRepository.Get(publicationId);
        }

        //public async Task<(byte[] FileContents, string ContentType, string FileName)> GetFileByIdAsync(long publicationId)
        //{
        //    var publication = FindById(publicationId);

        //    if (publication == null)
        //        throw new ArgumentException("Publication does not exist", nameof(publicationId));

        //    switch (publication.Broker.Name.ToLower())
        //    {
        //        case "pangaea":
        //            {
        //                PangaeaDataRepoConverter dataRepoConverter = new PangaeaDataRepoConverter(_broker);

        //                tmp = new Tuple<string, string>(dataRepoConverter.Convert(datasetVersionId), "text/txt");
        //                return tmp;
        //            }

        //        default:
        //            break;
        //    }

        //    if (!File.Exists(filePath))
        //    {
        //        throw new FileNotFoundException("File not found.", filePath);
        //    }
        //}

        public bool Update(Publication publication)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetRepository<Publication>();
                    repo.Merge(publication);
                    var merged = repo.Get(publication.Id);
                    repo.Put(merged);
                    uow.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string GetFilePathById(long publicationId, string extension = "zip")
        {
            try
            {
                var publication = FindById(publicationId);

                if (publication == null)
                    throw new ArgumentException("Publication does not exist", nameof(publicationId));

                return Path.Combine(AppConfiguration.DataPath, "datasets", publication.DatasetVersion.Dataset.Id.ToString(), "publish", publication.Repository.Name.ToLower(), $"{publication.DatasetVersion.Dataset.Id}_{publication.DatasetVersion.VersionNo}_dataset_{publication.Repository.Name.ToLower()}.{extension}");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetFileNameById(long publicationId, string extension = "zip")
        {
            try
            {
                var publication = FindById(publicationId);

                if (publication == null)
                    throw new ArgumentException("Publication does not exist", nameof(publicationId));

                return $"{publication.DatasetVersion.Dataset.Id}_{publication.DatasetVersion.VersionNo}_dataset_{publication.Repository.Name.ToLower()}.{extension}";
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
