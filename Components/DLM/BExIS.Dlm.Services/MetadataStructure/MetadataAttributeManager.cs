using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Persistence.Api;
using BExIS.Dlm.Entities.MetadataStructure;

namespace BExIS.Dlm.Services.MetadataStructure
{
    public sealed class MetadataAttributeManager
    {

        public MetadataAttributeManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.MetadataAttributeRepo = uow.GetReadOnlyRepository<MetadataAttribute>();
        }

        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<MetadataAttribute> MetadataAttributeRepo { get; private set; }

        #endregion

        #region MetadataAttribute

        public MetadataAttribute Create(string shortName, string name, string description, bool isMultiValue, bool isBuiltIn, string owner, MeasurementScale measurementScale, DataContainerType containerType, string entitySelectionPredicate,
            DataType dataType, Unit unit, Methodology methodology, 
            //Classifier classifier,
            ICollection<AggregateFunction> functions, ICollection<GlobalizationInfo> globalizationInfos, ICollection<Constraint> constraints            
            )
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(shortName));
            Contract.Requires(dataType != null && dataType.Id >= 0);
            Contract.Requires(unit != null && unit.Id >= 0);

            Contract.Ensures(Contract.Result<MetadataAttribute>() != null && Contract.Result<MetadataAttribute>().Id >= 0);

            MetadataAttribute e = new MetadataAttribute()
            {
                ShortName = shortName,
                Name = name,
                Description = description,
                IsMultiValue = isMultiValue,
                IsBuiltIn = isBuiltIn,
                Owner = owner,
                MeasurementScale = measurementScale,
                ContainerType = containerType,
                EntitySelectionPredicate = entitySelectionPredicate,
                DataType = dataType,
                Unit = unit,
                Methodology = methodology,
                AggregateFunctions = functions,
                GlobalizationInfos = globalizationInfos,
                Constraints = constraints,                
            };
            //if (classifier != null && classifier.Id > 0)
            //    e.Classification = classifier;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataAttribute> repo = uow.GetRepository<MetadataAttribute>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);            
        }

        public bool Delete(MetadataAttribute entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataAttribute> repo = uow.GetRepository<MetadataAttribute>();
                
                entity = repo.Reload(entity);
                repo.LoadIfNot(entity.ExtendedProperties);
                //repo.LoadIfNot(entity.ParameterUsages);
                
                entity.ExtendedProperties.Clear();

                // consider package associations

                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<MetadataAttribute> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (MetadataAttribute e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (MetadataAttribute e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataAttribute> repo = uow.GetRepository<MetadataAttribute>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.LoadIfNot(latest.ExtendedProperties);
                    //repo.LoadIfNot(entity.ParameterUsages);

                    // consider package associations

                    //vpuRepo.Delete(entity.ParameterUsages);
                    //entity.ParameterUsages.Clear();

                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public MetadataAttribute Update(MetadataAttribute entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<MetadataAttribute>() != null && Contract.Result<MetadataAttribute>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataAttribute> repo = uow.GetRepository<MetadataAttribute>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);    
        }

        #endregion
    
        #region Associations

        public DataAttribute AddConstraint(DataContainer container, Constraint constraint)
        {
            throw new NotImplementedException();
        }

        public DataAttribute RemoveConstraint(DataContainer container, Constraint constraint)
        {
            throw new NotImplementedException();
        }

        public DataAttribute AddConstraint(ExtendedProperty extendedProperty, Constraint constraint)
        {
            throw new NotImplementedException();
        }

        public DataAttribute RemoveConstraint(ExtendedProperty extendedProperty, Constraint constraint)
        {
            throw new NotImplementedException();
        }

        public DataAttribute AddExtendedProperty(DataContainer container, ExtendedProperty extendedProperty)
        {
            throw new NotImplementedException();
        }

        public DataAttribute RemoveExtendedProperty(ExtendedProperty extendedProperty)
        {
            throw new NotImplementedException();
        }

        public DataAttribute AddAggregateFunction(DataContainer container, AggregateFunction aggregateFunction)
        {
            throw new NotImplementedException();
        }

        public DataAttribute RemoveAggregateFunction(DataContainer container, AggregateFunction aggregateFunction)
        {
            throw new NotImplementedException();
        }

        public DataAttribute AddGlobalizationInfo(DataContainer container, GlobalizationInfo globalizationInfo)
        {
            throw new NotImplementedException();
        }

        public DataAttribute RemoveGlobalizationInfo(GlobalizationInfo globalizationInfo)
        {
            throw new NotImplementedException();
        }
        
        #endregion

    }
}
