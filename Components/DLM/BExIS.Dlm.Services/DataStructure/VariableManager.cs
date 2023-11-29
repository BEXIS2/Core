using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public VariableTemplate CreateVariableTemplate(string name, DataType dataType, Unit unit, string description = "", string defaultValue = "", ICollection<Meaning> meanings = null, ICollection<Constraint> constraints = null, bool approved = false)
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
                Unit = unit,
                Approved = approved,
           
            };

            if (meanings != null) e.Meanings = meanings;
            if (constraints != null) e.VariableConstraints = constraints;

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
                merged.Meanings = entity.Meanings;
                merged.VariableConstraints = entity.VariableConstraints;

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

                if (entity?.Meanings.Count > 0)
                {
                    entity.Meanings = new List<Meaning>();
                    repo.Put(entity);
                    uow.Commit();
                }

                repo.Delete(entity);

                uow.Commit();
            }

            return (true);
        }

        // add constraint
        // remove constraint

        // add meanings



        // remove meanings

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
        public VariableInstance CreateVariable(string name, DataType dataType, Unit unit, long dataStructureId, bool isOptional, bool isKey, int orderNo, long variableTemplateId = 0, string description = "", string defaultValue = "",int displayPatternId = 0, List<MissingValue> missingValues = null, List<long> constraints = null)
        {
            // check incoming varaibles
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "name is empty but is required.");
            if (dataType == null) throw new ArgumentNullException(nameof(dataType), "datatype is null but is required.");
            if (dataStructureId <= 0) throw new ArgumentNullException(nameof(dataStructureId),"dataStructureId must be greater then 0.");
            //if (variableTemplateId <= 0) throw new ArgumentNullException(nameof(variableTemplateId), "variableTemplateId must be greater then 0.");

            // update missing values placeholder
            if (missingValues!=null && missingValues.Any())
            {
                missingValues.ForEach(m => {
                    m.Placeholder = getPlaceholder(getTypeCode(dataType.SystemType), missingValues);
                });
            }

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
                OrderNo = orderNo,
                MissingValues = missingValues
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<VariableInstance> repo = uow.GetRepository<VariableInstance>();
                IRepository<VariableTemplate> variableTemplateRepo = uow.GetRepository<VariableTemplate>();
                IRepository<StructuredDataStructure> datastructureRepo = uow.GetRepository<StructuredDataStructure>();
                IRepository<Constraint> constraintRepo = uow.GetRepository<Constraint>();

                var datastructure = datastructureRepo.Get(dataStructureId);
                var variableTemplate = variableTemplateRepo.Get(variableTemplateId);
                var cons = constraintRepo.Query().Where(c => constraints.Contains(c.Id)).ToList();

                e.DataStructure = datastructure;
                e.VariableTemplate = variableTemplate;
                e.VariableConstraints = cons;

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

        #region missingvalues helper

        public string getPlaceholder(TypeCode typeCode, List<MissingValue> missingValues, string format = "")
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {

                switch (typeCode)
                {
                    case TypeCode.Int16:
                        try
                        {
                            List<short> placeholders = missingValues.Select(mv => Convert.ToInt16(mv.Placeholder)).ToList();
                            short temp = short.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > short.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Int32:
                        try
                        {
                            List<Int32> placeholders = missingValues.Select(mv => Convert.ToInt32(mv.Placeholder)).ToList();

                            int temp = int.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > int.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Int64:
                        try
                        {
                            List<long> placeholders = missingValues.Select(mv => Convert.ToInt64(mv.Placeholder)).ToList();
                            long temp = long.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > long.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.UInt16:
                        try
                        {
                            List<ushort> placeholders = missingValues.Select(mv => Convert.ToUInt16(mv.Placeholder)).ToList();
                            ushort temp = ushort.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > ushort.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.UInt32:
                        try
                        {
                            List<uint> placeholders = missingValues.Select(mv => Convert.ToUInt32(mv.Placeholder)).ToList();

                            uint temp = uint.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > uint.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.UInt64:
                        try
                        {
                            List<ulong> placeholders = missingValues.Select(mv => Convert.ToUInt64(mv.Placeholder)).ToList();

                            ulong temp = ulong.MaxValue - 1;
                            while (placeholders.Contains(temp) && temp > ulong.MinValue + 1)
                            {
                                temp--;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Double:
                        try
                        {
                            List<string> placeholders = missingValues.Select(mv => mv.Placeholder).ToList();

                            double temp = double.MaxValue / 10 - 1.0;
                            while (placeholders.Contains(temp.ToString(format)) && temp > double.MinValue / 10 + 1.0)
                            {
                                temp -= temp / 1E+14;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Decimal:
                        try
                        {
                            List<string> placeholders = missingValues.Select(mv => mv.Placeholder).ToList();

                            decimal temp = decimal.MaxValue - (decimal)1.0;
                            while (placeholders.Contains(temp.ToString(format)) && temp > decimal.MinValue + (decimal)1.0)
                            {
                                temp -= (decimal)1.0;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.Single:
                        try
                        {
                            List<float> placeholders = missingValues.Select(mv => Convert.ToSingle(mv.Placeholder)).ToList();

                            float temp = float.MaxValue - (float)1.0;
                            while (placeholders.Contains(temp) && temp > float.MinValue + (float)1.0)
                            {
                                temp -= (float)1.0;
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.DateTime:
                        try
                        {
                            List<string> placeholders = missingValues.Select(mv => mv.Placeholder).ToList();

                            DateTime temp = DateTime.MaxValue.AddHours(-1);
                            while (placeholders.Contains(temp.ToString(format)))
                            {
                                temp = temp.AddHours(-1);
                                temp = temp.AddYears(-1); //Reduce also by 1 year to be able to distinguish placeholder also after application of display pattern e.g. YYYY-MM-DD
                            }
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    case TypeCode.String:
                        try
                        {
                            int temp = DateTime.Now.GetHashCode();
                            return temp.ToString(format);
                        }
                        catch
                        {
                            return null;
                        }

                    default:
                        return null;
                }
            }
        }

        //with this funktion you can check if the Placeholder you want to use can be used
        public bool ValidatePlaceholder(TypeCode typeCode, string placeholder, VariableInstance variable, long missingvalueId = 0)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                List<MissingValue> missingValues = variable.MissingValues.ToList();

                switch (typeCode)
                {
                    case TypeCode.Int16:
                        try
                        {
                            short temp = 0;
                            if (short.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToInt16(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Int32:
                        try
                        {
                            int temp = 0;
                            if (int.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToInt32(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Int64:
                        try
                        {
                            long temp = 0;
                            if (long.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToInt64(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.UInt16:
                        try
                        {
                            ushort temp = 0;
                            if (ushort.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToUInt16(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.UInt32:
                        try
                        {
                            uint temp = 0;
                            if (uint.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToUInt32(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.UInt64:
                        try
                        {
                            ulong temp = 0;
                            if (ulong.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToUInt64(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Double:
                        try
                        {
                            double temp = (double)0.0;
                            if (double.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToDouble(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Decimal:
                        try
                        {
                            decimal temp = (decimal)0.0;
                            if (decimal.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToDecimal(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.Single:
                        try
                        {
                            float temp = (float)0.0;
                            if (float.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && temp == Convert.ToSingle(mv.Placeholder))
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.DateTime:
                        try
                        {
                            DateTime temp = new DateTime();
                            if (DateTime.TryParse(placeholder, out temp))
                            {
                                foreach (MissingValue mv in missingValues)
                                {
                                    if (mv.Id != missingvalueId && placeholder == mv.Placeholder)
                                        return false;
                                }
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }

                    case TypeCode.String:
                        try
                        {
                            foreach (MissingValue mv in missingValues)
                            {
                                if (mv.Id != missingvalueId && placeholder == mv.Placeholder)
                                    return false;
                            }
                            return true;
                        }
                        catch
                        {
                            return false;
                        }

                    default:
                        return false;
                }
            }
        }


        private TypeCode getTypeCode(string systemType)
        {
            foreach (DataTypeCode tc in Enum.GetValues(typeof(DataTypeCode)))
            {
                if (tc.ToString() == systemType)
                {
                    return (TypeCode)tc;
                }
            }

            return TypeCode.String;
        }
        #endregion



    }


}
