using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Xml.Helpers
{
    public class DatasetStore : IEntityStore
    {
        private const string _entityName = "Dataset";

        public List<EntityStoreItem> GetEntities()
        {
            return GetEntities(0, 0);
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            bool withPaging = (take > 0);


            using (var uow = this.GetUnitOfWork())
            using (DatasetManager dm = new DatasetManager())
            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            {
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                var entities = new List<EntityStoreItem>();

                try
                {
                    List<long> metadataStructureIds = metadataStructureManager.Repo.Query().Select(m => m.Id).ToList();

                    List<long> metadataSturctureIdsForDatasets = new List<long>();
                    metadataSturctureIdsForDatasets = metadataStructureIds.Where(m => xmlDatasetHelper.HasEntity(m, _entityName)).ToList();

                    foreach (var msid in metadataSturctureIdsForDatasets)
                    {
                        var datasetIds = new List<long>();
                        // get all datasets based on metadata data structure id
                        if (withPaging)
                        {
                            datasetIds = dm.DatasetRepo
                                                    .Query(d => d.MetadataStructure.Id.Equals(msid))
                                                    .Skip(skip)
                                                    .Take(take)
                                                    .Select(d => d.Id).ToList();
                        }
                        else
                        {
                            datasetIds = dm.DatasetRepo.Query(d => d.MetadataStructure.Id.Equals(msid)).Select(d => d.Id).ToList();
                        }


                        if (!datasetIds.Any()) continue;

                        // create tuples based on dataset id list, and get latest version of each dataset

                        List<DatasetVersion> datasetVersions = dm.GetDatasetLatestVersions(datasetIds, false);

                        foreach (var dsv in datasetVersions)
                        {
                            var e = new EntityStoreItem()
                            {
                                Id = dsv.Dataset.Id,
                                Title = dsv.Title,
                                Version = dm.GetDatasetVersionCount(dsv.Dataset.Id)
                            };

                            entities.Add(e);
                        }

                        List<DatasetVersion> deletedDatasetVersions = dm.GetDeletedDatasetLatestVersions(datasetIds);
                        if (deletedDatasetVersions.Any())
                        {
                            foreach (var dsv in deletedDatasetVersions)
                            {
                                var e = new EntityStoreItem()
                                {
                                    Id = dsv.Dataset.Id,
                                    Title = dsv.Title + " (deleted)",
                                    Version = dm.GetDatasetVersionCount(dsv.Dataset.Id)
                                };


                                entities.Add(e);
                            }


                        }
                    }

                    return entities.ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        public int CountEntities()
        {
            using (var uow = this.GetUnitOfWork())
            using (DatasetManager dm = new DatasetManager())
            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            {


                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                var entities = new List<EntityStoreItem>();
                int count = 0;

                try
                {
                    List<long> metadataStructureIds = metadataStructureManager.Repo.Query().Select(m => m.Id).ToList();

                    List<long> metadataSturctureIdsForDatasets = new List<long>();
                    metadataSturctureIdsForDatasets = metadataStructureIds.Where(m => xmlDatasetHelper.HasEntity(m, _entityName)).ToList();

                    foreach (var msid in metadataSturctureIdsForDatasets)
                    {
                        var datasetIds = new List<long>();
                        // get all datasets based on metadata data structure id

                        datasetIds = dm.DatasetRepo.Query(d => d.MetadataStructure.Id.Equals(msid)).Select(d => d.Id).ToList();
                        count += datasetIds.Count;

                    }

                    return count;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    dm.Dispose();
                }
            }
        }

        public string GetTitleById(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var dm = new DatasetManager();

                try
                {
                    var dataset = dm.GetDataset(id);
                    if (dataset.Status == DatasetStatus.Deleted) return String.Empty;

                    var dsv = dm.GetDatasetLatestVersion(id);

                    return dsv.Title;
                }
                catch
                {
                    return String.Empty;
                }
                finally
                {
                    dm.Dispose();
                }
            }
        }

        public int CountVersions(long id)
        {
            DatasetManager dm = new DatasetManager();

            try
            {
                var datasetIds = dm.GetDatasetLatestIds();
                var datasetHelper = new XmlDatasetHelper();

                int version = dm.GetDataset(id).Versions.Count;

                return version;
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                dm.Dispose();
            }
        }

        public List<EntityStoreItem> GetVersionsById(long id)
        {
            DatasetManager dm = new DatasetManager();
            List<EntityStoreItem> tmp = new List<EntityStoreItem>();
            try
            {
                var datasetIds = dm.GetDatasetLatestIds();
                var datasetHelper = new XmlDatasetHelper();
                var versions = dm.GetDataset(id).Versions.OrderBy(v => v.Timestamp).ToList();

                foreach (var v in versions)
                {
                    tmp.Add(new EntityStoreItem()
                    {
                        Id = v.Id,
                        Title = v.Title,
                        Version = versions.IndexOf(v) + 1,
                        CommitComment = "(" + v.Timestamp.ToString("dd.MM.yyyy HH:mm") + "): " + v.ChangeDescription
                    });
                }

                return tmp;
            }
            catch (Exception ex)
            {
                return tmp;
            }
            finally
            {
                dm.Dispose();
            }
        }

        public bool HasVersions()
        {
            return true;
        }

        public bool Exist(long id)
        {
            DatasetManager dm = new DatasetManager();
            Dataset dataset = null;

            try
            {
                dataset = dm.GetDataset(id);
                return dataset != null ? true : false;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                dm.Dispose();
            }
        }
    }
}