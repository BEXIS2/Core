using BExIS.Dlm.Entities.DataStructure;
using System;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    /// <summary>
    /// DataStructureManager class is responsible for CRUD (Create, Read, Update, Delete) operations on the aggregate area of the data structure.
    /// The data structure aggregate area is a set of entities like <see cref="DataStructure"/>, <see cref="VariableUsage"/>, and <see cref="ParameterUsage"/> that in 
    /// cooperation together can materialize the formal specification of the structure of group of datasets.
    /// </summary>
    public class VariableManager : IDisposable
    {

        private IUnitOfWork guow = null;
        public VariableManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.VariableTemplateRepo = guow.GetReadOnlyRepository<VariableTemplate>();
            this.VariableInstanceRepo = guow.GetReadOnlyRepository<VariableInstance>();

        }

        private bool isDisposed = false;
        ~VariableManager()
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
        public IReadOnlyRepository<VariableInstance> VariableInstanceRepo { get; private set; }

        /// <summary>
        /// Provides read-only querying and access to unstructured data structures
        /// </summary>
        public IReadOnlyRepository<VariableTemplate> VariableTemplateRepo { get; private set; }


        #endregion


        #region VariableTemplate

        // get
        /// <summary>
        /// Get Variable Template by id from database, if exist
        /// </summary>
        /// <param name="id"></param>
        /// <returns>VariableTemplate</returns>
        public VariableTemplate GetVariableTemplate(long id)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableTemplate> repo = uow.GetRepository<VariableTemplate>();
                var e = repo.Get(id);

                return e;
            }
        }

        // create
        /// <summary>
        /// Create an entry in the database.
        /// name and datatype is required
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="unit"></param>
        /// <param name="isOptional"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <returns>VariableTemplate</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public VariableTemplate CreateVariableTemplate(string name, DataType dataType, Unit unit, string description="",  string defaultValue="")
        {
            // check incoming varaibles
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Name is empty but is required.");
            if (dataType == null) throw new ArgumentNullException(nameof(dataType), "DataType is null but is required.");

            VariableTemplate e = new VariableTemplate()
            {
                Label = name,
                Description = description,
                DataType = dataType,
                DefaultValue = defaultValue,
                Unit = unit
              };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableTemplate> repo = uow.GetRepository<VariableTemplate>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        // update

        public VariableTemplate UpdateVariableTemplate(VariableTemplate entity)
        {
            if (entity == null) throw new ArgumentException("entity must not be null.");
            if (entity.Id <= 0) throw new ArgumentException("id must be greater then 0.");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableTemplate> repo = uow.GetRepository<VariableTemplate>();
                var merged = repo.Get(entity.Id);
                
                merged.Approved = entity.Approved;
                merged.Description = entity.Description;
                merged.DataType = entity.DataType;
                merged.DefaultValue = entity.DefaultValue;
                merged.Unit = entity.Unit;
                merged.MissingValues = entity.MissingValues;
                merged.DisplayPatternId = entity.DisplayPatternId;
                merged.IsValueOptional = entity.IsValueOptional;
                merged.VariableConstraints = entity.VariableConstraints;
                merged.Label = entity.Label;
                merged.MinCardinality = entity.MinCardinality;
                merged.MaxCardinality = entity.MaxCardinality;

                repo.Put(merged);
                uow.Commit();
            }

            return (entity);
        }

        // delete
        public bool DeleteVariableTemplate(long id)
        {

            if (id <= 0) throw new ArgumentException("Id must be greater then 0.", nameof(id));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableTemplate> repo = uow.GetRepository<VariableTemplate>();

                var entity = repo.Get(id);
                repo.Delete(entity);

                uow.Commit();
            }

            return (true);
        }

        // add constraint
        // remove constraint

        #endregion

        #region VariableInstance

        // get
        public VariableInstance GetVariable(long id)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableInstance> repo = uow.GetRepository<VariableInstance>();
                var e = repo.Get(id);

                return e;
            }
        }

        // create - need datastucture, variable template 
        /// <summary>
        /// create a Variable Instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataStructureId"></param>
        /// <param name="variableTemplateId"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <returns>created VariableInstance</returns>
        /// <exception cref="ArgumentException"></exception>
        public VariableInstance CreateVariable(string name, long dataStructureId, long variableTemplateId, int displayPatternId = -1)
        {
            // check incoming varaibles
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "name is empty but is required.");
            if (dataStructureId <= 0) throw new ArgumentNullException(nameof(dataStructureId), "dataStructureId must be greater then 0.");
            if (variableTemplateId <= 0) throw new ArgumentNullException(nameof(variableTemplateId), "variableTemplateId must be greater then 0.");





            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableInstance> repo = uow.GetRepository<VariableInstance>();
                IRepository<VariableTemplate> variableTemplateRepo = uow.GetRepository<VariableTemplate>();
                IRepository<StructuredDataStructure> datastructureRepo = uow.GetRepository<StructuredDataStructure>();

                var datastructure = datastructureRepo.Get(dataStructureId);
                var variableTemplate = variableTemplateRepo.Get(variableTemplateId);

                VariableInstance e = new VariableInstance()
                {
                    Label = name,
                    DataStructure = datastructure,
                    VariableTemplate = variableTemplate,
                    Description = variableTemplate.Description,
                    DataType = variableTemplate.DataType,
                    Unit = variableTemplate.Unit,
                    DefaultValue = variableTemplate.DefaultValue,
                    IsValueOptional = variableTemplate.IsValueOptional,
                    DisplayPatternId = displayPatternId
                };

                // add variable to datastructure
                datastructure.Variables.Add(e);

                repo.Put(e);
                uow.Commit();

                return (e);
            }

        }



        // create - need datastucture, variable template 
        /// <summary>
        /// create a Variable Instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="unit"></param>
        /// <param name="dataStructureId"></param>
        /// <param name="isOptional"></param>
        /// <param name="isKey"></param>
        /// <param name="variableTemplateId"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        /// <param name="displayPatternId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public VariableInstance CreateVariable(string name, DataType dataType, Unit unit, long dataStructureId, bool isOptional, bool isKey, int orderNo, long variableTemplateId = 0, string description = "", string defaultValue = "",int displayPatternId = 0)
        {
            // check incoming varaibles
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "name is empty but is required.");
            if (dataType == null) throw new ArgumentNullException(nameof(dataType), "datatype is null but is required.");
            if (dataStructureId <= 0) throw new ArgumentNullException(nameof(dataStructureId),"dataStructureId must be greater then 0.");
            //if (variableTemplateId <= 0) throw new ArgumentNullException(nameof(variableTemplateId), "variableTemplateId must be greater then 0.");



            VariableInstance e = new VariableInstance()
            {
                Label = name,
                Description = description,
                DataType = dataType,
                DefaultValue = defaultValue,
                Unit = unit,
                IsValueOptional = isOptional,
                IsKey = isKey,
                DisplayPatternId = displayPatternId,
                OrderNo = orderNo
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableInstance> repo = uow.GetRepository<VariableInstance>();
                IRepository<VariableTemplate> variableTemplateRepo = uow.GetRepository<VariableTemplate>();
                IRepository<StructuredDataStructure> datastructureRepo = uow.GetRepository<StructuredDataStructure>();

                var datastructure = datastructureRepo.Get(dataStructureId);
                var variableTemplate = variableTemplateRepo.Get(variableTemplateId);

                e.DataStructure = datastructure;
                e.VariableTemplate = variableTemplate;

                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        // update
        /// <summary>
        /// Update a incoming VariableInstance into the database
        /// </summary>
        /// <param name="entity">entity with new values</param>
        /// <returns>Updated Variable Instance</returns>
        /// <exception cref="ArgumentException"></exception>
        public VariableInstance UpdateVariable(VariableInstance entity)
        {
            if (entity == null) throw new ArgumentException("entity must not be null.");
            if (entity.Id <= 0) throw new ArgumentException("id must be greater then 0.");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableInstance> repo = uow.GetRepository<VariableInstance>();
                repo.Merge(entity);

                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return (entity);
        }

        // delete
        /// <summary>
        /// Delete a Variable Instance based on the incoming id
        /// </summary>
        /// <param name="id">Variable Instance Id</param>
        /// <returns>true if is deleted</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool DeleteVariable(long id)
        {

            if (id <= 0) throw new ArgumentException("Id must be greater then 0.", nameof(id));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableInstance> repo = uow.GetRepository<VariableInstance>();

                var entity = repo.Get(id);
                repo.Delete(entity);

                uow.Commit();
            }

            return (true);
        }

        // add constraint
        // remove constraint

        #endregion

    }


}
