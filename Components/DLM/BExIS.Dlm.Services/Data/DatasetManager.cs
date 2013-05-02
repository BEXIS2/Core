using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Persistence.Api;
using BExIS.Dlm.Entities.Data;
using System.Xml;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Dlm.Services.Data
{
    public class DatasetManager
    {
        public DatasetManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.DatasetRepo = uow.GetReadOnlyRepository<Dataset>();
            this.DataTupleRepo = uow.GetReadOnlyRepository<DataTuple>();
            this.ExtendedPropertyValueRepo = uow.GetReadOnlyRepository<ExtendedPropertyValue>();
            this.VariableValueRepo = uow.GetReadOnlyRepository<VariableValue>();
            this.ParameterValueRepo = uow.GetReadOnlyRepository<ParameterValue>();
            this.AmendmentRepo = uow.GetReadOnlyRepository<Amendment>();
        }

        #region Data Readers

        // provide read only repos for the whole aggregate area
        public IReadOnlyRepository<Dataset> DatasetRepo { get; private set; }
        public IReadOnlyRepository<DataTuple> DataTupleRepo { get; private set; }
        public IReadOnlyRepository<ExtendedPropertyValue> ExtendedPropertyValueRepo { get; private set; }
        public IReadOnlyRepository<VariableValue> VariableValueRepo { get; private set; }
        public IReadOnlyRepository<ParameterValue> ParameterValueRepo { get; private set; }
        public IReadOnlyRepository<Amendment> AmendmentRepo { get; private set; }

        #endregion

        #region Dataset

        public Dataset GetDataset(Int64 datasetId)
        {
            Dataset ds = DatasetRepo.Get(datasetId);
            if(ds != null)
                ds.Materialize();
            return (ds);
        }

        public Dataset CreateDataset(Dataset dataset)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(dataset.Title));
            Contract.Requires(dataset.DataStructure != null && dataset.DataStructure.Id >= 0);

            Contract.Ensures(Contract.Result<Dataset>() != null && Contract.Result<Dataset>().Id >= 0);

            dataset.DataStructure.Datasets.Add(dataset);
            dataset.ExtendedPropertyValues.ToList().ForEach(ex => ex.Dataset = dataset);
            dataset.ContentDescriptors.ToList().ForEach(ex => ex.Dataset = dataset);
            dataset.Tuples.ToList().ForEach(ex => ex.Dataset = dataset);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                repo.Put(dataset);
                uow.Commit();
            }
            return (dataset);
        }

        public Dataset CreateDataset(string title, string description, XmlDocument metadata,
            ICollection<DataTuple> tuples, ICollection<ExtendedPropertyValue> extendedPropertyValues, ICollection<ContentDescriptor> contentDescriptors
            , BExIS.Dlm.Entities.DataStructure.DataStructure dataStructure)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(title));
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);

            Contract.Ensures(Contract.Result<Dataset>() != null && Contract.Result<Dataset>().Id >= 0);
            Dataset e = new Dataset()
            {
                Title = title,
                Description = description,
                Metadata = metadata, // maybe detachnig the document is necessary!
                Tuples = new List<DataTuple>(tuples),
                ExtendedPropertyValues = new List<ExtendedPropertyValue>(extendedPropertyValues),
                ContentDescriptors = new List<ContentDescriptor>(contentDescriptors),
                DataStructure= dataStructure,
            };

            e.DataStructure.Datasets.Add(e);
            e.ExtendedPropertyValues.ToList().ForEach(ex => ex.Dataset = e);
            e.ContentDescriptors.ToList().ForEach(ex => ex.Dataset = e);
            e.Tuples.ToList().ForEach(ex => ex.Dataset = e);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);            
        }

        public bool DeleteDataset(Dataset entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                IRepository<ExtendedPropertyValue> exRepo = uow.GetRepository<ExtendedPropertyValue>();
                IRepository<DataTuple> tuRepo = uow.GetRepository<DataTuple>();

                entity = repo.Reload(entity);
                repo.LoadIfNot(entity.Tuples);
                repo.LoadIfNot(entity.ExtendedPropertyValues);
                
                exRepo.Delete(entity.ExtendedPropertyValues);
                entity.ExtendedPropertyValues.Clear();

                tuRepo.Delete(entity.Tuples);
                entity.Tuples.Clear();

                entity.DataStructure = null;
                
                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool DeleteDataset(IEnumerable<Dataset> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (Dataset e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (Dataset e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                IRepository<ExtendedPropertyValue> exRepo = uow.GetRepository<ExtendedPropertyValue>();
                IRepository<DataTuple> tuRepo = uow.GetRepository<DataTuple>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.LoadIfNot(latest.Tuples);
                    repo.LoadIfNot(latest.ExtendedPropertyValues);

                    exRepo.Delete(latest.ExtendedPropertyValues);
                    latest.ExtendedPropertyValues.Clear();

                    tuRepo.Delete(latest.Tuples);
                    latest.Tuples.Clear();

                    latest.DataStructure = null;

                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public Dataset UpdateDataset(Dataset entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<Dataset>() != null && Contract.Result<Dataset>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);    
        }

        #endregion

        #region DataTuple      

        public DataTuple CreateDataTuple(int orderNo, ICollection<VariableValue> variableValues, ICollection<Amendment> amendments, Dataset dataset)
        {
            //Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(dataset != null);

            Contract.Ensures(Contract.Result<DataTuple>() != null && Contract.Result<DataTuple>().Id >= 0);
            DataTuple e = new DataTuple()
            {
                OrderNo = orderNo,
                Dataset = dataset,
                VariableValues = new List<VariableValue>(variableValues),
                Amendments= new List<Amendment>(amendments),
            };
            e.Dataset.Tuples.Add(e);
            e.Amendments.ToList().ForEach(ex => ex.Tuple = e);
            //e.VariableValues.ToList().ForEach(ex => ex.Tuple = e);

            // check to see if all variable values and thier paramater values are defined in the data strcuture
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataTuple> repo = uow.GetRepository<DataTuple>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool DeleteDataTuple(DataTuple entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataTuple> repo = uow.GetRepository<DataTuple>();

                entity = repo.Reload(entity);
                entity.Dataset = null;

                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool DeleteDataTuple(IEnumerable<DataTuple> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (DataTuple e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (DataTuple e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataTuple> repo = uow.GetRepository<DataTuple>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    latest.Dataset = null;

                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public DataTuple UpdateDataTuple(DataTuple entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<DataTuple>() != null && Contract.Result<DataTuple>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataTuple> repo = uow.GetRepository<DataTuple>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }
        
        #endregion

        // the Classes derived from DataValue are not independent persistence classes. They get persisted with their containers, So there is no need for Delete and update, 
        // e.g., tuple1.Amendments.First().Value = 10, UpdateTuple(tuple1);

        #region Extended Property Value

        public ExtendedPropertyValue CreateExtendedPropertyValue(string value, string note, DateTime samplingTime, DateTime resultTime, ObtainingMethod obtainingMethod, Int64 extendedPropertyId, Dataset dataset)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));
            Contract.Requires(extendedPropertyId > 0);
            Contract.Requires(dataset != null);

            Contract.Ensures(Contract.Result<ExtendedPropertyValue>() != null);
            ExtendedPropertyValue e = new ExtendedPropertyValue()
            {
                Value = value,
                Note = note,
                SamplingTime = samplingTime,
                ResultTime = resultTime,
                ObtainingMethod = obtainingMethod,
                ExtendedPropertyId = extendedPropertyId,
                Dataset = dataset, // subject to delete
            };
            e.Dataset.ExtendedPropertyValues.Add(e);

            //using (IUnitOfWork uow = this.GetUnitOfWork())
            //{
            //    IRepository<ExtendedPropertyValue> repo = uow.GetRepository<ExtendedPropertyValue>();
            //    repo.Put(e);
            //    uow.Commit();
            //}
            return (e);
        }

        #endregion

        #region Parameter Value

        public Amendment CreateAmendment(string value, string note, DateTime samplingTime, DateTime resultTime, ObtainingMethod obtainingMethod, Int64 parameterId, DataTuple tuple)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));
            Contract.Requires(parameterId > 0);
            Contract.Requires(tuple != null);
            Contract.Ensures(Contract.Result<Amendment>() != null);

            Amendment e = new Amendment()
            {
                Value = value,
                Note = note,
                SamplingTime = samplingTime,
                ResultTime = resultTime,
                ObtainingMethod = obtainingMethod,
                ParameterId = parameterId,     
                Tuple = tuple,
            };

            //using (IUnitOfWork uow = this.GetUnitOfWork())
            //{
            //    IRepository<Amendment> repo = uow.GetRepository<Amendment>();
            //    repo.Put(e);
            //    uow.Commit();
            //}
            return (e);
        }

        #endregion

        #region Variable Value

        public VariableValue CreateVariableValue(string value, string note, DateTime samplingTime, DateTime resultTime, ObtainingMethod obtainingMethod, Int64 variableId, ICollection<ParameterValue> parameterValues)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));
            Contract.Requires(variableId > 0);
            Contract.Ensures(Contract.Result<VariableValue>() != null);

            VariableValue e = new VariableValue()
            {
                Value = value,
                Note = note,
                SamplingTime = samplingTime,
                ResultTime = resultTime,
                ObtainingMethod = obtainingMethod,
                VariableId = variableId,
                ParameterValues = new List<ParameterValue>(parameterValues),
            };

            //using (IUnitOfWork uow = this.GetUnitOfWork())
            //{
            //    IRepository<VariableValue> repo = uow.GetRepository<VariableValue>();
            //    repo.Put(e);
            //    uow.Commit();
            //}
            return (e);
        }

        #endregion

        #region Parameter Value

        public ParameterValue CreateParameterValue(string value, string note, DateTime samplingTime, DateTime resultTime, ObtainingMethod obtainingMethod, Int64 parameterId)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));
            Contract.Requires(parameterId > 0);
            Contract.Ensures(Contract.Result<ParameterValue>() != null);

            ParameterValue e = new ParameterValue()
            {
                Value = value,
                Note = note,
                SamplingTime = samplingTime,
                ResultTime = resultTime,
                ObtainingMethod = obtainingMethod,
                ParameterId = parameterId,
            };

            //using (IUnitOfWork uow = this.GetUnitOfWork())
            //{
            //    IRepository<ParameterValue> repo = uow.GetRepository<ParameterValue>();
            //    repo.Put(e);
            //    uow.Commit();
            //}
            return (e);
        }

        #endregion

        #region Content Descriptor

        public ContentDescriptor CreateContentDescriptor(string name, string mimeType, string uri, Int32 orderNo, Dataset dataset)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(mimeType));
            Contract.Requires(!string.IsNullOrWhiteSpace(uri));
            Contract.Requires(dataset != null);
            Contract.Ensures(Contract.Result<ContentDescriptor>() != null);

            ContentDescriptor e = new ContentDescriptor()
            {
                Name = name,
                MimeType = mimeType,
                OrderNo = orderNo,
                URI = uri,                
                Dataset = dataset,
            };
            e.Dataset.ContentDescriptors.Add(e);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ContentDescriptor> repo = uow.GetRepository<ContentDescriptor>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool DeleteContentDescriptor(ContentDescriptor entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ContentDescriptor> repo = uow.GetRepository<ContentDescriptor>();

                entity = repo.Reload(entity);
                entity.Dataset = null;

                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool DeleteContentDescriptor(IEnumerable<ContentDescriptor> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (ContentDescriptor e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (ContentDescriptor e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ContentDescriptor> repo = uow.GetRepository<ContentDescriptor>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    latest.Dataset = null;

                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public ContentDescriptor UpdateContentDescriptor(ContentDescriptor entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<ContentDescriptor>() != null && Contract.Result<ContentDescriptor>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<ContentDescriptor> repo = uow.GetRepository<ContentDescriptor>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }
      
        #endregion

        #region Associations

        // there is no need for RemoveDataView as it is equal to DeleteDataView. DataView must be associated with a dataset or some datastructures but not both
        // if you like to promote a view from a dataset to a datastructure, set its Dataset property to null and send it to DataStructureManager.AddDataView

        public void AddDataView(Dataset dataset, DataView view)
        {
            Contract.Requires(dataset != null );
            Contract.Requires(view != null && view.Id >= 0);
            Contract.Requires(view.Dataset == null);

            DatasetRepo.Reload(dataset);
            DatasetRepo.LoadIfNot(dataset.Views);
            int count = (from v in dataset.Views
                         where v.Id.Equals(view.Id)
                         select v
                        )
                        .Count();

            if (count > 0)
                throw new Exception(string.Format("There is a connection between dataset {0} and view {1}", dataset.Id, view.Id));

            dataset.Views.Add(view);
            view.Dataset = dataset;
            view.DataStructures.Clear();

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                // save the relation controller object which is the 1 side in 1:N relationships. in this case: View
                IRepository<DataView> repo = uow.GetRepository<DataView>();
                repo.Put(view);
                uow.Commit();
            }
        }
       
        #endregion
    }
}
