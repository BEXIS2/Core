using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Persistence.Api;
using BExIS.Dlm.Entities.Data;
using System.Xml;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities;
using BExIS.Dlm.Entities.Administration;

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
        public IReadOnlyRepository<DatasetVersion> DatasetVersionRepo { get; private set; }
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
            //if(ds != null)
            //    ds.Materialize();
            return (ds);
        }

        public Dataset CreateEmptyDataset(Dataset dataset)
        {
            Contract.Requires(dataset != null);
            Contract.Requires(dataset.DataStructure != null && dataset.DataStructure.Id >= 0);

            Contract.Ensures(Contract.Result<Dataset>() != null && Contract.Result<Dataset>().Id >= 0);

            dataset.DataStructure.Datasets.Add(dataset);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                repo.Put(dataset);
                uow.Commit();
            }
            return (dataset);
        }

        /// <summary>
        /// In case some the dataset's attributes are changed, data set is bound to a research plan and so on, use this function
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public Dataset UpdateDataset(Dataset dataset)
        {
            Contract.Requires(dataset != null);
            Contract.Requires(dataset.Id >= 0);

            Contract.Ensures(Contract.Result<Dataset>() != null && Contract.Result<Dataset>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                repo.Put(dataset);
                uow.Commit();
            }
            return (dataset);
        }

        /// <summary>
        /// Checks out the dataset in order to make it available for edit! edit means the possibility to add a new version.
        /// dataset must be in CheckedIn status
        /// </summary>
        /// <param name="datasetId"></param>
        public void CheckOutDataset(Int64 datasetId, string userName)
        {
            checkOutDataset(datasetId, userName);
        }

        /// <summary>
        /// approves the working copy version as a new version and changes the status of the dataset to CheckedIn.
        /// The status must be in CheckedOut and the user must be similar to the checkout user
        /// </summary>
        /// <param name="datasetId"></param>
        public void CheckInDataset(Int64 datasetId, string comment, string userName)
        {
            checkInDataset(datasetId, comment, userName, false);
        }

        /// <summary>
        /// rolls back all the changes done on the latest version (deletes the working copy changes) and takes the dataset back to CheckedIn state
        /// The dataset must be in CheckedOut state and the performing user should be the check out user.
        /// </summary>
        /// <param name="datasetId"></param>
        public void RollbackDataset(Int64 datasetId, string userName)
        {
            rollbackDataset(datasetId, userName, false);
        }

        public bool DeleteDataset(Dataset entity, string userName)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();

                entity = repo.Reload(entity);
                if (entity.Status == DatasetStatus.Deleted)
                    return false;
                /// the dataset must be in CheckedIn state to be deleted
                /// so if it is checked out, the checkout version (working copy) is removed first
                if (entity.Status == DatasetStatus.CheckedOut)
                {
                    this.rollbackDataset(entity.Id, userName, false);
                    entity = repo.Reload(entity);
                }
                entity.Status = DatasetStatus.Deleted;
                repo.Put(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool DeleteDataset(IEnumerable<Dataset> entities, string userName)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (Dataset e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (Dataset e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    /// the dataset must be in CheckedIn state to be deleted
                    /// so if it is checked out, the checkout version (working copy) is removed first
                    if (latest.Status == DatasetStatus.CheckedOut)
                    {
                        this.rollbackDataset(entity.Id, userName, false, true); // the commit is better to be false!
                        latest = repo.Reload(latest);
                    }
                    latest.Status = DatasetStatus.Deleted;
                    repo.Put(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        //public Dataset UpdateDataset(Dataset entity)
        //{
        //    Contract.Requires(entity != null, "provided entity can not be null");
        //    Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

        //    Contract.Ensures(Contract.Result<Dataset>() != null && Contract.Result<Dataset>().Id >= 0, "No entity is persisted!");

        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<Dataset> repo = uow.GetRepository<Dataset>();
        //        repo.Put(entity); // Merge is required here!!!!
        //        uow.Commit();
        //    }
        //    return (entity);    
        //}

        #endregion

        #region DatasetVersion

        public DatasetVersion GetDatasetLatestVersion(Int64 datasetId)
        {
            Dataset ds = DatasetRepo.Get(datasetId); // it would be nice to not fetch the dataset!
            if (ds.Status == DatasetStatus.Deleted)
                throw new Exception(string.Format("Dataset {0} is deleted", datasetId));
            if (ds.Status == DatasetStatus.CheckedIn)
                return (getDatasetLatestVersion(ds));
            else if (ds.Status == DatasetStatus.CheckedOut)
            {
                Int64 latestVersionId = ds.Versions.OrderBy(t => t.Timestamp).Last().Id;
                return (getSpecificDatasetVersion(ds, latestVersionId));
            }
            return (null);
        }

        public DatasetVersion GetDatasetVersion(Int64 datasetId, Int64 versionId)
        {
            /// check whether the version id is in fact the latest? the latest checked in version should be returned. if dataset is checked out, the latest stored version is hidden yet.
            /// If the dataset is marked as deleted its like that it is not there at all
            /// get the latest version from the Versions property, or run a direct query on the db
            /// get the latest version by querying Tuples table for records with version <= latest version

            Dataset ds = DatasetRepo.Get(datasetId); // it would be nice to not fetch the dataset!
            if (ds.Status == DatasetStatus.Deleted)
                throw new Exception(string.Format("Dataset {0} is deleted", datasetId));
            Int64 latestVersionId = ds.Versions.OrderBy(t => t.Timestamp).Last().Id;
            if(latestVersionId.Equals(versionId) && ds.Status == DatasetStatus.CheckedOut) // its a request for the working copy which is hidden
                throw new Exception(string.Format("Invalid version request. The version {0} points to the working copy!", versionId));
            DatasetVersion dsv = getSpecificDatasetVersion(ds, versionId);
            return (dsv);
        }

        /// <summary>
        /// report what has been done by this version. deletes, updates, new records and changes in the dataset attributes
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public DatasetVersion GetDatasetVersionProfile(Int64 datasetId, Int64 versionId)
        {
            /// get the latest version from the Versions property, or run a direct query on the db
            /// get the latest version by querying Tuples table for records with version <= latest version
            /// 
            return null;
        }

        //public DatasetVersion CreateDatasetVersion(Dataset dataset, XmlDocument metadata,
        //    ICollection<DataTuple> createdTuples, ICollection<DataTuple> editedTuples, ICollection<DataTuple> deletedTuples, ICollection<DataTuple> unchangedTuples,
        //    ICollection<ExtendedPropertyValue> extendedPropertyValues, ICollection<ContentDescriptor> contentDescriptors
        //    )
        //{
        //    Contract.Requires(dataset != null && dataset.Id >= 0);
        //    Contract.Requires(dataset.Status == DatasetStatus.CheckedOut);
        //    // check the checkoutUser

        //    Contract.Ensures(Contract.Result<DatasetVersion>() != null && Contract.Result<DatasetVersion>().Id >= 0);

        //    if (dataset.Versions.Max(p => p.Timestamp) >= dataset.LastCheckIOTimestamp)
        //        throw new Exception(string.Format("The current checked out dataset {0} already has a working"));

        //    DateTime timestamp = DateTime.UtcNow;
        //    DatasetVersion dsNewVersion = new DatasetVersion()
        //    {
        //        Timestamp = timestamp,
        //        Metadata = metadata,
        //        ExtendedPropertyValues = new List<ExtendedPropertyValue>(extendedPropertyValues),
        //        ContentDescriptors = new List<ContentDescriptor>(contentDescriptors),
        //    };

        //    ((List<DataTuple>)dsNewVersion.ExtendedPropertyValues).ForEach(ex => ex.DatasetVersion = dsNewVersion);
        //    ((List<DataTuple>)dsNewVersion.ContentDescriptors).ForEach(ex => ex.DatasetVersion = dsNewVersion);
        //    ((List<DataTuple>)dsNewVersion.PriliminaryTuples).ForEach(ex => ex.DatasetVersion = dsNewVersion);

        //    dsNewVersion = applyTupleChanges(dsNewVersion, createdTuples, editedTuples, deletedTuples, unchangedTuples);

        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<DatasetVersion> repo = uow.GetRepository<DatasetVersion>();
        //        repo.Put(dsNewVersion);
        //        uow.Commit();
        //    }

        //    return (dsNewVersion);

        //}

        /// <summary>
        /// there is no need to pass metadata, extendedPropertyValues, contentDescriptors .. as they can be assigned to the version before sending it to this editing method
        /// The general procedure is CheckOut, Edit*, CheckIn or Rollback
        /// While the dataset is checked out, all the changes go to the latest+1 version which acts like a working copy
        /// </summary>
        /// <param name="workingCoptDatasetVersion"></param>
        /// <param name="createdTuples"></param>
        /// <param name="editedTuples"></param>
        /// <param name="deletedTuples"></param>
        /// <param name="unchangedTuples"></param>
        /// <returns></returns>
        public DatasetVersion EditDatasetVersion(DatasetVersion workingCoptDatasetVersion,
            ICollection<DataTuple> createdTuples, ICollection<DataTuple> editedTuples, ICollection<DataTuple> deletedTuples, ICollection<DataTuple> unchangedTuples
            //,ICollection<ExtendedPropertyValue> extendedPropertyValues, ICollection<ContentDescriptor> contentDescriptors
            )
        {
            Contract.Requires(workingCoptDatasetVersion.Dataset != null && workingCoptDatasetVersion.Dataset.Id >= 0);
            Contract.Requires(workingCoptDatasetVersion.Dataset.Status == DatasetStatus.CheckedOut);

            Contract.Ensures(Contract.Result<DatasetVersion>() != null && Contract.Result<DatasetVersion>().Id >= 0);

            // be sure you are working on the latest version (working copy). applyTupleChanges takes the working copy from the DB            
            DatasetVersion edited = applyTupleChanges(workingCoptDatasetVersion, createdTuples, editedTuples, deletedTuples, unchangedTuples);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DatasetVersion> repo = uow.GetRepository<DatasetVersion>();
                // check whether the changes to the latest version, which is changed in the applyTupleChanges , are committed too!
                repo.Put(edited);
                uow.Commit();
            }

            return (edited);
        }

        #endregion

        #region Private Methods

        private DatasetVersion getSpecificDatasetVersion(Int64 datasetId, Int64 versionId)
        {
            Dataset ds = DatasetRepo.Get(datasetId);
            return (getSpecificDatasetVersion(ds, versionId));
        }

        private DatasetVersion getSpecificDatasetVersion(Dataset dataset, Int64 versionId)
        {
            //if(ds != null)
            //    ds.Materialize();
            return null;
        }

        private DatasetVersion getDatasetLatestVersion(Int64 datasetId)
        {
            Dataset ds = DatasetRepo.Get(datasetId);
            return (getDatasetLatestVersion(ds));
        }

        private DatasetVersion getDatasetLatestVersion(Dataset dataset)
        {
            /// the latest checked in version should be returned.
            /// if dataset is checked out, the latest stored version is hidden yet as it behaves like a working copy.
            /// so if dataset is checked in, get latest version bur returning whatever is in the tuples table from the requested version and before
            /// if its checked out get a version before the latest Versions.OrderByDesc(Timestamp),Skip(1).Take(1)/ 
            /// in case the user has asked for the latest version while the dataset is checked out, the new and unchanged tuples are in the Tuples table but deleted and changed one should be retrieved from the TupleVersions table
            /// Take care about this also in GetLatestVersion public method.
            /// If the dataset is marked as deleted its like that it is not there at all
            /// get the latest version from the Versions property, or run a direct query on the db
            /// get the latest version by querying Tuples table for records with version <= latest version
            /// 
            //if(ds != null)
            //    ds.Materialize();
            if (dataset.Status == DatasetStatus.CheckedIn)
            {
                Int64 latestVersionId = dataset.Versions.OrderBy(t => t.Timestamp).Last().Id;
                return (getSpecificDatasetVersion(dataset, latestVersionId));
            }
            return null;
        }
        
        private DatasetVersion getDatasetWorkingCopyVersion(Int64 datasetId)
        {
            Dataset ds = DatasetRepo.Get(datasetId);
            return (getDatasetWorkingCopyVersion(ds));
        }

        private DatasetVersion getDatasetWorkingCopyVersion(Dataset dataset)
        {
            if (dataset.Status == DatasetStatus.CheckedOut)
            {
                Int64 latestVersionId = dataset.Versions.OrderBy(t => t.Timestamp).Last().Id;
                return (getSpecificDatasetVersion(dataset, latestVersionId));
            }
            return null;
        }

        /// <summary>
        /// checks out the dataset and creates a new version on it. the new version acts like a working copy while it is not committed, hence editable.
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="userName"></param>
        private void checkOutDataset(Int64 datasetId, string userName)
        {
            DateTime timestamp = DateTime.UtcNow;
            DatasetVersion dsNewVersion = new DatasetVersion()
            {
                Timestamp = timestamp,
                Metadata = null,
                ExtendedPropertyValues = new List<ExtendedPropertyValue>(),
                ContentDescriptors = new List<ContentDescriptor>(),
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                var q = repo.Query(p => p.Id == datasetId && p.Status == DatasetStatus.CheckedIn && p.CheckOutUser.Equals(string.Empty));
                Dataset ds = q.FirstOrDefault();
                if (ds != null)
                {
                    ds.Status = DatasetStatus.CheckedOut;
                    ds.LastCheckIOTimestamp = timestamp;
                    ds.CheckOutUser = userName;
                    ds.Versions.Add(dsNewVersion);
                    dsNewVersion.Dataset = ds;
                    repo.Put(ds);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="comment"></param>
        /// <param name="adminMode">if true, the check for current user is bypassed</param>
        private void checkInDataset(Int64 datasetId, string comment, string userName, bool adminMode)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                Dataset ds = null;
                if (adminMode)
                    ds = repo.Get(p => p.Id == datasetId && p.Status == DatasetStatus.CheckedOut).FirstOrDefault();
                else
                    ds = repo.Get(p => p.Id == datasetId && p.Status == DatasetStatus.CheckedOut && p.CheckOutUser.Equals(userName)).FirstOrDefault();
                if (ds != null)
                {
                    ds.Status = DatasetStatus.CheckedIn;
                    DatasetVersion dsv = ds.Versions.OrderBy(t => t.Timestamp).Last();
                    dsv.ChangeDescription = comment;
                    ds.LastCheckIOTimestamp = DateTime.UtcNow;
                    ds.CheckOutUser = string.Empty;
                    repo.Put(ds);
                    uow.Commit();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="userName"></param>
        /// <param name="adminMode"></param>
        /// <param name="commit">in some cases, rollback is called on a set of datasets. In  these cases its better to not commit at each rollback, but at the end</param>
        private void rollbackDataset(Int64 datasetId, string userName, bool adminMode, bool commit = true)
        {
            // maybe its required to pass the caller's repo in order to the rollback changes to be visible to the caller function and be able to commit them

            // check for admin mode
        }

        private DatasetVersion applyTupleChanges(DatasetVersion workingCopyVersion,
            ICollection<DataTuple> createdTuples, ICollection<DataTuple> editedTuples, ICollection<DataTuple> deletedTuples, ICollection<DataTuple> unchangedTuples)
        {
            // latest version is the latest checked in version. usually it is the previous version in comparison to the working copy version.
            DatasetVersion latestVersion = getDatasetLatestVersion(workingCopyVersion.Dataset);

            // do nothing with unchanged for now

            /// associate newly created tuples to the new version
            ((List<DataTuple>)workingCopyVersion.PriliminaryTuples).AddRange(createdTuples);
            ((List<DataTuple>)createdTuples)
                .ForEach(p =>
                {
                    p.DatasetVersion = workingCopyVersion;
                    p.TupleAction = TupleAction.Created;
                    p.Timestamp = workingCopyVersion.Timestamp;
                }
               );

            /// manage edited tuples: 
            /// 1: create a DataTupleVersion based on their previous version
            /// 2: Remove them from the latest version
            /// 3: add them to the new version
            /// 4: set timestamp for the edited ones
            /// 
            foreach (var edited in editedTuples)
            {
                DataTuple orginalTuple = latestVersion.EffectiveTuples.Where(p => p.Id == edited.Id).Single();
                DataTupleVersion tupleVersion = new DataTupleVersion()
                {
                    TupleAction = TupleAction.Edited,
                    Extra = orginalTuple.Extra,
                    Id = orginalTuple.Id,
                    OrderNo = orginalTuple.OrderNo,
                    Timestamp = orginalTuple.Timestamp,
                    XmlAmendments = orginalTuple.XmlAmendments,
                    XmlVariableValues = orginalTuple.XmlVariableValues,
                    OriginalTuple = orginalTuple,
                    DatasetVersion = latestVersion,
                    ActingDatasetVersion = workingCopyVersion,
                };
                orginalTuple.DatasetVersion = workingCopyVersion;
                orginalTuple.Timestamp = workingCopyVersion.Timestamp;
                latestVersion.PriliminaryTuples.Remove(orginalTuple);
                latestVersion.EffectiveTuples.Remove(orginalTuple);

            }

            /// manage deleted tuples: 
            /// 1: create a DataTupleVersion based on their previous version
            /// 2: Remove them from the latest version
            /// 3: DO NOT add them to the new version
            /// 4: DO NOT set timestamp for the deleted ones
            /// 
            foreach (var deleted in deletedTuples)
            {
                DataTuple orginalTuple = latestVersion.EffectiveTuples.Where(p => p.Id == deleted.Id).Single();
                DataTupleVersion tupleVersion = new DataTupleVersion()
                {
                    TupleAction = TupleAction.Deleted,
                    Extra = orginalTuple.Extra,
                    Id = orginalTuple.Id,
                    OrderNo = orginalTuple.OrderNo,
                    Timestamp = orginalTuple.Timestamp,
                    XmlAmendments = orginalTuple.XmlAmendments,
                    XmlVariableValues = orginalTuple.XmlVariableValues,
                    OriginalTuple = orginalTuple,
                    DatasetVersion = latestVersion,
                    ActingDatasetVersion = workingCopyVersion,
                };
                latestVersion.PriliminaryTuples.Remove(orginalTuple);
                latestVersion.EffectiveTuples.Remove(orginalTuple);
                orginalTuple.DatasetVersion = null;
            }

            return (workingCopyVersion);
        }

        #endregion

        #region DataTuple

        public DataTuple CreateDataTuple(int orderNo, ICollection<VariableValue> variableValues, ICollection<Amendment> amendments, DatasetVersion datasetVersion)
        {
            //Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(datasetVersion != null);

            Contract.Ensures(Contract.Result<DataTuple>() != null && Contract.Result<DataTuple>().Id >= 0);
            DataTuple e = new DataTuple()
            {
                OrderNo = orderNo,
                DatasetVersion = datasetVersion,
                VariableValues = new List<VariableValue>(variableValues),
                Amendments= new List<Amendment>(amendments),
            };
            e.DatasetVersion.PriliminaryTuples.Add(e);
            e.Amendments.ToList().ForEach(ex => ex.Tuple = e);
            //e.VariableValues.ToList().ForEach(ex => ex.Tuple = e);

            // check to see if all variable values and their parameter values are defined in the data structure
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataTuple> repo = uow.GetRepository<DataTuple>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        [Obsolete("Avoid using!")]
        public bool DeleteDataTuple(DataTuple entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataTuple> repo = uow.GetRepository<DataTuple>();

                entity = repo.Reload(entity);
                entity.DatasetVersion = null;

                repo.Delete(entity);

                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        [Obsolete("Avoid using!")]
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
                    latest.DatasetVersion = null;

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

        public ExtendedPropertyValue CreateExtendedPropertyValue(string value, string note, DateTime samplingTime, DateTime resultTime, ObtainingMethod obtainingMethod,
            Int64 extendedPropertyId, DatasetVersion datasetVersion)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));
            Contract.Requires(extendedPropertyId > 0);
            Contract.Requires(datasetVersion != null);

            Contract.Ensures(Contract.Result<ExtendedPropertyValue>() != null);
            ExtendedPropertyValue e = new ExtendedPropertyValue()
            {
                Value = value,
                Note = note,
                SamplingTime = samplingTime,
                ResultTime = resultTime,
                ObtainingMethod = obtainingMethod,
                ExtendedPropertyId = extendedPropertyId,
                DatasetVersion = datasetVersion, // subject to delete
            };
            e.DatasetVersion.ExtendedPropertyValues.Add(e);

            //using (IUnitOfWork uow = this.GetUnitOfWork())
            //{
            //    IRepository<ExtendedPropertyValue> repo = uow.GetRepository<ExtendedPropertyValue>();
            //    repo.Put(e);
            //    uow.Commit();
            //}
            return (e);
        }

        #endregion

        #region Amendments

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

        public ContentDescriptor CreateContentDescriptor(string name, string mimeType, string uri, Int32 orderNo, DatasetVersion datasetVersion)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(mimeType));
            Contract.Requires(!string.IsNullOrWhiteSpace(uri));
            Contract.Requires(datasetVersion != null);
            Contract.Ensures(Contract.Result<ContentDescriptor>() != null);

            ContentDescriptor e = new ContentDescriptor()
            {
                Name = name,
                MimeType = mimeType,
                OrderNo = orderNo,
                URI = uri,                
                DatasetVersion = datasetVersion,
            };
            e.DatasetVersion.ContentDescriptors.Add(e);

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
                entity.DatasetVersion = null;

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
                    latest.DatasetVersion = null;

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

        public bool DeleteDataView(DataView entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataView> repo = uow.GetRepository<DataView>();

                entity = repo.Reload(entity);
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        #endregion
    }
}
