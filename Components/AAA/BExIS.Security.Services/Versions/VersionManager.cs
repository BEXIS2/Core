using System;
using System.Linq;
using Vaiona.Persistence.Api;
using Version = BExIS.Security.Entities.Versions.Version;

namespace BExIS.Security.Services.Versions
{
    public class VersionManager
    {
        public Version GetLatestVersion(string module = "Shell")
        {
            if (string.IsNullOrWhiteSpace(module))
                return null;

            using (var uow = this.GetUnitOfWork())
            {
                var versionsRepository = uow.GetReadOnlyRepository<Version>();
                return versionsRepository.Query(v => v.Module == module)
                       .OrderByDescending(v => v.Date)
                       .FirstOrDefault();
            }           
        }

        public void Update(Version version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            using (var uow = this.GetUnitOfWork())
            {
                var versionsRepository = uow.GetRepository<Version>();
                versionsRepository.Merge(version);
                var merged = versionsRepository.Get(version.Id);

                merged.Date = DateTime.Now;

                versionsRepository.Put(merged);
                uow.Commit();
            }
        }

        public bool Exists(string module, string value)
        {
            if (string.IsNullOrEmpty(module))
                return false;

            if (string.IsNullOrEmpty(value))
                return false;

            using (var uow = this.GetUnitOfWork())
            {
                var versionsRepository = uow.GetReadOnlyRepository<Version>();
                return versionsRepository.Query(v => v.Module == module && v.Value == value).Any();
            }
        }

        public Version Create(string module, string value)
        {
            if (string.IsNullOrWhiteSpace(module))
                throw new ArgumentException(nameof(module));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(nameof(value));

            using (var uow = this.GetUnitOfWork())
            {
                var versionsRepository = uow.GetRepository<Version>();

                var exists = versionsRepository.Query(v =>
                v.Module == module &&
                v.Value == value).Any();

                if (exists)
                    return null;

                var version = new Version
                {
                    Module = module,
                    Value = value,
                    Date = DateTime.Now
                };

                versionsRepository.Put(version);
                uow.Commit();

                return version;
            }
        }
    }
}