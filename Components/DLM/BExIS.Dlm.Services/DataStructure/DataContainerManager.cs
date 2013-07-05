using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.DataStructure;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using BExIS.Core.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public sealed class DataContainerManager
    {

        public DataContainerManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.DataAttributeRepo = uow.GetReadOnlyRepository<DataAttribute>();
            this.ExtendedPropertyRepo = uow.GetReadOnlyRepository<ExtendedProperty>();
            this.UsageRepo = uow.GetReadOnlyRepository<ParameterUsage>();
        }

        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<DataAttribute> DataAttributeRepo { get; private set; }
        public IReadOnlyRepository<ExtendedProperty> ExtendedPropertyRepo { get; private set; }
        public IReadOnlyRepository<ParameterUsage> UsageRepo { get; private set; }

        #endregion

        #region DataAttribute

        public DataAttribute CreateDataAttribute(string shortName, string name, string description, bool isMultiValue, bool isBuiltIn, string owner, MeasurementScale measurementScale, DataContainerType containerType, string entitySelectionPredicate,
            DataType dataType, Unit unit, Methodology methodology, ICollection<AggregateFunction> functions, ICollection<GlobalizationInfo> globalizationInfos, ICollection<Constraint> constraints,
            ICollection<ExtendedProperty> extendedProperies
            )
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(shortName));
            Contract.Requires(!string.IsNullOrWhiteSpace(owner));
            Contract.Requires(dataType != null && dataType.Id >= 0);

            Contract.Ensures(Contract.Result<DataAttribute>() != null && Contract.Result<DataAttribute>().Id >= 0);
            DataAttribute e = new DataAttribute()
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
                AggregateFunctions = new List<AggregateFunction>(functions),
                GlobalizationInfos = new List<GlobalizationInfo>(globalizationInfos),
                Constraints = new List<Constraint>(constraints),
                ExtendedProperties = new List<ExtendedProperty>(extendedProperies),
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataAttribute> repo = uow.GetRepository<DataAttribute>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);            
        }

        public bool DeleteVariable(DataAttribute entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataAttribute> repo = uow.GetRepository<DataAttribute>();
                IRepository<ExtendedProperty> exRepo = uow.GetRepository<ExtendedProperty>();
                IRepository<ParameterUsage> vpuRepo = uow.GetRepository<ParameterUsage>();

                entity = repo.Reload(entity);
                repo.LoadIfNot(entity.ExtendedProperties);
                //repo.LoadIfNot(entity.ParameterUsages);
                
                exRepo.Delete(entity.ExtendedProperties);
                entity.ExtendedProperties.Clear();

                //vpuRepo.Delete(entity.ParameterUsages);
                //entity.ParameterUsages.Clear();

                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool DeleteVariable(IEnumerable<DataAttribute> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (DataAttribute e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (DataAttribute e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataAttribute> repo = uow.GetRepository<DataAttribute>();
                IRepository<ExtendedProperty> exRepo = uow.GetRepository<ExtendedProperty>();
                IRepository<ParameterUsage> vpuRepo = uow.GetRepository<ParameterUsage>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.LoadIfNot(latest.ExtendedProperties);
                    //repo.LoadIfNot(entity.ParameterUsages);

                    exRepo.Delete(entity.ExtendedProperties);
                    entity.ExtendedProperties.Clear();

                    //vpuRepo.Delete(entity.ParameterUsages);
                    //entity.ParameterUsages.Clear();

                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public DataAttribute UpdateVariable(DataAttribute entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<DataAttribute>() != null && Contract.Result<DataAttribute>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataAttribute> repo = uow.GetRepository<DataAttribute>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);    
        }

        #endregion

        #region Parameter      

        //public Parameter CreateParameter(string shortName, string name, string description, bool isMultiValue, MeasurementScale measurementScale, DataContainerType containerType, string entitySelectionPredicate,
        //   DataType dataType, Unit unit, Methodology methodology, ICollection<AggregateFunction> functions, ICollection<GlobalizationInfo> globalizationInfos, ICollection<Constraint> constraints,
        //   ICollection<ExtendedProperty> extendedProperies
        //   )
        //{
        //    Contract.Requires(!string.IsNullOrWhiteSpace(shortName));
        //    Contract.Requires(dataType != null && dataType.Id >= 0);

        //    Contract.Ensures(Contract.Result<Parameter>() != null && Contract.Result<Parameter>().Id >= 0);
        //    Parameter e = new Parameter()
        //    {
        //        ShortName = shortName,
        //        Name = name,
        //        Description = description,
        //        IsMultiValue = isMultiValue,
        //        MeasurementScale = measurementScale,
        //        ContainerType = containerType,
        //        EntitySelectionPredicate = entitySelectionPredicate,
        //        DataType = dataType,
        //        Unit = unit,
        //        Methodology = methodology,
        //        AggregateFunctions = new List<AggregateFunction>(functions),
        //        GlobalizationInfos = new List<GlobalizationInfo>(globalizationInfos),
        //        Constraints = new List<Constraint>(constraints),
        //        ExtendedProperties = new List<ExtendedProperty>(extendedProperies),
        //    };

        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<Parameter> repo = uow.GetRepository<Parameter>();
        //        repo.Put(e);
        //        uow.Commit();
        //    }
        //    return (e);
        //}

        //public bool DeleteParameter(Parameter entity)
        //{
        //    Contract.Requires(entity != null);
        //    Contract.Requires(entity.Id >= 0);

        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<Parameter> repo = uow.GetRepository<Parameter>();
        //        IRepository<ExtendedProperty> exRepo = uow.GetRepository<ExtendedProperty>();
        //        IRepository<VariableParameterUsage> vpuRepo = uow.GetRepository<VariableParameterUsage>();

        //        entity = repo.Reload(entity);
        //        repo.LoadIfNot(entity.ExtendedProperties);
        //        repo.LoadIfNot(entity.VariableUsages);

        //        exRepo.Delete(entity.ExtendedProperties);
        //        entity.ExtendedProperties.Clear();

        //        vpuRepo.Delete(entity.VariableUsages);
        //        entity.VariableUsages.Clear();

        //        repo.Delete(entity);

        //        uow.Commit();
        //    }
        //    // if any problem was detected during the commit, an exception will be thrown!
        //    return (true);
        //}

        //public bool DeleteParameter(IEnumerable<Parameter> entities)
        //{
        //    Contract.Requires(entities != null);
        //    Contract.Requires(Contract.ForAll(entities, (Parameter e) => e != null));
        //    Contract.Requires(Contract.ForAll(entities, (Parameter e) => e.Id >= 0));

        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<Parameter> repo = uow.GetRepository<Parameter>();
        //        IRepository<ExtendedProperty> exRepo = uow.GetRepository<ExtendedProperty>();
        //        IRepository<VariableParameterUsage> vpuRepo = uow.GetRepository<VariableParameterUsage>();

        //        foreach (var entity in entities)
        //        {
        //            var latest = repo.Reload(entity);
        //            repo.LoadIfNot(latest.ExtendedProperties);
        //            repo.LoadIfNot(entity.VariableUsages);

        //            exRepo.Delete(entity.ExtendedProperties);
        //            entity.ExtendedProperties.Clear();

        //            vpuRepo.Delete(entity.VariableUsages);
        //            entity.VariableUsages.Clear();

        //            repo.Delete(latest);
        //        }
        //        uow.Commit();
        //    }
        //    return (true);
        //}

        //public Parameter UpdateParameter(Parameter entity)
        //{
        //    Contract.Requires(entity != null, "provided entity can not be null");
        //    Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

        //    Contract.Ensures(Contract.Result<Parameter>() != null && Contract.Result<Parameter>().Id >= 0, "No entity is persisted!");

        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<Parameter> repo = uow.GetRepository<Parameter>();
        //        repo.Put(entity); // Merge is required here!!!!
        //        uow.Commit();
        //    }
        //    return (entity);
        //}
           
        #endregion

        #region Extended Property

        public ExtendedProperty CreateExtendedProperty(string name, string description, DataContainer container, ICollection<Constraint> constraints)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(container != null && container.Id >= 0);

            Contract.Ensures(Contract.Result<ExtendedProperty>() != null && Contract.Result<ExtendedProperty>().Id >= 0);
            ExtendedProperty e = new ExtendedProperty()
            {
                Name = name,
                Description = description,
                DataContainer = container,
            };
            //if (constraints != null)
            //    e.Constraints = new List<Constraint>(constraints);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool DeleteExtendedProperty(ExtendedProperty entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();

                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool DeleteExtendedProperty(IEnumerable<ExtendedProperty> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (ExtendedProperty e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (ExtendedProperty e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public ExtendedProperty UpdateExtendedProperty(ExtendedProperty entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<ExtendedProperty>() != null && Contract.Result<ExtendedProperty>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ExtendedProperty> repo = uow.GetRepository<ExtendedProperty>();
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

        /// <summary>
        /// Removes the association between the container and the constraint. not the container nor the constraint. Note that it is possible for the constraint
        /// to be associated with more than container
        /// </summary>
        /// <param name="container">A variable or a parameter</param>
        /// <param name="constraint">Any kind of constraint: DomainValue, Validator, DefaultValue</param>
        /// <returns></returns>
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
