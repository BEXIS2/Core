using BExIS.Security.Entities.Objects;
using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class FeatureManager
    {
        public Feature Create(Feature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature));

            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetRepository<Feature>();
                featureRepository.Put(feature);
                uow.Commit();
                return feature;
            }
        }

        public Feature Create(string name, string description, Feature parent = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException(nameof(description));

            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetRepository<Feature>();

                var feature = new Feature()
                {
                    Name = name,
                    Description = description,
                    Parent = parent != null ? featureRepository.Get(parent.Id) : null
                };

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

        public bool Exists(string name, Feature parent)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetReadOnlyRepository<Feature>();

                if (parent == null)
                    return featureRepository.Query(f => f.Name == name && f.Parent == null).Take(2).Count() == 1;

                return featureRepository.Query(f => f.Name == name && f.Parent.Id == parent.Id).Take(2).Count() == 1;
            }
        }

        public List<Feature> Find()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetReadOnlyRepository<Feature>();
                return requestRepository.Query().ToList();
            }
        }

        public IList<Feature> Find(Expression<Func<Feature, bool>> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetReadOnlyRepository<Feature>();
                return featureRepository.Query().Where(predicate).ToList();
            }
        }

        public Feature GetById(long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetRepository<Feature>();
                return featureRepository.Get(featureId);
            }
        }

        public Feature GetByName(string groupName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetRepository<Feature>();
                return featureRepository.Query(m => m.Name == groupName).SingleOrDefault();
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
                var repo = uow.GetRepository<Feature>();
                repo.Merge(feature);
                uow.Commit();
            }
        }
    }
}