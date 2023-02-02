using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.MetadataStructure
{
    public class MetadataAttributeManager : IDisposable
    {
        ConstraintHelper helper = new ConstraintHelper();

        private IUnitOfWork guow = null;
        public MetadataAttributeManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.MetadataAttributeRepo = guow.GetReadOnlyRepository<MetadataAttribute>();
            this.MetadataSimpleAttributeRepo = guow.GetReadOnlyRepository<MetadataSimpleAttribute>();
            this.MetadataCompoundAttributeRepo = guow.GetReadOnlyRepository<MetadataCompoundAttribute>();
            this.MetadataParameterRepo = guow.GetReadOnlyRepository<MetadataParameter>();

            //[DS] add this Repos to get usages by id
            this.MetadataNestedAttributeUsageRepo = guow.GetReadOnlyRepository<MetadataNestedAttributeUsage>();
            this.MetadataAttributeUsageRepo = guow.GetReadOnlyRepository<MetadataAttributeUsage>();
            this.MetadataParameterUsageRepo = guow.GetReadOnlyRepository<MetadataParameterUsage>();
        }

        private bool isDisposed = false;
        ~MetadataAttributeManager()
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
        public IReadOnlyRepository<MetadataAttribute> MetadataAttributeRepo { get; private set; }
        public IReadOnlyRepository<MetadataParameter> MetadataParameterRepo { get; private set; }
        public IReadOnlyRepository<MetadataSimpleAttribute> MetadataSimpleAttributeRepo { get; private set; }
        public IReadOnlyRepository<MetadataCompoundAttribute> MetadataCompoundAttributeRepo { get; private set; }
        public IReadOnlyRepository<MetadataNestedAttributeUsage> MetadataNestedAttributeUsageRepo { get; private set; }
        public IReadOnlyRepository<MetadataAttributeUsage> MetadataAttributeUsageRepo { get; private set; }
        public IReadOnlyRepository<MetadataParameterUsage> MetadataParameterUsageRepo { get; private set; }

        #endregion

        #region MetadataAttribute

        /// <summary>
        /// Persists a simple metadata attribute is the database.
        /// </summary>
        /// <param name="entity">is an unsaved simple metadata attribute.</param>
        /// <returns>The saved attribute.</returns>
        /// <remarks>The method does not check duplicate names.</remarks>
        public MetadataSimpleAttribute Create(MetadataSimpleAttribute entity)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(entity.ShortName));
            Contract.Requires(entity.DataType != null && entity.DataType.Id >= 0);

            Contract.Ensures(Contract.Result<MetadataSimpleAttribute>() != null && Contract.Result<MetadataSimpleAttribute>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataSimpleAttribute> repo = uow.GetRepository<MetadataSimpleAttribute>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        /// <summary>
        /// Creates a simple metadata attribute and persists it in the database
        /// </summary>
        /// <param name="shortName"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="isMultiValue">Indicates whether the attribute accepts multiple values.</param>
        /// <param name="isBuiltIn">If yes, the attribute is created by the system itself and is not delete-able</param>
        /// <param name="scope">Creates a context of ownership for the attribute so that the modules, or different parts of the system can use it i.e. for filtering.</param>
        /// <param name="measurementScale"></param>
        /// <param name="containerType"></param>
        /// <param name="entitySelectionPredicate"></param>
        /// <param name="dataType"></param>
        /// <param name="unit"></param>
        /// <param name="methodology"></param>
        /// <param name="functions"></param>
        /// <param name="globalizationInfos"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public MetadataSimpleAttribute Create(string shortName, string name, string description, bool isMultiValue, bool isBuiltIn, string scope, MeasurementScale measurementScale, DataContainerType containerType, string entitySelectionPredicate,
            DataType dataType, Unit unit, Methodology methodology,
            //Classifier classifier, 
            ICollection<AggregateFunction> functions, ICollection<GlobalizationInfo> globalizationInfos, ICollection<Constraint> constraints
            )
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(shortName));
            Contract.Requires(dataType != null && dataType.Id >= 0);
            //Contract.Requires(unit != null && unit.Id >= 0);

            Contract.Ensures(Contract.Result<MetadataSimpleAttribute>() != null && Contract.Result<MetadataSimpleAttribute>().Id >= 0);

            MetadataSimpleAttribute entity = new MetadataSimpleAttribute()
            {
                ShortName = shortName,
                Name = name,
                Description = description,
                IsMultiValue = isMultiValue,
                IsBuiltIn = isBuiltIn,
                Scope = scope,
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
            //    entity.Classification = classifier;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataSimpleAttribute> repo = uow.GetRepository<MetadataSimpleAttribute>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        /// <summary>
        /// Persists a compound metadata attribute in the database
        /// </summary>
        /// <param name="entity">is an unsaved compound metadata data attribute</param>
        /// <returns>The saved compound metadata attribute</returns>
        /// <remarks>The attribute should have at least two other attributes as its member to be considered a compound! Also it should have at least a non empty short name.</remarks>     
        public MetadataCompoundAttribute Create(MetadataCompoundAttribute entity)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(entity.ShortName));
            //Contract.Requires(entity.DataType != null && entity.DataType.Id >= 0);
            //Contract.Requires(entity.Unit != null && entity.Unit.Id >= 0);
            Contract.Requires(entity.MetadataNestedAttributeUsages != null && entity.MetadataNestedAttributeUsages.Count >= 2);

            Contract.Ensures(Contract.Result<MetadataCompoundAttribute>() != null && Contract.Result<MetadataCompoundAttribute>().Id >= 0);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataCompoundAttribute> repo = uow.GetRepository<MetadataCompoundAttribute>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        /// <summary>
        /// Persists a metadata parameter in the database
        /// </summary>
        /// <param name="entity">is an unsaved metadata parameter</param>
        /// <returns>The saved metadata parameter</returns>
        /// <remarks>It should have at least a non empty short name.</remarks>     
        public MetadataParameter Create(MetadataParameter entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "parameter not exist.");
            if(string.IsNullOrWhiteSpace(entity.ShortName)) throw new ArgumentNullException(nameof(entity.ShortName), "shortname of parameter is empty.");
            if(entity.DataType == null) throw new ArgumentNullException(nameof(entity.DataType), "data type of parameter is null.");
            Contract.Ensures(Contract.Result<MetadataCompoundAttribute>() != null && Contract.Result<MetadataCompoundAttribute>().Id >= 0);


            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataParameter> repo = uow.GetRepository<MetadataParameter>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        /// <summary>
        /// Deletes the saved <paramref name="entity"/> from the database. The entity should not be a built-in one.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>true if successful, false otherwise.</returns>
        /// <remarks>If by deleting the <paramref name="entity"/> any other compound attribute will have less than 2 members, the deletion fails.</remarks>
        public bool Delete(MetadataAttribute entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);
            Contract.Requires(entity.IsBuiltIn == false);
            // check whether this attribute is used in any compound and if yes, those compounds have enough members after the deletion!
            var q = from compound in MetadataCompoundAttributeRepo.Query()
                    from usage in compound.MetadataNestedAttributeUsages
                        // whether the compound is linked to entity and whether deleting entity causes the compound to have less than two members
                    where usage.Member == entity && compound.MetadataNestedAttributeUsages.Count() <= 2
                    select compound;
            int cnt = 0;
            if ((cnt = q.Count()) > 0)
            {
                if (cnt == 1)
                    throw new Exception(string.Format("Deletion failed! Attribute {0} is used in {1} compound attribute that has less than 3 members. Compound attributes should have at least 2 members, invariantly.", entity.Id, cnt));
                else
                    throw new Exception(string.Format("Deletion failed! Attribute {0} is used in {1} compound attributes that have less than 3 members. Compound attributes should have at least 2 members, invariantly.", entity.Id, cnt));
            }

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataAttribute> repo = uow.GetRepository<MetadataAttribute>();

                entity = repo.Reload(entity);
                repo.LoadIfNot(entity.ExtendedProperties);
                entity.ExtendedProperties.Clear();
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        /// <summary>
        /// Tries to delete all the attributes provided in the <paramref name="entities"/>, supposed that they are all persisted and non built-in attributes.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns>true if successful, false otherwise.</returns>
        /// <remarks>If by deleting the any of the <paramref name="entities"/> any other compound attribute will have less than 2 members, the deletion fails.</remarks>
        public bool Delete(IEnumerable<MetadataAttribute> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (MetadataAttribute e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (MetadataAttribute e) => e.Id >= 0));
            Contract.Requires(Contract.ForAll(entities, (MetadataAttribute e) => e.IsBuiltIn == false));

            var q = from compound in MetadataCompoundAttributeRepo.Query()
                    from usage in compound.MetadataNestedAttributeUsages
                        // whether the compound is linked to entity and whether deleting entity causes the compound to have less than two members
                    where entities.Contains(usage.Member) && compound.MetadataNestedAttributeUsages.Count() <= 2
                    select compound;
            int cnt = 0;
            if ((cnt = q.Count()) > 0)
            {
                throw new Exception(string.Format("Deletion failed! At least one of the deleting attributes is used in a compound attribute with less than 3 members. Compound attributes should have at least 2 members, invariantly.", cnt));
            }

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<MetadataAttribute> repo = uow.GetRepository<MetadataAttribute>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.LoadIfNot(latest.ExtendedProperties);
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
                //var localEntity = repo.Merge(entity);
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();

                return (merged);
            }

        }

        #endregion

        #region Associations

        /// <summary>
        /// add a metadata parameter to an metadata attribute and connect both via metadata parameter usage
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="parameter"></param>
        /// <param name="label"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public MetadataParameterUsage AddParameterUsage(MetadataAttribute attribute, MetadataParameter parameter)
        {
            if(parameter == null || parameter.Id <= 0) throw new ArgumentNullException("parameter","parameter should not be null.") ;
            if(attribute == null || attribute.Id <= 0) throw new ArgumentNullException("attribute", "attribute should not be null.");

            string label = parameter.Name;
            string description = parameter.Description;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                var parameterRepo = uow.GetReadOnlyRepository<MetadataParameter>();
                var attributesRepo = uow.GetReadOnlyRepository<MetadataAttribute>();

                attribute = attributesRepo.Get(attribute.Id);
                attributesRepo.Reload(attribute);
                attributesRepo.LoadIfNot(attribute.MetadataParameterUsages);

                int count = 0;
                try
                {
                    count = (from v in attribute.MetadataParameterUsages
                             where v.Member.Id.Equals(parameter.Id)
                             select v
                             )
                             .Count();
                }
                catch { }

                MetadataParameterUsage usage = new MetadataParameterUsage()
                {
                    Master = attribute,
                    Member = parameter,
                    // if there is no label provided, use the attribute name and a sequence number calculated by the number of occurrences of that attribute in the current structure
                    Label = !string.IsNullOrWhiteSpace(label) ? label : (count <= 0 ? attribute.Name : string.Format("{0} ({1})", attribute.Name, count)),
                    Description = description
                };

               //attribute.MetadataParameterUsages.Add(usage);

                IRepository<MetadataParameterUsage> repo = uow.GetRepository<MetadataParameterUsage>();
                repo.Put(usage);
                uow.Commit();

                return (usage);
            }
        }

        public void AddConstraint(DomainConstraint constraint, DataContainer container)
        {
            helper.SaveConstraint(constraint, container);
        }

        public void AddConstraint(PatternConstraint constraint, DataContainer container)
        {
            helper.SaveConstraint(constraint, container);
        }

        public void AddConstraint(RangeConstraint constraint, DataContainer container)
        {
            helper.SaveConstraint(constraint, container);
        }

        public void AddConstraint(ComparisonConstraint constraint, DataContainer container)
        {
            helper.SaveConstraint(constraint, container);
        }

        public void RemoveConstraint(DomainConstraint constraint)
        {
            helper.Delete(constraint);
        }

        public void RemoveConstraint(PatternConstraint constraint)
        {
            helper.Delete(constraint);
        }

        public void RemoveConstraint(RangeConstraint constraint)
        {
            helper.Delete(constraint);
        }

        public void RemoveConstraint(ComparisonConstraint constraint)
        {
            helper.Delete(constraint);
        }

        public ExtendedProperty AddExtendedProperty(DataContainer container, ExtendedProperty extendedProperty)
        {
            ExtendedPropertyHelper helper = new ExtendedPropertyHelper();
            return helper.Create(extendedProperty, container); 

        }

        public bool RemoveExtendedProperty(ExtendedProperty extendedProperty)
        {
            ExtendedPropertyHelper helper = new ExtendedPropertyHelper();
            return helper.Delete(extendedProperty);
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
