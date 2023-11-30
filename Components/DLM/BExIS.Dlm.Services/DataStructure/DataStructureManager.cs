using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;
using DS = BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Dlm.Services.DataStructure
{
    /// <summary>
    /// DataStructureManager class is responsible for CRUD (Create, Read, Update, Delete) operations on the aggregate area of the data structure.
    /// The data structure aggregate area is a set of entities like <see cref="DataStructure"/>, <see cref="VariableUsage"/>, and <see cref="ParameterUsage"/> that in 
    /// cooperation together can materialize the formal specification of the structure of group of datasets.
    /// </summary>
    public class DataStructureManager : IDisposable
    {

        private IUnitOfWork guow = null;
        public DataStructureManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.StructuredDataStructureRepo = guow.GetReadOnlyRepository<StructuredDataStructure>();
            this.UnStructuredDataStructureRepo = guow.GetReadOnlyRepository<UnStructuredDataStructure>();
            this.AllTypesDataStructureRepo = guow.GetReadOnlyRepository<DS.DataStructure>();
            this.VariableRepo = guow.GetReadOnlyRepository<Variable>();
        }

        private bool isDisposed = false;
        ~DataStructureManager()
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

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                IRepository<UnStructuredDataStructure> sRepo = uow.GetRepository<UnStructuredDataStructure>();
                if (
                        (repo.Query(p => p.Name.ToLower() == name.ToLower()).Count() <= 0) &&
                        (sRepo.Query(p => p.Name.ToLower() == name.ToLower()).Count() <= 0)
                    )
                {
                    StructuredDataStructure e = new StructuredDataStructure()
                    {
                        Name = name,
                        Description = description,
                        XsdFileName = xsdFileName,
                        XslFileName = xslFileName,
                        IndexerType = indexerType,
                        // Indexer = indexer, // how its possible to have the indexer before assigning variable to the structure
                    };
                    repo.Put(e);
                    uow.Commit();
                    return (e);
                }
            }
            return (null);
        }

        /// <summary>
        /// If the <paramref name="entity"/> is not associated to any <see cref="Dateset"/>, the method deletes it from the database.
        /// </summary>
        /// <param name="entity">The data structure object to be deleted.</param>
        /// <returns>True if the data structure is deleted, False otherwise.</returns>
        /// <remarks>Database exceptions are not handled intentionally, so that if the data structure is related to some datasets, a proper exception will be thrown.</remarks>
        //public bool DeleteStructuredDataStructure(StructuredDataStructure entity)
        //{
        //    Contract.Requires(entity != null);
        //    Contract.Requires(entity.Id >= 0);

        //    return DeleteStructuredDataStructure(entity);
        //}

        public bool DeleteStructuredDataStructure(StructuredDataStructure entity)
        {
            Contract.Requires(entity.Id >= 0);

            IReadOnlyRepository<Dataset> datasetRepo = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>();
            if (datasetRepo.Query(p => p.DataStructure.Id == entity.Id).Count() > 0)
                throw new Exception(string.Format("Data structure {0} is used by datasets. Deletion Failed", entity.Id));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                IRepository<VariableInstance> variableRepo = uow.GetRepository<VariableInstance>();
                //IRepository<MissingValue> missinValuesRepo = uow.GetRepository<MissingValue>();

                //variableRepo.Evict();
                variableRepo.Evict();

                entity = repo.Reload(entity);
                // delete associated variables and thier parameters
                foreach (var usage in entity.Variables)
                {
                    var localVar = variableRepo.Reload(usage);           
                    variableRepo.Delete(localVar);
                }

                //uow.Commit(); //  should not be needed
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
                IRepository<VariableInstance> variableRepo = uow.GetRepository<VariableInstance>();

                foreach (var entity in entities)
                {
                    variableRepo.Evict();

                    var latest = repo.Reload(entity);
                    if (datasetRepo.Query(p => p.DataStructure.Id == latest.Id).Count() > 0)
                    {
                        uow.Ignore();
                        throw new Exception(string.Format("Data structure {0} is used by datasets. Deletion Failed", entity.Id));
                    }

                    // delete associated variables and thier parameters
                    foreach (var usage in latest.Variables)
                    {
                        var localVar = variableRepo.Reload(usage);
                        variableRepo.Delete(localVar);
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
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
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


            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                IRepository<StructuredDataStructure> sRepo = uow.GetRepository<StructuredDataStructure>();
                if (
                        (repo.Query(p => p.Name.ToLower() == name.ToLower()).Count() <= 0) &&
                        (sRepo.Query(p => p.Name.ToLower() == name.ToLower()).Count() <= 0)
                   )
                {
                    UnStructuredDataStructure e = new UnStructuredDataStructure()
                    {
                        Name = name,
                        Description = description,
                    };
                    repo.Put(e);
                    uow.Commit();
                    return (e);
                }
            }
            return (null);
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
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region Associations

        /// <summary>
        /// Add Variables to datastructure
        /// </summary>
        /// <param name="dataStructureId"></param>
        /// <param name="variableId"></param>
        /// <returns>updated datastructure</returns>
        /// <exception cref="ArgumentException"></exception>
        public StructuredDataStructure AddVariable(long dataStructureId, long variableId)
        {
            if (dataStructureId <= 0) throw new ArgumentException("dataStructureId not exist");
            if (variableId == null) throw new ArgumentException("variableId not exist");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> structuredDataStructureRepo = uow.GetRepository<StructuredDataStructure>();
                IRepository<VariableInstance> variableInstanceRepo = uow.GetRepository<VariableInstance>();

                var dataStructure = structuredDataStructureRepo.Get(dataStructureId);
                VariableInstance variableInstance = variableInstanceRepo.Get(variableId);

                structuredDataStructureRepo.LoadIfNot(dataStructure.Variables);

                dataStructure.Variables.Add(variableInstance);


                variableInstanceRepo.Put(variableInstance);
                uow.Commit();
                return (dataStructure);
            }
        }

        /// <summary>
        /// Add Vraiables to datastructure
        /// </summary>
        /// <param name="dataStructureId"></param>
        /// <param name="variableIds"></param>
        /// <returns>updated datastructure</returns>
        /// <exception cref="ArgumentException"></exception>
        public StructuredDataStructure AddVariables(long dataStructureId, List<long> variableIds)
        {
            if (dataStructureId <= 0) throw new ArgumentException("dataStructureId not exist");
            if (variableIds == null) throw new ArgumentException("variableId not exist");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> structuredDataStructureRepo = uow.GetRepository<StructuredDataStructure>();
                IRepository<VariableInstance> variableInstanceRepo = uow.GetRepository<VariableInstance>();

                var dataStructure = structuredDataStructureRepo.Get(dataStructureId);

                List<VariableInstance> variableInstances = new List<VariableInstance>();

                foreach (var id in variableIds)
                {
                    variableInstances.Add(variableInstanceRepo.Get(id));
                }

                structuredDataStructureRepo.LoadIfNot(dataStructure.Variables);

                dataStructure.Variables.ToList().AddRange(variableInstances);


                variableInstanceRepo.Put(variableInstances);
                uow.Commit();
                return dataStructure;
            }
        }

        public void RemoveVariableUsage(VariableInstance usage)
        {
            Contract.Requires(usage != null && usage.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableInstance> repo = uow.GetRepository<VariableInstance>();
                //IRepository<Parameter> paramRepo = uow.GetRepository<Parameter>();
                usage = repo.Get(usage.Id);

                repo.Delete(usage);
                //paramRepo.Delete(usage.Parameters.ToList());
                uow.Commit();
            }
        }

        public void RemoveVariableUsage(long id)
        {
            Contract.Requires(id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableInstance> repo = uow.GetRepository<VariableInstance>();
                var usage = repo.Get(id);
                repo.Delete(usage);

                uow.Commit();
            }
        }

        /// <summary>
        /// Adds a spanning view to the passed structured data structure. Spanning views are available and applicable to all datasets associated with the data structure.
        /// </summary>
        /// <param name="dataStructure">The structured data structure to add the data view to.</param>
        /// <param name="view">The data view to be linked to the data structure as a spanning view.</param>
        public void AddDataView(BExIS.Dlm.Entities.DataStructure.StructuredDataStructure dataStructure, DatasetView view)
        {
            // view should not be connected to a Dataset. if so throw an exception and the caller must remove the relationship to that dataset and then add to a data structure
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0);


            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Reload(dataStructure);
                repo.LoadIfNot(dataStructure.Views);
                int count = (from v in dataStructure.Views
                             where v.Id.Equals(view.Id)
                             select v
                            )
                            .Count();

                if (count > 0)
                    throw new Exception(string.Format("There is a connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

                dataStructure.Views.Add(view);
                view.DataStructures.Add(dataStructure);

                repo.Put(dataStructure);
                uow.Commit();
            }
        }

        /// <summary>
        /// Adds a spanning view to the passed unstructured data structure. Spanning views are available and applicable to all datasets associated with the data structure.
        /// </summary>
        /// <param name="dataStructure">The unstructured data structure to add the data view to.</param>
        /// <param name="view">The data view to be linked to the data structure as a spanning view.</param>
        public void AddDataView(BExIS.Dlm.Entities.DataStructure.UnStructuredDataStructure dataStructure, DatasetView view)
        {
            // view should not be connected to a Dataset. if so throw an exception and the caller must remove the relationship to that dataset and then add to a data structure
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();

                repo.Reload(dataStructure);
                repo.LoadIfNot(dataStructure.Views);
                int count = (from v in dataStructure.Views
                             where v.Id.Equals(view.Id)
                             select v
                            )
                            .Count();

                if (count > 0)
                    throw new Exception(string.Format("There is a connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

                dataStructure.Views.Add(view);
                view.DataStructures.Add(dataStructure);

                repo.Put(dataStructure);
                uow.Commit();
            }
        }

        /// <summary>
        /// Removes the relationship between the structured data structure and the view, neither the data structure nor the view.
        /// </summary>
        /// <param name="dataStructure">The data structure to be release from the relationship.</param>
        /// <param name="view">The view to be release from the relationship.</param>
        public void RemoveDataView(BExIS.Dlm.Entities.DataStructure.StructuredDataStructure dataStructure, DatasetView view)
        {
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<StructuredDataStructure>() != null && Contract.Result<StructuredDataStructure>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<StructuredDataStructure> repo = uow.GetRepository<StructuredDataStructure>();
                repo.Reload(dataStructure);
                repo.LoadIfNot(dataStructure.Views);
                int count = (from v in dataStructure.Views
                             where v.Id.Equals(view.Id)
                             select v
                            )
                            .Count();

                if (count <= 0)
                    throw new Exception(string.Format("There is no connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

                dataStructure.Views.Remove(view);
                view.DataStructures.Remove(dataStructure);

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
        public void RemoveDataView(BExIS.Dlm.Entities.DataStructure.UnStructuredDataStructure dataStructure, DatasetView view)
        {
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);
            //Contract.Ensures(Contract.Result<UnStructuredDataStructure>() != null && Contract.Result<UnStructuredDataStructure>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<UnStructuredDataStructure> repo = uow.GetRepository<UnStructuredDataStructure>();
                repo.Reload(dataStructure);
                repo.LoadIfNot(dataStructure.Views);
                int count = (from v in dataStructure.Views
                             where v.Id.Equals(view.Id)
                             select v
                            )
                            .Count();

                if (count <= 0)
                    throw new Exception(string.Format("There is no connection between data structure {0} and view {1}", dataStructure.Id, view.Id));

                dataStructure.Views.Remove(view);
                view.DataStructures.Remove(dataStructure);

                repo.Put(dataStructure);
                uow.Commit();
            }
            //throw new NotImplementedException();
        }

        #endregion

    }
}
