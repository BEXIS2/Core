using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;
using Version = BExIS.Security.Entities.Versions.Version;

namespace BExIS.Security.Services.Versions
{
    public class VersionManager
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public VersionManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            VersionRepository = _guow.GetReadOnlyRepository<Version>();
        }

        ~VersionManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Version> VersionRepository { get; }

        public IQueryable<Version> Versions => VersionRepository.Query();

        public Version LatestVersion => VersionRepository.Query().OrderByDescending(v => v.Date).FirstOrDefault();

        public void Dispose()
        {
            Dispose(true);
        }

        public Version GetLatestVersion(string module = "Shell")
        {
            using (var uow = this.GetUnitOfWork())
            {
                return !Exists(module) ? null : VersionRepository.Query(v => string.Equals(v.Module, module, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(v => v.Date).FirstOrDefault();
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _guow?.Dispose();
                    _isDisposed = true;
                }
            }
        }

        public void Update(Version entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<Version>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);

                merged.Date = DateTime.Now;

                repo.Put(merged);
                uow.Commit();
            }
        }

        public bool Exists(string module, string value)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Version>();

                if (string.IsNullOrEmpty(module))
                    return false;

                if (string.IsNullOrEmpty(value))
                    return false;

                return operationRepository.Query(v => v.Module.ToUpperInvariant() == module.ToUpperInvariant() && v.Value.ToUpperInvariant() == value.ToUpperInvariant()).Count() == 1;
            }
        }

        public bool Exists(string module)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Version>();

                if (string.IsNullOrEmpty(module))
                    return false;

                return operationRepository.Query(v => v.Module.ToUpperInvariant() == module.ToUpperInvariant()).Any();
            }
        }

        public Version Create(string module, string value)
        {
            using (var uow = this.GetUnitOfWork())
            {
                if (Exists(module, value))
                    return null;

                var version = new Version()
                {
                    Module = module,
                    Value = value,
                    Date = DateTime.Now
                };

                var versionRepository = uow.GetRepository<Version>();
                versionRepository.Put(version);
                uow.Commit();

                return version;
            }
        }
    }
}
