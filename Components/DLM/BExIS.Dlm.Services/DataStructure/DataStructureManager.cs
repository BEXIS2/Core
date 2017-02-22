using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Persistence.Api;
using DS = BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Dlm.Services.DataStructure
{
    /// <summary>
    /// DataStructureManager class is responsible for CRUD (Create, Read, Update, Delete) operations on the aggregate area of the data structure.
    /// The data structure aggregate area is a set of entities like <see cref="DataStructure"/>, <see cref="VariableUsage"/>, and <see cref="ParameterUsage"/> that in 
    /// cooperation together can materialize the formal specification of the structure of group of datasets.
    /// </summary>
    public sealed class DataStructureManager
    {

        public DataStructureManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.StructuredDataStructureRepo = uow.GetReadOnlyRepository<StructuredDataStructure>();
            this.UnStructuredDataStructureRepo = uow.GetReadOnlyRepository<UnStructuredDataStructure>();
            this.AllTypesDataStructureRepo = uow.GetReadOnlyRepository<DS.DataStructure>();
            this.VariableRepo = uow.GetReadOnlyRepository<Variable>();
        }

        #region Data Readers

        /// <summary>
        /// Provides read-only querying and access to structured data structures
        /// </summary>
        public IReadOnlyRepository<StructuredDataStructure> StructuredDataStructureRepo { get; private set; }

        /// <summary>
        /// Provides read-only querying and access to unstructured data structures
        /// </summary>
        public IReadOnlyRepository<UnStructuredDataStructure> UnStructuredDataStructureRepo { get; private set; }

        /// <summary>
        /// Provides read-only querying and access to both data structure types
        /// </summary>
        public IReadOnlyRepository<DS.DataStructure> AllTypesDataStructureRepo { get; private set; }

        /// <summary>
        /// Provides read-only querying and access to variables
        /// </summary>
        public IReadOnlyRepository<Variable> VariableRepo { get; private set; }

        #endregion

        #region StructuredDataStructure

        /// <summary>
        /// Creates a structured data structure <seealso cref="StructuredDataStructure"/> and persists the entity in the database.
        /// </summary>
        /// <param name="name">The name of the data structure</param>
        /// <param name="description">A free text describing the purpose, usage, and/or the domain of the data structure usage.</param>
        /// <param name="xsdFileName">Not in use.</param>
        /// <param name="xslFileName">Not in use.</param>
        /// <param name="indexerType">If the data structure is used as a matrix, The indexer type show what kind of column would be represented by the indexer variable. <see cref="DataStructureCategory"/></param>
        /// <param name="indexer">The variable indicating the first indexing column of the matrix, if the data structure is representing a matrix.</param>
        /// <returns>The persisted structured data structure instance.</returns>
        public StructuredDataStructure CreateStructuredDataStructure(string name, string description, string xsdFileName, string xslFileName, DataStructureCategory indexerType, Variable indexer = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(indexerType != DataStructureCategory.Generic ? (indexer != null) : true);
            Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0);

            StructuredDataStructure e = new StructuredDataStructure()
            {
                Name = name,
                Description = description,
                XsdFileName = xsdFileName,
                XslFileName = xslFileName,
                IndexerType = indexerType,
                // Indexer = indexer, // how its possible to have the indexer before assigning variable to the structure
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        /// <summary>
        /// If the <paramref name="entity"/> is not associated to any <see cref="Dateset"/>, the method deletes it from the database.
        /// </summary>
        /// <param name="entity">The data structure object to be deleted.</param>
        /// <returns>True if the data structure is deleted, False otherwise.</returns>
        /// <remarks>Database exceptions are not handled intentionally, so that if the data structure is related to some datasets, a proper exception will be thrown.</remarks>
        public bool DeleteStructuredDataStructure(StructuredDataStructure entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);
            IReadOnlyRepository<Dataset> datasetRepo = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>();
            if (datasetRepo.Query(p => p.DataStructure.Id == entity.Id).Count() > 0)
                throw new Exception(string.Format("Data structure {0} is used by datasets. Deletion Failed", entity.Id));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                // delete associated variables and thier parameters
                IRepository<Variable> variableRepo = uow.GetRepository<Variable>();
                IRepository<Parameter> paramRepo = uow.GetRepository<Parameter>();
                foreach (var usage in entity.Variables)
                {
                    variableRepo.Delete(usage);
                    paramRepo.Delete(usage.Parameters.ToList());

                }
                //uow.Commit(); //  should not be needed
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        /// <summary>
        /// If non of the <paramref name="entities"/> are associated to any <see cref="Dateset"/> entity, the method deletes them from the database.
        /// </summary>
        /// <param name="entities">The data structure objects to be deleted in a all or none approach.</param>
        /// <returns>True if all the data structures are deleted, False otherwise.</returns>
        /// <remarks>If any of the data structure objects is used by any dataset, the whole transaction will be roll backed, so that the other entities are also not deleted.
        /// <br>Database exceptions are not handled intentionally, so that if the data structure is related to some datasets, a proper exception will be thrown.</br>
        /// </remarks>
        public bool DeleteStructuredDataStructure(IEnumerable<StructuredDataStructure> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (StructuredDataStructure e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (StructuredDataStructure e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                IReadOnlyRepository<Dataset> datasetRepo = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>();
                IRepository<Variable> variableRepo = uow.GetRepository<Variable>();
                IRepository<Parameter> paramRepo = uow.GetRepository<Parameter>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    if (datasetRepo.Query(p => p.DataStructure.Id == latest.Id).Count() > 0)
                    {
                        uow.Ignore();
                        throw new Exception(string.Format("Data structure {0} is used by datasets. Deletion Failed", entity.Id));
                    }

                    // delete associated variables and thier parameters
                    foreach (var usage in latest.Variables)
                    {
                        variableRepo.Delete(usage);
                        paramRepo.Delete(usage.Parameters.ToList());
                    }
                    //uow.Commit(); //  should not be needed

                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        /// <summary>
        /// Applies changes to the data structure and persists them in the database.
        /// </summary>
        /// <param name="entity">The entity containing the changes.</param>
        /// <returns>The data structure entity with the changes applied.</returns>
        public StructuredDataStructure UpdateStructuredDataStructure(StructuredDataStructure entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region UnStructuredDataStructure

        /// <summary>
        /// Creates an unstructured data structure <seealso cref="UnStructuredDataStructure"/> and persists it in the database.
        /// </summary>
        /// <param name="name">The name of the data structure</param>
        /// <param name="description">A free text describing the purpose, usage, and/or the domain of the data structure usage.</param>
        public UnStructuredDataStructure CreateUnStructuredDataStructure(string name, string description)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0);

            UnStructuredDataStructure e = new UnStructuredDataStructure()
            {
                Name = name,
                Description = description,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        /// <summary>
        /// If the <paramref name="entity"/> is not associated to any <see cref="Dateset"/>, the method deletes it from the database.
        /// </summary>
        /// <param name="entity">The data structure object to be deleted.</param>
        /// <returns>True if the data structure is deleted, False otherwise.</returns>
        /// <remarks>Database exceptions are not handled intentionally, so that if the data structure is related to some datasets, a proper exception will be thrown.</remarks>
        public bool DeleteUnStructuredDataStructure(UnStructuredDataStructure entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();

                entity = repo.Reload(entity);
                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        /// <summary>
        /// If non of the <paramref name="entities"/> are associated to any <see cref="Dateset"/> entity, the method deletes them from the database.
        /// </summary>
        /// <param name="entities">The data structure objects to be deleted in a all or none approach.</param>
        /// <returns>True if all the data structures are deleted, False otherwise.</returns>
        /// <remarks>If any of the data structure objects is used by any dataset, the whole transaction will be roll backed, so that the other entities are also not deleted.
        /// <br>Database exceptions are not handled intentionally, so that if the data structure is related to some datasets, a proper exception will be thrown.</br>
        /// </remarks>
        public bool DeleteUnStructuredDataStructure(IEnumerable<UnStructuredDataStructure> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (UnStructuredDataStructure e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (UnStructuredDataStructure e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        /// <summary>
        /// Applies changes to the data structure and persists them in the database.
        /// </summary>
        /// <param name="entity">The entity containing the changes.</param>
        /// <returns>The data structure entity with the changes applied.</returns>
        public UnStructuredDataStructure UpdateUnStructuredDataStructure(UnStructuredDataStructure entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");
            Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region Associations

        /// <summary>
        /// Creates a link between a <see cref="StructuredDataStructure"/> and <see cref="DataAttribute"/>. This link is known as <see cref="Variable"/>.
        /// In addition to what a variable inherits from the associated data attribute, it can have its own label, default and missing values, and optionality of its value.
        /// </summary>
        /// <param name="dataStructure">The structured data structure to be linked to the data attribute</param>
        /// <param name="dataAttribute">The data attribute to be used in a data structure as a variable</param>
        /// <param name="isValueOptional">Indicates whether the <see cref="VariableValue"/> associated to the variable is optional or not. This allows dataset to not provide data values for optional variables.</param>
        /// <param name="label">The display name of the variable. It may differ from the associated data attribute name. The variable label usually indicates the role of the data attribute in the structure. 
        /// Its possible for a data structure to use a data attribute more than once by creating more than one variables, hence having different labels.</param>
        /// <param name="defaultValue">The default value of the associated variable values. Mainly considered for user interface purposes.</param>
        /// <param name="missingValue">A specific sentinel value that when is put into the variable values, means those values are missing and should not be considered data.</param>
        /// <param name="variableUnit">A specific unit for the variable. If not provided the unit of the <paramref name="dataAttibute"/> is used.
        /// If provided, its dimension must be equal to the dimension of the <paramref name="dataAttribute"/>'s unit.</param>
        /// <returns>A created and persisted variable object.</returns>
        public Variable AddVariableUsage(StructuredDataStructure dataStructure, DataAttribute dataAttribute, bool isValueOptional, string label, string defaultValue, string missingValue, string description, Unit variableUnit = null)
        {
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(dataAttribute != null && dataAttribute.Id >= 0);
            Contract.Requires((variableUnit == null && dataAttribute.Unit == null) || (variableUnit == null) || (variableUnit.Dimension == dataAttribute.Unit.Dimension));
            Contract.Ensures(Contract.Result<Variable>() != null && Contract.Result<Variable>().Id >= 0);

            //StructuredDataStructureRepo.Reload(dataStructure);
            StructuredDataStructureRepo.LoadIfNot(dataStructure.Variables);
            int count = (from v in dataStructure.Variables
                         where v.DataAttribute.Id.Equals(dataAttribute.Id)
                         select v
                        )
                        .Count();

            //if (count > 0)
            //    throw new Exception(string.Format("Data attribute {0} is already used as a variable in data structure {0}", dataAttribute.Id, dataStructure.Id));

            Variable usage = new Variable()
            {
                DataStructure = dataStructure,
                DataAttribute = dataAttribute,
                MinCardinality = isValueOptional ? 0 : 1,
                // if there is no label provided, use the data attribute name and a sequence number calculated by the number of occurrences of that data attribute in the current structure
                Label = !string.IsNullOrWhiteSpace(label) ? label : (count <= 0 ? dataAttribute.Name : string.Format("{0} ({1})", dataAttribute.Name, count)),
                DefaultValue = defaultValue,
                MissingValue = missingValue,
                Description = description,
                Unit = (variableUnit != null ? variableUnit : dataAttribute.Unit),
            };
            dataAttribute.UsagesAsVariable.Add(usage);
            dataStructure.Variables.Add(usage);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Variable> repo = uow.GetRepository<Variable>();
                repo.Put(usage);
                uow.Commit();
            }
            return (usage);
        }

        /// <summary>
        /// Detaches the data attribute and the data structure that were linked by the variable and then deletes the variable from the database.
        /// </summary>
        /// <param name="usage">The variable object to be deleted.</param>
        /// <remarks>If the variable is referenced by any <see cref="DataValue"/> the method fails to delete the variable. Also, all the parameters associated to the variable will be deleted.</remarks>
        public void RemoveVariableUsage(Variable usage)
        {
            Contract.Requires(usage != null && usage.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Variable> repo = uow.GetRepository<Variable>();
                IRepository<Parameter> paramRepo = uow.GetRepository<Parameter>();
                repo.Delete(usage);
                paramRepo.Delete(usage.Parameters.ToList());
                uow.Commit();
            }
        }

        /// <summary>
        /// The method functions in a similar way to the <see cref="AddVariableUsage"/> method, but operates on a <see cref="Parameter"/>
        /// </summary>
        /// <param name="variableUsage"></param>
        /// <param name="dataAttribute"></param>
        /// <param name="isValueOptional"></param>
        /// <param name="label"></param>
        /// <param name="defaultValue"></param>
        /// <param name="missingValue"></param>
        /// <returns></returns>
        public Parameter AddParameterUsage(Variable variableUsage, DataAttribute dataAttribute, bool isValueOptional, string label, string defaultValue, string missingValue, string description)
        {
            Contract.Requires(variableUsage != null && variableUsage.DataAttribute.Id >= 0);
            Contract.Requires(dataAttribute != null && dataAttribute.Id >= 0);
            Contract.Ensures(Contract.Result<Parameter>() != null && Contract.Result<Parameter>().Id >= 0);

            VariableRepo.Reload(variableUsage);
            VariableRepo.LoadIfNot(variableUsage.Parameters);
            int count = (from pu in variableUsage.Parameters
                         where pu.DataAttribute.Id.Equals(dataAttribute.Id)
                         select pu
                        )
                        .Count();

            // support multiple use of a data attribute as a parameter in a variable context
            //if (count > 0)
            //    throw new Exception(string.Format("Data attribute {0} is already used as a parameter in conjunction with variable {1} in data structure {2}", dataAttribute.Id, variableUsage.DataAttribute.Id, variableUsage.DataStructure.Id));

            Parameter usage = new Parameter()
            {
                DataAttribute = dataAttribute,
                Variable = variableUsage,
                MinCardinality = isValueOptional ? 0 : 1,
                // if there is no label provided, use the data attribute name and a sequence number calculated by the number of occurrences of that data attribute in the current usage
                Label = !string.IsNullOrWhiteSpace(label) ? label : (count <= 0 ? dataAttribute.Name : string.Format("{0} ({1})", dataAttribute.Name, count)),
                DefaultValue = defaultValue,
                MissingValue = missingValue,
                Description = description
            };
            dataAttribute.UsagesAsParameter.Add(usage);
            variableUsage.Parameters.Add(usage);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Parameter> repo = uow.GetRepository<Parameter>();
                repo.Put(usage);
                uow.Commit();
            }
            return (usage);
        }

        /// <summary>
        /// The method functions in a similar way to the <see cref="RemoveVariableUsage"/> method, but operates on a <see cref="Parameter"/>
        /// </summary>
        /// <param name="usage"></param>
        public void RemoveParameterUsage(Parameter usage)
        {
            Contract.Requires(usage != null && usage.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Parameter> repo = uow.GetRepository<Parameter>();
                repo.Delete(usage);
                uow.Commit();
            }
        }

        /// <summary>
        /// Adds a spanning view to the passed structured data structure. Spanning views are available and applicable to all datasets associated with the data structure.
        /// </summary>
        /// <param name="dataStructure">The structured data structure to add the data view to.</param>
        /// <param name="view">The data view to be linked to the data structure as a spanning view.</param>
        public void AddDataView(BExIS.Dlm.Entities.DataStructure.StructuredDataStructure dataStructure, DataView view)
        {
            // view should not be connected to a Dataset. if so throw an exception and the caller must remove the relationship to that dataset and then add to a data structure
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0);


            StructuredDataStructureRepo.Reload(dataStructure);
            StructuredDataStructureRepo.LoadIfNot(dataStructure.Views);
            int count = (from v in dataStructure.Views
                         where v.Id.Equals(view.Id)
                         select v
                        )
                        .Count();

            if (count > 0)
                throw new Exception(string.Format("There is a connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

            dataStructure.Views.Add(view);
            view.DataStructures.Add(dataStructure);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Put(dataStructure);
                uow.Commit();
            }
        }

        /// <summary>
        /// Adds a spanning view to the passed unstructured data structure. Spanning views are available and applicable to all datasets associated with the data structure.
        /// </summary>
        /// <param name="dataStructure">The unstructured data structure to add the data view to.</param>
        /// <param name="view">The data view to be linked to the data structure as a spanning view.</param>
        public void AddDataView(BExIS.Dlm.Entities.DataStructure.UnStructuredDataStructure dataStructure, DataView view)
        {
            // view should not be connected to a Dataset. if so throw an exception and the caller must remove the relationship to that dataset and then add to a data structure
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0);


            UnStructuredDataStructureRepo.Reload(dataStructure);
            UnStructuredDataStructureRepo.LoadIfNot(dataStructure.Views);
            int count = (from v in dataStructure.Views
                         where v.Id.Equals(view.Id)
                         select v
                        )
                        .Count();

            if (count > 0)
                throw new Exception(string.Format("There is a connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

            dataStructure.Views.Add(view);
            view.DataStructures.Add(dataStructure);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                repo.Put(dataStructure);
                uow.Commit();
            }
        }

        /// <summary>
        /// Removes the relationship between the structured data structure and the view, neither the data structure nor the view.
        /// </summary>
        /// <param name="dataStructure">The data structure to be release from the relationship.</param>
        /// <param name="view">The view to be release from the relationship.</param>
        public void RemoveDataView(BExIS.Dlm.Entities.DataStructure.StructuredDataStructure dataStructure, DataView view)
        {
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0);

            StructuredDataStructureRepo.Reload(dataStructure);
            StructuredDataStructureRepo.LoadIfNot(dataStructure.Views);
            int count = (from v in dataStructure.Views
                         where v.Id.Equals(view.Id)
                         select v
                        )
                        .Count();

            if (count <= 0)
                throw new Exception(string.Format("There is no connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

            dataStructure.Views.Remove(view);
            view.DataStructures.Remove(dataStructure);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Put(dataStructure);
                uow.Commit();
            }
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the relationship between the structured data structure and the view, neither the data structure nor the view.
        /// </summary>
        /// <param name="dataStructure">The data structure to be release from the relationship.</param>
        /// <param name="view">The view to be release from the relationship.</param>
        public void RemoveDataView(BExIS.Dlm.Entities.DataStructure.UnStructuredDataStructure dataStructure, DataView view)
        {
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0);

            UnStructuredDataStructureRepo.Reload(dataStructure);
            UnStructuredDataStructureRepo.LoadIfNot(dataStructure.Views);
            int count = (from v in dataStructure.Views
                         where v.Id.Equals(view.Id)
                         select v
                        )
                        .Count();

            if (count <= 0)
                throw new Exception(string.Format("There is no connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

            dataStructure.Views.Remove(view);
            view.DataStructures.Remove(dataStructure);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                repo.Put(dataStructure);
                uow.Commit();
            }
            //throw new NotImplementedException();
        }

        #endregion

    }
}
