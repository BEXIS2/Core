using BExIS.Security.Entities.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class FeatureManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public FeatureManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            FeatureRepository = _guow.GetReadOnlyRepository<Feature>();
        }

        ~FeatureManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Feature> FeatureRepository { get; }
        public IQueryable<Feature> Features => FeatureRepository.Query();

        public void Create(Feature feature)
        {
            using (var uow = this.GetUnitOfWork())
            {
                if (feature == null)
                    return;

                if (Exists(feature.Name, feature.Parent))
                    return;

                var featureRepository = uow.GetRepository<Feature>();
                featureRepository.Put(feature);
                uow.Commit();
            }
        }

        public Feature Create(string name, string description, Feature parent = null)
        {
            using (var uow = this.GetUnitOfWork())
            {
                if (string.IsNullOrEmpty(name))
                    return null;

                if (Exists(name, parent))
                    return null;

                var feature = new Feature()
                {
                    Name = name,
                    Description = description,
                    Parent = parent
                };

                var featureRepository = uow.GetRepository<Feature>();
                featureRepository.Put(feature);
                uow.Commit();

                return feature;
            }
        }

        public void Delete(Feature feature)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetRepository<Feature>();
                featureRepository.Delete(feature);
                uow.Commit();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool Exists(string name, Feature parent)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetReadOnlyRepository<Feature>();

                if (parent == null)
                    return featureRepository.Query(f => f.Name.ToUpperInvariant() == name.ToUpperInvariant() && f.Parent == null).Count() == 1;

                return featureRepository.Query(f => f.Name.ToUpperInvariant() == name.ToUpperInvariant() && f.Parent.Id == parent.Id).Count() == 1;
            }
        }

        public Feature FindById(long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetRepository<Feature>();
                return featureRepository.Get(featureId);
            }
        }

        public Feature FindByName(string groupName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetRepository<Feature>();
                return featureRepository.Query(m => m.Name.ToLowerInvariant() == groupName.ToLowerInvariant()).FirstOrDefault();
            }
        }

        public List<Feature> FindRoots()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetRepository<Feature>();
                return featureRepository.Query(f => f.Parent == null).ToList();
            }
        }

        public void Update(Feature feature)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetRepository<Feature>();
                featureRepository.Put(feature);
                uow.Commit();
            }
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
    }
}