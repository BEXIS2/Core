using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using System.Threading.Tasks;


namespace BExIS.Utils.Data
{
    public class DatasetVersionHelper
    {
        public static async Task<long> GetVersionId(long datasetId, string username, int versionNr = 0 ,bool useTags = false, double tagNr = 0)
        {
            // get entity by versionnr,versionName, tagnr
            RightType rightType = RightType.Read;
            bool isPublic = false;
            bool isVerionReady = false;


            using (var permissionManager = new EntityPermissionManager())
            using (var datasetManager = new DatasetManager())
            {
                var dataset = datasetManager.GetDataset(datasetId);


                // check if dataset is public
                isPublic = await permissionManager.IsPublicAsync(dataset.EntityTemplate.EntityType.Id, datasetId);

                // if dataset is deleted and public return the latest version
                if (dataset.Status == DatasetStatus.Deleted && isPublic)
                {
                    if (tagNr > 0)
                    {
                        return datasetManager.GetLatestVersionIdByTagNr(datasetId, tagNr);
                    }
                    else if (versionNr > 0)
                    {
                        //get the latest version belong to the tag of the requested version
                        var datasetVersionId = datasetManager.GetDatasetVersionId(datasetId, versionNr);
                        return datasetVersionId;
                    }
                    else // use latest version
                    {
                        var datasetVersionId = datasetManager.GetDatasetLatestVersionId(datasetId, DatasetStatus.Deleted);
                        return datasetVersionId;
                    }

                }

                // get rights
                bool hasEditRights = await permissionManager.HasEffectiveRightsAsync(username, typeof(BExIS.Dlm.Entities.Data.Dataset), datasetId, RightType.Write);
                bool hasReadRights = await permissionManager.HasEffectiveRightsAsync(username, typeof(BExIS.Dlm.Entities.Data.Dataset), datasetId, RightType.Read);

                // if user has edit rights && public/ internal accessible dont matter
                if (hasEditRights)
                {
                    if (tagNr > 0) // first try use tag
                    {
                        return datasetManager.GetLatestVersionIdByTagNr(datasetId, tagNr);
                    }
                    else if (versionNr > 0) // try use versionnr
                    {
                        return datasetManager.GetDatasetVersionId(datasetId, versionNr);
                    }
                    else // use latest version
                    {
                        return datasetManager.GetDatasetLatestVersionId(datasetId);
                    }
                }
                else if (hasReadRights)
                {

                    if (tagNr > 0) // public, not public version & not public 
                    {
                        return datasetManager.GetLatestVersionIdByTagNr(datasetId, tagNr);
                    }
                    else if (versionNr > 0) // public, not public version & not public 
                    {

                        if (!useTags)
                        {
                            var datasetVersion = datasetManager.GetDatasetVersion(datasetId, versionNr);
                            return datasetVersion.Id;
                        }
                        else
                        {
                            //get the latest version belong to the tag of the requested version
                            var datasetVersionId = datasetManager.GetDatasetVersionId(datasetId, versionNr);
                            var datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);
                            if (datasetVersion.Tag != null)// if a tag exist for the version
                            {
                                var latest = datasetManager.GetLatestVersionIdByTagNr(datasetId, datasetVersion.Tag.Nr);
                                return latest;
                            }
                            else // if no tag exist for the version, get the latest version
                            {
                                var tag = datasetManager.GetLatestTag(datasetId, true);
                                if (tag != null) // if any tag exist
                                {
                                    var latest = datasetManager.GetLatestVersionIdByTagNr(datasetId, tag.Nr);
                                    return latest;
                                }

                            }
                        }
                    }
                    else // use latest version or latest tag
                    {
                        if (!useTags) // if tags are not used
                        {
                            var datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
                            return datasetVersion.Id;
                        }
                        else // if tags are used
                        {
                            Tag tag = datasetManager.GetLatestTag(datasetId, true);
                            if (tag == null) return -1;
                            else
                            {
                                var datasetVersion = datasetManager.GetLatestVersionByTagNr(datasetId, tag.Nr);
                                return datasetVersion.Id;
                            }
                        }
                    }
                }
                else // no rigths
                {

                    long datasetVersionId = -1;
                    DatasetVersion datasetVersion = null;
                    Tag tag = null;

                    if (isPublic || !string.IsNullOrEmpty(username))
                    {
                        if (tagNr > 0)
                        {
                            datasetVersion = datasetManager.GetLatestVersionByTagNr(datasetId, tagNr);
                            if (datasetVersion.Tag != null && datasetVersion.Tag.Final)
                                return datasetVersion.Id;

                            // if tag is not reachable
                            tag = datasetManager.GetLatestTag(datasetId, true);
                            datasetVersion = datasetManager.GetLatestVersionByTagNr(datasetId, tag.Nr);
                            if (datasetVersion.Tag != null && datasetVersion.Tag.Final)
                                return datasetVersion.Id;

                            return -1;
                        }
                        else if (versionNr > 0) // public, not public version & not public 
                        {
                            if (!useTags) // if tags are not used
                            {
                                datasetVersion = datasetManager.GetDatasetVersion(datasetId, versionNr);
                                return datasetVersion.Id;
                            }
                            else // if tags  used
                            {
                                datasetVersion = datasetManager.GetLatestVersionByTagNr(datasetId, datasetVersion.Tag.Nr);

                                if (datasetVersion.Tag != null && datasetVersion.Tag.Final)
                                    return datasetVersion.Id;

                                // if tag is not reachable
                                tag = datasetManager.GetLatestTag(datasetId, true);
                                datasetVersion = datasetManager.GetLatestVersionByTagNr(datasetId, tag.Nr);

                                if (datasetVersion.Tag != null && datasetVersion.Tag.Final)
                                    return datasetVersion.Id;
                            }

                        }

                        // check in the settings if tags are active

                        if (useTags)
                        {
                            // get latest public
                            tag = datasetManager.GetLatestTag(datasetId, true);
                            if (tag == null) return -1;
                            else
                            {
                                datasetVersion = datasetManager.GetLatestVersionByTagNr(datasetId, tag.Nr);
                                return datasetVersion.Id;
                            }
                        }
                        else
                        {
                            return datasetManager.GetDatasetLatestVersionId(datasetId);
                        }
                    }
                }


            }

            return -1;
        }
    }
}
