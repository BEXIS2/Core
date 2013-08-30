using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
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
            this.DatasetVersionRepo = uow.GetReadOnlyRepository<DatasetVersion>();
            this.DataTupleRepo = uow.GetReadOnlyRepository<DataTuple>();
            this.DataTupleVerionRepo = uow.GetReadOnlyRepository<DataTupleVersion>();
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
        public IReadOnlyRepository<DataTupleVersion> DataTupleVerionRepo { get; private set; }
        public IReadOnlyRepository<ExtendedPropertyValue> ExtendedPropertyValueRepo { get; private set; }
        public IReadOnlyRepository<VariableValue> VariableValueRepo { get; private set; }
        public IReadOnlyRepository<ParameterValue> ParameterValueRepo { get; private set; }
        public IReadOnlyRepository<Amendment> AmendmentRepo { get; private set; }

        #endregion

        #region Dataset

        public bool IsDatasetCheckedOutFor(Int64 datasetId, string userName)
        {
            return ( DatasetRepo.Query(p => p.Status == DatasetStatus.CheckedOut && p.Id == datasetId && p.CheckOutUser == getUserIdentifier(userName)).Count() == 1);
        }

        public bool IsDatasetCheckedIn(Int64 datasetId)
        {
            return (DatasetRepo.Query(p => p.Status == DatasetStatus.CheckedIn && p.Id == datasetId).Count() == 1);
        }
        
        public Dataset GetDataset(Int64 datasetId)
        {
            Dataset ds = DatasetRepo.Get(datasetId);
            //if(ds != null)
            //    ds.Materialize();
            return (ds);
        }

        public Dataset CreateEmptyDataset(Entities.DataStructure.DataStructure dataStructure, ResearchPlan researchPlan)
        {
            Contract.Requires(dataStructure != null && dataStructure.Id >= 0);
            
            Contract.Ensures(Contract.Result<Dataset>() != null && Contract.Result<Dataset>().Id >= 0);
            
            Dataset dataset = new Dataset(dataStructure);
            dataset.ResearchPlan = researchPlan;
            dataset.Status = DatasetStatus.CheckedIn;
            dataset.CheckOutUser = string.Empty;

            dataset.DataStructure.Datasets.Add(dataset);
            researchPlan.Datasets.Add(dataset);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                repo.Put(dataset);
                uow.Commit();
            }
            return (dataset);
        }

        /// <summary>
        /// In cases that the dataset's attributes are changed, data set is bound to a research plan and so on, use this function
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
        public bool CheckOutDataset(Int64 datasetId, string userName)
        {
            return(checkOutDataset(datasetId, userName));
        }

        /// <summary>
        /// approves the working copy version as a new version and changes the status of the dataset to CheckedIn.
        /// The status must be in CheckedOut and the user must be similar to the checkout user.
        /// try preventing simultaneous check-in
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
        public void UndoCheckoutDataset(Int64 datasetId, string userName)
        {
            undoCheckout(datasetId, userName, false);
        }

        public bool DeleteDataset(Int64 datasetId, string userName, bool rollbackCheckout)
        {
            Contract.Requires(datasetId >= 0);

            // do not move them to editDatasetVersion function
            this.DatasetRepo.Evict();
            this.DatasetVersionRepo.Evict();
            this.DataTupleRepo.Evict();
            this.DataTupleVerionRepo.Evict();

            Dataset entity = this.DatasetRepo.Get(datasetId);
            if (entity.Status == DatasetStatus.Deleted)
                return false;
            /// the dataset must be in CheckedIn state to be deleted
            /// so if it is checked out, the checkout version (working copy) is removed first
            if (entity.Status == DatasetStatus.CheckedOut)
            {
                if (rollbackCheckout == true)
                {
                    this.undoCheckout(entity.Id, userName, false);
                }
                else
                {
                    throw new Exception(string.Format("Dataset {0} is in check out state, which prevents it from being deleted. Rollback the changes or check them in and try again", entity.Id));
                }
            }

            // Make an artificial check-out / edit/ check in so that all the data tuples move to the history
            // this movement reduces the amount of tuples in the active tuples table and also marks the dataset as archived upon delete
            checkOutDataset(entity.Id, userName);
            var workingCopy = getDatasetWorkingCopy(entity.Id);
            var tuples = getWorkingCopyTuples(workingCopy);
            workingCopy = editDatasetVersion(workingCopy, null, null, tuples, null); // deletes all the tuples from the active list and moves them to the history table
            checkInDataset(entity.Id, "Dataset is deleted", userName, false);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                entity = repo.Get(datasetId);
                entity.Status = DatasetStatus.Deleted;
                repo.Put(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool PurgeDataset(Int64 datasetId)
        {
            Contract.Requires(datasetId >= 0);

            // Attention: when create and purge or delete tuple are called in one run (one http request) the is a problem with removal of tuples/ version/ dataset because having some reference!!!
            // but if they are called on a single dataset in 2 different http requests, there is no problem!?
            // perhaps the NH session is not flushed completely or has some references to the objects in the caches, as the session end function is not called yet! this is why an Evict before purge is required!

            this.DatasetRepo.Evict();
            this.DatasetVersionRepo.Evict();
            this.DataTupleRepo.Evict();
            this.DataTupleVerionRepo.Evict();

            Dataset entity = this.DatasetRepo.Get(datasetId);
            
            //if (entity.Status != DatasetStatus.Deleted)
            //    return false;

            List<Int64> versionIds = entity.Versions
                           .Select(p => p.Id)
                           .ToList();

            var tupleVersions = (versionIds == null || versionIds.Count() <=0)? null: DataTupleVerionRepo.Get(p => versionIds.Contains(p.DatasetVersion.Id));
            var tuples = (versionIds == null || versionIds.Count() <= 0) ? null : DataTupleRepo.Get(p => versionIds.Contains(p.DatasetVersion.Id));
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                IRepository<DataTupleVersion> repo2 = uow.GetRepository<DataTupleVersion>();
                IRepository<DataTuple> repo3 = uow.GetRepository<DataTuple>();

                if (tupleVersions != null)
                {
                    foreach (var item in tupleVersions)
                    {
                        item.OriginalTuple = null;
                        item.DatasetVersion = null;
                        item.ActingDatasetVersion = null;
                        repo2.Delete(item); // this is not a good solution as it loads all the tuple versions an then deletes them!! a generic solution for bulk delete without loading is needed
                    }
                }
                if (tuples != null)
                {
                    foreach (var tuple in tuples)
                    {
                        try
                        {
                            tuple.History.ToList().ForEach(p => p.OriginalTuple = null);
                            tuple.History.Clear();
                        }
                        catch { }
                        try
                        {
                            tuple.DatasetVersion.PriliminaryTuples.Remove(tuple);
                            tuple.DatasetVersion = null;
                        }
                        catch { }
                        repo3.Delete(tuple); // this is not a good solution as it loads all the tuple versions an then deletes them!! a generic solution for bulk delete without loading is needed
                    }
                }
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        #endregion

        #region DatasetVersion
        
        /// <summary>
        /// if dataset is checked in, get latest version bur returning whatever is in the tuples table from the requested version and before
        /// if its checked out get a version before the latest Versions.OrderByDesc(Timestamp),Skip(1).Take(1)/ 
        /// in case the user has asked for the latest version while the dataset is checked out, the new and unchanged tuples are in the Tuples table but deleted and changed one should be retrieved from the TupleVersions table
        /// get the latest version by querying Tuples table for records with version <= latest version
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <returns></returns>
        public List<DataTuple> GetDatasetVersionEffectiveTuples(DatasetVersion datasetVersion)
        {
            return getDatasetVersionEffectiveTuples(datasetVersion);
        }
       
        public List<DatasetVersion> GetDatasettVersions(Int64 datasetId)
        {
            List<DatasetVersion> dsVersions = DatasetVersionRepo.Query(p => p.Dataset.Id == datasetId && p.Dataset.Status == DatasetStatus.CheckedIn).OrderByDescending(p => p.Timestamp).ToList();
            if (dsVersions != null)
            {
                //dsVersions.ForEach(p=> p.Materialize());
                return (dsVersions);
            }
            try
            {
                Dataset dataset = DatasetRepo.Get(datasetId);
                if (dataset == null)
                    throw new Exception(string.Format("Dataset {0} does not exist!", datasetId));
                if (dataset.Status == DatasetStatus.Deleted)
                    throw new Exception(string.Format("Dataset {0} is deleted", datasetId));
                if (dataset.Status == DatasetStatus.CheckedOut)
                {
                    dsVersions = dataset.Versions.Where(p => p.Status == DatasetVersionStatus.Old || p.Status == DatasetVersionStatus.CheckedIn).ToList(); //dataset.Versions.OrderByDescending(p => p.Timestamp).Skip(1).ToList(); // the first version in the list is the working copy
                    if (dsVersions != null)
                    {
                        //dsVersions.ForEach(p => p.Materialize());
                        return (dsVersions);
                    }
                }
            }
            catch
            {
                throw new Exception(string.Format("Dataset {0} does not exist or an  error occurred!", datasetId));
            }
            return (null);
        }

        public DatasetVersion GetDatasetLatestVersion(Int64 datasetId)
        {
            DatasetVersion dsVersion = DatasetVersionRepo.Query(p => 
                    p.Dataset.Id == datasetId 
                    && p.Dataset.Status == DatasetStatus.CheckedIn 
                    && p.Status == DatasetVersionStatus.CheckedIn)
                    .FirstOrDefault();//DatasetVersionRepo.Query(p => p.Dataset.Id == datasetId && p.Dataset.Status == DatasetStatus.CheckedIn).OrderByDescending(p => p.Timestamp).FirstOrDefault();
            if (dsVersion != null)
            {
                //dsVersion.Materialize();
                return (dsVersion);
            }
            try
            {
                Dataset dataset = DatasetRepo.Get(datasetId);
                if(dataset == null)
                    throw new Exception(string.Format("Dataset {0} does not exist!", datasetId));
                if (dataset.Status == DatasetStatus.Deleted)
                    throw new Exception(string.Format("Dataset {0} is deleted", datasetId));
                if (dataset.Status == DatasetStatus.CheckedOut)
                {
                    throw new Exception(string.Format("Dataset {0} is checked out.", datasetId));
                }             
            }
            catch
            {
                throw new Exception(string.Format("Dataset {0} does not exist or an  error occurred!", datasetId));
            }
            return (null);
        }

        public List<DatasetVersion> GetDatasetLatestVersions(List<Int64> datasetIds, bool includeCheckouts = false)
        {
            if (includeCheckouts) // the working copy versions of checked out datasets are also included
            {
                var q1 = DatasetVersionRepo.Query(p => 
                        datasetIds.Contains(p.Dataset.Id) 
                        && (p.Dataset.Status == DatasetStatus.CheckedIn || p.Dataset.Status == DatasetStatus.CheckedOut) 
                        && (p.Status == DatasetVersionStatus.CheckedIn || p.Status == DatasetVersionStatus.CheckedOut)
                    );
                return (q1.ToList());
            }
            else //just latest checked in versions or checked in datasets 
            {
                var q1 = DatasetVersionRepo.Query(p =>
                        datasetIds.Contains(p.Dataset.Id)
                        && (p.Dataset.Status == DatasetStatus.CheckedIn)
                        && (p.Status == DatasetVersionStatus.CheckedIn)
                    );
                return (q1.ToList());
            }
            // also works, but uses the time stamps instead of STATUS info
           // var qu = (from dsv in DatasetVersionRepo.Get(p => datasetIds.Contains(p.Dataset.Id) && p.Dataset.Status != DatasetStatus.Deleted)
           //           group dsv by dsv.Dataset.Id into grp
           //           let maxTimestamp = grp.Max(p => p.Timestamp)
           //           select grp.Single(p => p.Timestamp >= maxTimestamp)); 
            
           //return (qu.ToList());
        }

        public List<DatasetVersion> GetDatasetLatestVersions(bool includeCheckouts = false)
        {
            if (includeCheckouts) // the working copy versions of checked out datasets are also included
            {
                var q1 = DatasetVersionRepo.Query(p =>                        
                        (p.Dataset.Status == DatasetStatus.CheckedIn || p.Dataset.Status == DatasetStatus.CheckedOut)
                        && (p.Status == DatasetVersionStatus.CheckedIn || p.Status == DatasetVersionStatus.CheckedOut)
                    );
                return (q1.ToList());
            }
            else //just latest checked in versions or checked in datasets 
            {
                var q1 = DatasetVersionRepo.Query(p =>
                        (p.Dataset.Status == DatasetStatus.CheckedIn)
                        && (p.Status == DatasetVersionStatus.CheckedIn)
                    );
                return (q1.ToList());
            }

            //// its a mixed query that happens partially in the database. The grouping is happening in the memory which is BAD. JAVAD. 
            //// I have tested a full DB version but does not work.  needs more investigation

            //var qu = (from dsv in DatasetVersionRepo.Get(p => p.Dataset.Status != DatasetStatus.Deleted)
            //          group dsv by dsv.Dataset.Id into grp
            //          let maxTimestamp = grp.Max(p => p.Timestamp)
            //          select grp.Single(p => p.Timestamp >= maxTimestamp));

            //return (qu.ToList());
        }

        public Dictionary<Int64, XmlDocument> GetDatasetLatestMetadataVersions(bool includeCheckouts = false)
        {
            if (includeCheckouts) // the working copy versions of checked out datasets are also included
            {
                var q1 = DatasetVersionRepo.Query(p =>
                        (p.Dataset.Status == DatasetStatus.CheckedIn || p.Dataset.Status == DatasetStatus.CheckedOut)
                        && (p.Status == DatasetVersionStatus.CheckedIn || p.Status == DatasetVersionStatus.CheckedOut)
                    ).Select(p => new KeyValuePair<Int64, XmlDocument>(p.Dataset.Id, p.Metadata));
                return (q1.ToList().ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }
            else //just latest checked in versions or checked in datasets 
            {
                var q1 = DatasetVersionRepo.Query(p =>
                        (p.Dataset.Status == DatasetStatus.CheckedIn || p.Dataset.Status == DatasetStatus.CheckedOut) // include checked in (latest) versions of currently checked out datasets
                        && (p.Status == DatasetVersionStatus.CheckedIn)
                    ).Select(p => new KeyValuePair<Int64, XmlDocument>(p.Dataset.Id, p.Metadata));
                return (q1.ToList().ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }

            //// it works using the timestamp technique
            //var qu = (from dsv in DatasetVersionRepo.Get(p => p.Dataset.Status != DatasetStatus.Deleted)
            //         group dsv by dsv.Dataset.Id into grp
            //         let maxTimestamp = grp.Max(p => p.Timestamp)
            //         select grp.Single(p => p.Timestamp >= maxTimestamp).Metadata);

            //return (qu.ToList());
        }

        public DatasetVersion GetDatasetLatestVersion(Dataset dataset)
        {
            /// the latest checked in version should be returned.
            /// if dataset is checked out, exception
            /// If the dataset is marked as deleted its like that it is not there at all

            return getDatasetLatestVersion(dataset);
        }

        public DatasetVersion GetDatasetVersion(Int64 datasetId, Int64 versionId)
        {
            /// check whether the version id is in fact the latest? the latest checked in version should be returned. if dataset is checked out, the latest stored version is hidden yet.
            /// If the dataset is marked as deleted its like that it is not there at all
            /// get the latest version from the Versions property, or run a direct query on the db
            /// get the latest version by querying Tuples table for records with version <= latest version

            // the requested version is earlier than the latest regardless of check-in/ out status or its the latest version and the dataset is checked in.
            DatasetVersion dsVersion = DatasetVersionRepo.Query(p => 
                                        p.Dataset.Id == datasetId
                                        && p.Id == versionId
                                        && (
                                                    (p.Dataset.Status == DatasetStatus.CheckedIn    && p.Status == DatasetVersionStatus.CheckedIn)
                                                ||  (p.Dataset.Status != DatasetStatus.Deleted      && p.Status == DatasetVersionStatus.Old)
                                            )
                                        )
                                      .FirstOrDefault();
            if (dsVersion != null)
                return (dsVersion);
            
            // else there is a problem, try to find and report it
            Dataset dataset = DatasetRepo.Get(datasetId); // it would be nice to not fetch the dataset!
            if (dataset.Status == DatasetStatus.Deleted)
                throw new Exception(string.Format("Dataset {0} is deleted", datasetId));
            Int64 latestVersionId = dataset.Versions.Where(p=> p.Status == DatasetVersionStatus.CheckedIn).Select(p=>p.Id).First();// .OrderByDescending(t => t.Timestamp).First().Id;
            if (versionId > latestVersionId)
                throw new Exception(string.Format("Invalid version id. The version id {0} is greater than the latest version number!", versionId));

            if (latestVersionId.Equals(versionId) && dataset.Status == DatasetStatus.CheckedOut) // its a request for the working copy which is hidden
                throw new Exception(string.Format("Invalid version is requested. The version {0} points to the working copy!", versionId));
            return null;
        }

        public DatasetVersion GetDatasetWorkingCopy(Int64 datasetId)
        {
            return getDatasetWorkingCopy(datasetId);
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

        /// <summary>
        /// there is no need to pass metadata, extendedPropertyValues, contentDescriptors .. as they can be assigned to the version before sending it to this editing method
        /// The general procedure is CheckOut, Edit*, CheckIn or Rollback
        /// While the dataset is checked out, all the changes go to the latest+1 version which acts like a working copy
        /// </summary>
        /// <param name="workingCoptDatasetVersion"></param>
        /// <param name="createdTuples"></param>
        /// <param name="editedTuples"></param>
        /// <param name="deletedTuples"></param>
        /// <param name="unchangedTuples">to be removed</param>
        /// <returns></returns>
        public DatasetVersion EditDatasetVersion(DatasetVersion workingCoptDatasetVersion,
            ICollection<DataTuple> createdTuples, ICollection<DataTuple> editedTuples, ICollection<DataTuple> deletedTuples, ICollection<DataTuple> unchangedTuples = null
            //,ICollection<ExtendedPropertyValue> extendedPropertyValues, ICollection<ContentDescriptor> contentDescriptors
            )
        {
            var workingCoptDatasetVersionId = workingCoptDatasetVersion.Id;
            // do not move them to editDatasetVersion function
            this.DatasetRepo.Evict();
            this.DatasetVersionRepo.Evict();
            this.DataTupleRepo.Evict();
            this.DataTupleVerionRepo.Evict();

            workingCoptDatasetVersion = this.DatasetVersionRepo.Get(workingCoptDatasetVersionId);

            return editDatasetVersion(workingCoptDatasetVersion, createdTuples, editedTuples, deletedTuples, unchangedTuples);
        }

       
        #endregion

        #region Private Methods

        private DatasetVersion getDatasetWorkingCopy(Int64 datasetId)
        {
            DatasetVersion dsVersion = DatasetVersionRepo.Query(p =>
                                       p.Dataset.Id == datasetId
                                       && p.Dataset.Status == DatasetStatus.CheckedOut
                                       && p.Status == DatasetVersionStatus.CheckedOut
                                       )
                                     .FirstOrDefault();
            if (dsVersion != null)
            {
                dsVersion.Materialize();
                return (dsVersion);
            }

            // else there is a problem, try to find and report it
            Dataset dataset = DatasetRepo.Get(datasetId); // it would be nice to not fetch the dataset!
            if (dataset.Status == DatasetStatus.Deleted)
                throw new Exception(string.Format("Dataset {0} is deleted", datasetId));
            if (dataset.Status == DatasetStatus.CheckedIn)
                throw new Exception(string.Format("Dataset {0} is in checked in state", datasetId));
            return null;
        }
        
        private List<DataTuple> getDatasetVersionEffectiveTuples(DatasetVersion datasetVersion)
        {
            List<DataTuple> tuples = new List<DataTuple>();
            Dataset dataset = datasetVersion.Dataset;
            if (dataset.Status == DatasetStatus.Deleted)
                throw new Exception(string.Format("Provided dataset version {0} belongs to deleted dataset {1}.", datasetVersion.Id, dataset.Id));
            Int64 latestVersionId = dataset.Versions.OrderByDescending(t => t.Timestamp).Where(p=>p.Timestamp <= dataset.LastCheckIOTimestamp).First().Id; // no need to replace it with the STATUS version
            if (datasetVersion.Id > latestVersionId)
                throw new Exception(string.Format("Invalid version id. The dataset version id {0} is greater than the latest version number!", datasetVersion.Id));

            if (latestVersionId == datasetVersion.Id && dataset.Status == DatasetStatus.CheckedOut) // its a request for the working copy
            {
                tuples = getWorkingCopyTuples(datasetVersion);
            }
            else if (latestVersionId == datasetVersion.Id && dataset.Status == DatasetStatus.CheckedIn) // its a request for the latest checked in version that should be served from the Tuples table
            {
                tuples = getPrimaryTuples(datasetVersion);
            }
            else
            {
                tuples = getHistoricTuples(datasetVersion);
            }
            tuples.ForEach(p => p.Materialize());
            return (tuples);
        }

        private DatasetVersion editDatasetVersion(DatasetVersion workingCoptDatasetVersion, ICollection<DataTuple> createdTuples, ICollection<DataTuple> editedTuples, ICollection<DataTuple> deletedTuples, ICollection<DataTuple> unchangedTuples)
        {
            Contract.Requires(workingCoptDatasetVersion.Dataset != null && workingCoptDatasetVersion.Dataset.Id >= 0);
            Contract.Requires(workingCoptDatasetVersion.Dataset.Status == DatasetStatus.CheckedOut);

            Contract.Ensures(Contract.Result<DatasetVersion>() != null && Contract.Result<DatasetVersion>().Id >= 0);

            // be sure you are working on the latest version (working copy). applyTupleChanges takes the working copy from the DB            
            List<DataTupleVersion> tobeAdded = new List<DataTupleVersion>();
            List<DataTuple> tobeDeleted = new List<DataTuple>();

            DatasetVersion edited = applyTupleChanges(workingCoptDatasetVersion, ref tobeAdded, ref tobeDeleted, createdTuples, editedTuples, deletedTuples, unchangedTuples);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DatasetVersion> repo = uow.GetRepository<DatasetVersion>();
                IRepository<DataTupleVersion> tupleVersionRepo = uow.GetRepository<DataTupleVersion>();
                IRepository<DataTuple> tupleRepo = uow.GetRepository<DataTuple>();

                foreach (var item in tobeAdded)
                {
                    tupleVersionRepo.Put(item);
                }

                foreach (var item in tobeDeleted)
                {                    
                    tupleRepo.Delete(item);
                }
                // check whether the changes to the latest version, which is changed in the applyTupleChanges , are committed too!
                repo.Put(edited);
                uow.Commit();
            }

            return (edited);
        }

        private List<DataTuple> getHistoricTuples(DatasetVersion datasetVersion)
        {
            //get previous versions including the version specified
            List<Int64> versionIds = datasetVersion.Dataset.Versions
                                       .Where(p => p.Timestamp <= datasetVersion.Timestamp)
                                       .OrderByDescending(t => t.Timestamp)
                                       .Select(p => p.Id)
                                       .ToList();
            //get all tuples from the main tuples table belonging to one of the previous versions + the current version
            List<DataTuple> tuples = DataTupleRepo.Get(p => versionIds.Contains(p.DatasetVersion.Id)).ToList();

            List<DataTuple> editedTuples = DataTupleVerionRepo.Get(p => (p.TupleAction == TupleAction.Edited) && (p.DatasetVersion.Id == datasetVersion.Id) && !(versionIds.Contains(p.ActingDatasetVersion.Id))).Cast<DataTuple>().ToList();
            List<DataTuple> deletedTuples = DataTupleVerionRepo.Get(p => (p.TupleAction == TupleAction.Deleted) && (versionIds.Contains(p.DatasetVersion.Id)) && !(versionIds.Contains(p.ActingDatasetVersion.Id))).Cast<DataTuple>().ToList();

            List<DataTuple> result = tuples.Union(editedTuples).Union(deletedTuples).ToList();
            return (result);
        }

        private List<DataTuple> getPrimaryTuples(DatasetVersion datasetVersion)
        {
            // effective tuples of the latest checked in version are in DataTuples table but they belong to the latest and previous versions
            List<Int64> versionIds = datasetVersion.Dataset.Versions
                                        .Where(p => p.Timestamp <= datasetVersion.Timestamp)
                                        .OrderByDescending(t=>t.Timestamp)
                                        .Select(p=>p.Id)
                                        .ToList();
            List<DataTuple> tuples = (versionIds == null || versionIds.Count() <= 0) ? new List<DataTuple>() : DataTupleRepo.Get(p => versionIds.Contains(p.DatasetVersion.Id)).ToList();
            //Dictionary<string, object> parameters = new Dictionary<string, object>() { { "datasetVersionId", datasetVersion.Id } };
            //List<DataTuple> tuples = DataTupleRepo.Get("getLatestCheckedInTuples", parameters).ToList();
            return (tuples);
        }

        private List<DataTuple> getWorkingCopyTuples(DatasetVersion datasetVersion)
        {
            // effective tuples of the working copy are similar to latest checked in version. They are in DataTuples table but they belong to the latest and previous versions
            List<Int64> versionIds = datasetVersion.Dataset.Versions
                                        .Where(p => p.Timestamp <= datasetVersion.Timestamp)
                                        .OrderByDescending(t => t.Timestamp)
                                        .Select(p => p.Id)
                                        .ToList();
            List<DataTuple> tuples = (versionIds == null || versionIds.Count() <= 0 )? new List<DataTuple>(): DataTupleRepo.Get(p => versionIds.Contains(((DataTuple)p).DatasetVersion.Id)).ToList();
            return (tuples);
        }
        
        private DatasetVersion getDatasetLatestVersion(Dataset dataset)
        {
            /// the latest checked in version should be returned.
            /// if dataset is checked out, exception
            /// If the dataset is marked as deleted its like that it is not there at all
           
            if (dataset == null)
                throw new Exception(string.Format("Provided dataset is null"));
            if (dataset.Status == DatasetStatus.Deleted)
                throw new Exception(string.Format("Dataset {0} is deleted", dataset.Id));
            if (dataset.Status == DatasetStatus.CheckedOut)
            {
                throw new Exception(string.Format("Dataset {0} is checked out.", dataset.Id));
            }
            if (dataset.Status == DatasetStatus.CheckedIn)
            {
                DatasetVersion dsVersion = dataset.Versions.OrderByDescending(t => t.Timestamp).First(p=>p.Status == DatasetVersionStatus.CheckedIn); // indeed the versions collection is ordered and there should be no need for ordering, but is just to prevent any side effects
                return (dsVersion);
            }
            return null;
        }

        /// <summary>
        /// checks out the dataset and creates a new version on it. the new version acts like a working copy while it is not committed, hence editable.
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="userName"></param>
        private bool checkOutDataset(Int64 datasetId, string userName)
        {
            bool checkedOut = false;
            DateTime timestamp = DateTime.UtcNow;
            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(@"<Metadata>Empty</Metadata>");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Dataset> repo = uow.GetRepository<Dataset>();
                var q = repo.Query(p => p.Id == datasetId && p.Status == DatasetStatus.CheckedIn && (p.CheckOutUser.Equals(string.Empty) || p.CheckOutUser == null));
                Dataset ds = q.FirstOrDefault();
                if (ds != null)
                {
                    DatasetVersion dsNewVersion = new DatasetVersion()
                    {
                        Timestamp = timestamp,
                        //Metadata = doc,
                        ExtendedPropertyValues = new List<ExtendedPropertyValue>(),
                        ContentDescriptors = new List<ContentDescriptor>(),
                        Status = DatasetVersionStatus.CheckedOut,
                    };
                    ds.Status = DatasetStatus.CheckedOut;
                    ds.LastCheckIOTimestamp = timestamp;
                    ds.CheckOutUser = getUserIdentifier(userName);
                    ds.Versions.Add(dsNewVersion);
                    dsNewVersion.Dataset = ds;
                    repo.Put(ds);
                    uow.Commit();
                    checkedOut = true;
                }
            }
            return (checkedOut);
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
                    ds = repo.Get(p => p.Id == datasetId && p.Status == DatasetStatus.CheckedOut && p.CheckOutUser.Equals(getUserIdentifier(userName))).FirstOrDefault();
                if (ds != null)
                {
                    DatasetVersion previousCheckIn = ds.Versions.FirstOrDefault(p => p.Status == DatasetVersionStatus.CheckedIn);
                    if (previousCheckIn != null)
                        previousCheckIn.Status = DatasetVersionStatus.Old;
                    
                    DatasetVersion dsv = ds.Versions.OrderByDescending(t => t.Timestamp).First(p=>p.Status == DatasetVersionStatus.CheckedOut);
                    dsv.ChangeDescription = comment;
                    dsv.Status = DatasetVersionStatus.CheckedIn;

                    ds.Status = DatasetStatus.CheckedIn;
                    ds.LastCheckIOTimestamp = DateTime.UtcNow;
                    ds.CheckOutUser = string.Empty;
                    
                    repo.Put(ds);
                    uow.Commit();
                }
            }
        }

        // in some cases maybe another attribute of the user is used like its ID, email or the IP address
        private string getUserIdentifier(string userName)
        {
            return (userName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="userName"></param>
        /// <param name="adminMode"></param>
        /// <param name="commit">in some cases, rollback is called on a set of datasets. In  these cases its better to not commit at each rollback, but at the end</param>
        private void undoCheckout(Int64 datasetId, string userName, bool adminMode, bool commit = true)
        {
            // maybe its required to pass the caller's repo in order to the rollback changes to be visible to the caller function and be able to commit them
            // bring back the historical tuples
            // remove the version after the processes are finished
            // take the dataset back to the checked in state
            // check for admin mode
        }

        private DatasetVersion applyTupleChanges(DatasetVersion workingCopyVersion, ref List<DataTupleVersion> tupleVersionsTobeAdded, ref List<DataTuple> tuplesTobeDeleted,
            ICollection<DataTuple> createdTuples, ICollection<DataTuple> editedTuples, ICollection<DataTuple> deletedTuples, ICollection<DataTuple> unchangedTuples = null)
        {
            // do nothing with unchanged for now

            #region Process Newly Created Tuples

            /// associate newly created tuples to the new version
            if (createdTuples != null && createdTuples.Count() > 0)
            {
                foreach (var item in createdTuples)
                {
                    item.Dematerialize();
                    workingCopyVersion.PriliminaryTuples.Add(item);
                    item.DatasetVersion = workingCopyVersion;
                    //item.TupleAction = TupleAction.Created;
                    item.Timestamp = workingCopyVersion.Timestamp;
                }
            }

            #endregion

            if ((editedTuples != null && editedTuples.Count() > 0) || (deletedTuples != null && deletedTuples.Count() > 0))
            {

                // latest version is the latest checked in version. it is the previous version in comparison to the working copy version.
                // the checks to see whether the dataset is checked out are considered to be done before
                DatasetVersion latestCheckedInVersion = workingCopyVersion.Dataset.Versions.OrderByDescending(p => p.Timestamp).FirstOrDefault(p => p.Status == DatasetVersionStatus.CheckedIn);
                if (latestCheckedInVersion == null) // there is no previous version, means its the first version. In this case there is no need to handle deleted and edited items!
                    return (workingCopyVersion);

                List<DataTuple> latestVersionEffectiveTuples = getPrimaryTuples(latestCheckedInVersion); //latestVersionEffectiveTuples =  DataTupleRepo.Get(p=>p.DatasetVersion ==  null).ToList();
               
                #region Process Edited Tuples

                /// manage edited tuples: 
                /// 1: create a DataTupleVersion based on its previous version
                /// 2: Remove the original from the original version
                /// 3: add them to the version
                /// 4: set timestamp for the edited ones
                if (editedTuples != null && editedTuples.Count() > 0)
                {
                    foreach (var edited in editedTuples)
                    {
                        edited.Dematerialize();
                        //need a better way to preserve the changed during the fetch of the original tuple. Maybe deep copy/ evict/ merge works
                        int orderNo = edited.OrderNo;
                        var xmlAmendments = edited.XmlAmendments;
                        var xmlVariableValues = edited.XmlVariableValues;

                        //DataTupleRepo.Evict(detached);
                        DataTuple orginalTuple = latestVersionEffectiveTuples.Where(p => p.Id == edited.Id).Single();//maybe preliminary tuples are enough
                        DataTupleVersion tupleVersion = new DataTupleVersion()
                        {
                            TupleAction = TupleAction.Edited,
                            Extra = orginalTuple.Extra,
                            //Id = orginalTuple.Id,
                            OrderNo = orginalTuple.OrderNo,
                            Timestamp = orginalTuple.Timestamp,
                            XmlAmendments = orginalTuple.XmlAmendments,
                            XmlVariableValues = orginalTuple.XmlVariableValues,
                            OriginalTuple = orginalTuple,
                            DatasetVersion = latestCheckedInVersion,
                            ActingDatasetVersion = workingCopyVersion,
                        };
                        //DataTuple merged = 
                        orginalTuple.History.Add(tupleVersion);

                        edited.OrderNo = orderNo;
                        edited.XmlAmendments = xmlAmendments;
                        edited.XmlVariableValues = xmlVariableValues;

                        edited.DatasetVersion = workingCopyVersion;
                        edited.Timestamp = workingCopyVersion.Timestamp;
                        //workingCopyVersion.PriliminaryTuples.Add(detached);

                        //latestCheckedInVersion.PriliminaryTuples.Remove(orginalTuple);
                        //latestVersionEffectiveTuples.Remove(orginalTuple);
                    }
                }

                #endregion

                #region Process Deleted Tuples

                /// manage deleted tuples: 
                /// 1: create a DataTupleVersion based on their previous version
                /// 2: Remove them from the latest version
                /// 3: DO NOT add them to the new version
                /// 4: DO NOT set timestamp for the deleted ones

                if (deletedTuples != null && deletedTuples.Count() > 0)
                {
                    foreach (var deleted in deletedTuples)
                    {
                        DataTuple orginalTuple = latestVersionEffectiveTuples.Where(p => p.Id == deleted.Id).Single();
                        DataTupleVersion tupleVersion = new DataTupleVersion()
                        {
                            TupleAction = TupleAction.Deleted,
                            Extra = orginalTuple.Extra,
                            //Id = orginalTuple.Id,
                            OrderNo = orginalTuple.OrderNo,
                            Timestamp = orginalTuple.Timestamp,
                            XmlAmendments = orginalTuple.XmlAmendments,
                            XmlVariableValues = orginalTuple.XmlVariableValues,
                            //OriginalTuple = orginalTuple,
                            DatasetVersion = latestCheckedInVersion,
                            ActingDatasetVersion = workingCopyVersion,
                        };

                        tupleVersion.OriginalTuple = null;

                        latestCheckedInVersion.PriliminaryTuples.Remove(orginalTuple);
                        latestVersionEffectiveTuples.Remove(orginalTuple);
                        workingCopyVersion.PriliminaryTuples.Remove(orginalTuple);
                        try
                        {
                            orginalTuple.History.ToList().ForEach(p => p.OriginalTuple = null);
                        }
                        catch { }
                        try
                        {
                            orginalTuple.DatasetVersion.PriliminaryTuples.Remove(orginalTuple);
                        }
                        catch { }

                        orginalTuple.History.Clear();
                        orginalTuple.DatasetVersion = null;

                        tuplesTobeDeleted.Add(orginalTuple);
                        tupleVersionsTobeAdded.Add(tupleVersion);
                    }
                }

                #endregion
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
