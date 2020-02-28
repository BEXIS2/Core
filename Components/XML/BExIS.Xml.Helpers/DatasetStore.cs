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
            using (var uow = this.GetUnitOfWork())
            {
                DatasetManager dm = new DatasetManager();
                MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                var entities = new List<EntityStoreItem>();

                try
                {
                    List<long> metadataStructureIds = metadataStructureManager.Repo.Query().Select(m => m.Id).ToList();

                    List<long> metadataSturctureIdsForDatasets = new List<long>();
                    metadataSturctureIdsForDatasets = metadataStructureIds.Where(m => xmlDatasetHelper.HasEntity(m, _entityName)).ToList();

                    foreach (var msid in metadataSturctureIdsForDatasets)
                    {
                        // get all datasets based on metadata data structure id
                        var datasetIds = dm.DatasetRepo.Query(d => d.MetadataStructure.Id.Equals(msid)).Select(d => d.Id).ToList();

                        if (!datasetIds.Any()) continue;

                        List<Tuple<long, long, string>> x = new List<Tuple<long, long, string>>();

                        // create tuples based on dataset id list, and get latest version of each dataset

                        foreach (var datasetId in datasetIds)
                        {
                            if (dm.IsDatasetCheckedIn(datasetId))
                            {
                                x.Add(new Tuple<long, long, string>(
                                    datasetId,
                                    dm.GetDatasetLatestVersionId(datasetId),
                                    string.Empty));
                            }
                        }

                        //select versionids for the next query
                        var verionIds = x.Select(t => t.Item2).ToList();

                        var r = xmlDatasetHelper.GetInformationFromVersions(verionIds, msid, NameAttributeValues.title);

                        if (r != null)
                        {
                            foreach (KeyValuePair<long, string> kvp in r)
                            {
                                long id = x.Where(t => t.Item2.Equals(kvp.Key)).FirstOrDefault().Item1;

                                var e = new EntityStoreItem()
                                {
                                    Id = id,
                                    Title = kvp.Value,
                                    Version = dm.GetDatasetVersionCount(id)
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
                    var datasetHelper = new XmlDatasetHelper();

                    return datasetHelper.GetInformation(id, NameAttributeValues.title);
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
                        Title = datasetHelper.GetInformationFromVersion(v.Id, NameAttributeValues.title),
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
    }
}