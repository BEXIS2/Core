using BExIS.Dim.Entities.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using BExIS.Modules.Mmm.UI.Helpers;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using IDIV.Modules.Mmm.UI.Models;
using MediaInfoLib;
using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Vaiona.Entities.Common;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace IDIV.Modules.Mmm.UI.Controllers
{
    public class ShowMultimediaDataController : Controller
    {
        // GET: ShowMultimediaData
        public ActionResult Index(long datasetID, string entityType = "Dataset")
        {
            ViewData["id"] = datasetID;

            EntityPermissionManager entityPermissionManager = null;
            try
            {
                entityPermissionManager = new EntityPermissionManager();

                ViewData["edit"] = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Write).Result;

                bool access = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read).Result;

                if (access)
                {
                    return View(getFilesByDatasetId(datasetID, entityType));
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                entityPermissionManager.Dispose();
            }
        }

        public ActionResult multimediaData(long datasetID, long versionId = 0, string entityType = "Dataset")
        {
            ViewData["Id"] = datasetID;

            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (DatasetManager datasetManager = new DatasetManager())
            {
                try
                {
                    bool isLatestVersion = false;
                    if (versionId == datasetManager.GetDatasetLatestVersion(datasetID).Id)
                        isLatestVersion = true;

                    ViewData["edit"] = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Write).Result;

                    bool access = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read).Result;
                    Session["DatasetInfo"] = new DatasetInfo(datasetID, versionId, isLatestVersion, access, entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Delete).Result);
                    if (access)
                        return PartialView("_multimediaData", getFilesByDatasetId(datasetID, entityType, versionId));
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
                finally
                {
                }
            }
        }

        public ActionResult getFilePreview(string path, string type)
        {
            switch (type)
            {
                case "image":
                    return PartialView("_imagePreview", path);

                case "video":
                    return PartialView("_videoPreview", path);

                case "audio":
                    return PartialView("_audioPreview", path);

               

                default:
                    return PartialView("_defaultPreview", path);
            }
        }

        public FileResult getFile(string path, string send_mail = "true")
        {
            path = Server.UrlDecode(path);
            if (FileHelper.FileExist(Path.Combine(AppConfiguration.DataPath, path)))
            {
                EntityPermissionManager entityPermissionManager = null;
                DatasetManager datasetManager = null;
                try
                {
                    entityPermissionManager = new EntityPermissionManager();
                    datasetManager = new DatasetManager();

                    DatasetInfo datasetInfo = (DatasetInfo)Session["DatasetInfo"];
                    string entityType = (string)Session["EntityType"];
                    long datasetID = datasetInfo.DatasetId;
                    bool access = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read).Result;
                    if (access)
                    {
                        path = Path.Combine(AppConfiguration.DataPath, path);
                        FileInfo fileInfo = new FileInfo(path);
                        Session["DatasetInfo"] = datasetInfo;
                        Session["EntityType"] = entityType;

                        // after 2.14.1 files are stored in original names
                        // by download only the files, the user need to know th edataset id and the version number
                        int versionNr = datasetManager.GetDatasetVersionNr(datasetInfo.DatasetVersionId);
                        string filename = datasetInfo.DatasetId + "_" + versionNr + "_" + fileInfo.Name;

                        string message = string.Format("File from {0} version {1} was downloaded: {2}.", datasetID, versionNr, filename);
                        LoggerFactory.LogCustom(message);

                        if (send_mail == "true")
                        {
                            using (var emailService = new EmailService())
                            {
                                emailService.Send(MessageHelper.GetFileDownloadHeader(datasetID, versionNr),
                                                                                        MessageHelper.GetFileDownloadMessage(GetUsernameOrDefault(), datasetID, fileInfo.Name),
                                                                                        GeneralSettings.SystemEmail
                                                                                        );
                            }
                        }
                        return File(path, MimeMapping.GetMimeMapping(fileInfo.Name), filename);
                    }
                    else
                    {
                        Session["DatasetInfo"] = datasetInfo;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    entityPermissionManager.Dispose();
                    datasetManager.Dispose();
                }
            }
            else
            {
                WebRequest request = WebRequest.Create(path);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return File(response.GetResponseStream(), MimeMapping.GetMimeMapping(response.ResponseUri.Segments.LastOrDefault()), response.ResponseUri.Segments.LastOrDefault());
            }
        }

        public bool deleteFile(string path)
        {
            path = Server.UrlDecode(path);
            {
                using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
                using (DatasetManager datasetManager = new DatasetManager())
                {
                    try
                    {
                        DatasetInfo datasetInfo = (DatasetInfo)Session["DatasetInfo"];
                        string entityType = (string)Session["EntityType"];

                        DatasetVersion workingCopy = new DatasetVersion();
                        string status = DatasetStateInfo.NotValid.ToString();
                        string[] temp = path.Split('\\');
                        long datasetID = datasetInfo.DatasetId;
                        status = datasetManager.GetDatasetLatestVersion(datasetID).StateInfo.State;
                        bool access = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Delete).Result;

                        if (access && (datasetManager.IsDatasetCheckedOutFor(datasetID, HttpContext.User.Identity.Name) || datasetManager.CheckOutDataset(datasetID, HttpContext.User.Identity.Name)))
                        {
                            try
                            {
                                workingCopy = datasetManager.GetDatasetWorkingCopy(datasetID);

                                using (var unitOfWork = this.GetUnitOfWork())
                                {
                                    workingCopy = unitOfWork.GetReadOnlyRepository<DatasetVersion>().Get(workingCopy.Id);

                                    //set StateInfo of the previus version
                                    if (workingCopy.StateInfo == null)
                                    {
                                        workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo()
                                        {
                                            State = status
                                        };
                                    }
                                    else
                                    {
                                        workingCopy.StateInfo.State = status;
                                    }

                                    unitOfWork.GetReadOnlyRepository<DatasetVersion>().Load(workingCopy.ContentDescriptors);

                                    ContentDescriptor contentDescriptor = workingCopy.ContentDescriptors.Where(cd => cd.URI.Equals(path)).FirstOrDefault();
                                    datasetManager.DeleteContentDescriptor(contentDescriptor);
                                }

                                //set modification
                                workingCopy.ModificationInfo = new EntityAuditInfo()
                                {
                                    Performer = HttpContext.User?.Identity?.Name,
                                    Comment = "File",
                                    ActionType = AuditActionType.Delete
                                };

                                // set system key values
                                int v = 1;
                                if (workingCopy.Dataset.Versions != null && workingCopy.Dataset.Versions.Count > 1) v = workingCopy.Dataset.Versions.Count();
                                workingCopy.Metadata = setSystemValuesToMetadata(v, workingCopy.Dataset.Id, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata);

                                datasetManager.EditDatasetVersion(workingCopy, null, null, null);

                                // ToDo: Get Comment from ui and users
                                datasetManager.CheckInDataset(datasetID, temp.Last(), HttpContext.User.Identity.Name, ViewCreationBehavior.None);
                                Session["DatasetInfo"] = datasetInfo;
                                Session["EntityType"] = entityType;
                                return true;
                            }
                            catch
                            {
                                datasetManager.CheckInDataset(datasetID, "Failed to delete File " + temp.Last(), HttpContext.User.Identity.Name, ViewCreationBehavior.None);
                                Session["DatasetInfo"] = datasetInfo;
                                Session["EntityType"] = entityType;
                                return false;
                            }
                        }
                        Session["DatasetInfo"] = datasetInfo;
                        Session["EntityType"] = entityType;
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public FileResult getFileStreamResult(string path)
        {
            path = Server.UrlDecode(path);
            if (FileHelper.FileExist(Path.Combine(AppConfiguration.DataPath, path)))
            {
                EntityPermissionManager entityPermissionManager = null;
                try
                {
                    entityPermissionManager = new EntityPermissionManager();
                    DatasetInfo datasetInfo = (DatasetInfo)Session["DatasetInfo"];
                    string entityType = (string)Session["EntityType"];

                    long datasetID = datasetInfo.DatasetId;
                    bool access = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read).Result;
                    if (access)
                    {
                        path = Path.Combine(AppConfiguration.DataPath, path);
                        FileInfo fileInfo = new FileInfo(path);
                        Session["DatasetInfo"] = datasetInfo;
                        Session["EntityType"] = entityType;
                        return new FileStreamResult(new FileStream(path, FileMode.Open), MimeMapping.GetMimeMapping(fileInfo.Name));
                    }
                    else
                    {
                        Session["DatasetInfo"] = datasetInfo;
                        Session["EntityType"] = entityType;
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
                finally
                {
                    entityPermissionManager.Dispose();
                }
            }
            else
            {
                WebRequest request = WebRequest.Create(path);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return new FileStreamResult(response.GetResponseStream(), MimeMapping.GetMimeMapping(response.ResponseUri.Segments.LastOrDefault()));
            }
        }

        public Dictionary<string, Dictionary<string, string>> getExif(Stream fileStream)
        {
            Dictionary<string, Dictionary<string, string>> exif = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                if (fileStream.CanSeek)
                {
                    foreach (MetadataExtractor.Directory d in ImageMetadataReader.ReadMetadata(fileStream).ToList())
                    {
                        Dictionary<string, string> tmp = new Dictionary<string, string>();
                        foreach (MetadataExtractor.Tag t in d.Tags)
                        {
                            tmp.Add(t.Name, t.Description);
                        }
                        exif.Add(d.Name, tmp);
                    }
                    return exif;
                }
                else
                {
                    return exif;
                }
            }
            catch
            {
                return exif;
            }
        }

        public Dictionary<string, Dictionary<string, string>> getVideoInfo(Stream fileStream)
        {
            Dictionary<string, Dictionary<string, string>> exif = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                if (fileStream.CanSeek)
                {
                    byte[] buffer = new byte[64 * 1024];
                    int bufferSize = 0;
                    using (MediaInfo mediaInfo = new MediaInfo())
                    {
                        mediaInfo.Open_Buffer_Init(fileStream.Length, 0);

                        do
                        {
                            //Reading data somewhere, do what you want for this.
                            bufferSize = fileStream.Read(buffer, 0, 64 * 1024);

                            //Sending the buffer to MediaInfo
                            System.Runtime.InteropServices.GCHandle GC = System.Runtime.InteropServices.GCHandle.Alloc(buffer, System.Runtime.InteropServices.GCHandleType.Pinned);
                            IntPtr From_Buffer_IntPtr = GC.AddrOfPinnedObject();
                            Status Result = (Status)mediaInfo.Open_Buffer_Continue(From_Buffer_IntPtr, (IntPtr)bufferSize);
                            GC.Free();
                            if ((Result & Status.Finalized) == Status.Finalized)
                                break;

                            //Testing if MediaInfo request to go elsewhere
                            if (mediaInfo.Open_Buffer_Continue_GoTo_Get() != -1)
                            {
                                Int64 Position = fileStream.Seek(mediaInfo.Open_Buffer_Continue_GoTo_Get(), SeekOrigin.Begin); //Position the file
                                mediaInfo.Open_Buffer_Init(fileStream.Length, Position); //Informing MediaInfo we have seek
                            }
                        }
                        while (bufferSize > 0);

                        string t = mediaInfo.Option("Info_Parameters");
                        Dictionary<string, string> tmp = new Dictionary<string, string>();

                        if (mediaInfo.Count_Get(StreamKind.Video) != 0)
                        {
                            tmp = new Dictionary<string, string>();
                            tmp.Add("Title", mediaInfo.Get(StreamKind.Video, 0, "Title"));
                            tmp.Add("Width", mediaInfo.Get(StreamKind.Video, 0, "Width"));
                            tmp.Add("Height", mediaInfo.Get(StreamKind.Video, 0, "Height"));
                            tmp.Add("Duration", mediaInfo.Get(StreamKind.Video, 0, "Duration/String3"));
                            exif.Add("Video", tmp);
                        }

                        if (mediaInfo.Count_Get(StreamKind.Audio) != 0)
                        {
                            tmp = new Dictionary<string, string>();
                            tmp.Add("Title", mediaInfo.Get(StreamKind.Audio, 0, "Title"));
                            tmp.Add("Duration", mediaInfo.Get(StreamKind.Audio, 0, "Duration/String3"));
                            exif.Add("Audio", tmp);
                        }
                        return exif;
                    }
                }
                else
                {
                    return exif;
                }
            }
            catch
            {
                return exif;
            }
        }

        public FileInformation getFileInfo(ContentDescriptor contentDescriptor)
        {
            try
            {
                if (contentDescriptor.Name.ToLower().Equals("unstructureddata"))
                {
                    var fileInfo = getFileInfo(contentDescriptor.URI);
                    fileInfo.Description = contentDescriptor.Description;
                    return fileInfo;
                }
                else
                    return new FileInformation()
                    {
                        Name = contentDescriptor.Name,
                        Description = contentDescriptor.Description,
                        Path = contentDescriptor.URI,
                        MimeType = contentDescriptor.MimeType
                    };
            }
            catch
            {
                return null;
            }
        }

        public Stream getFileStream(string path)
        {
            path = Server.UrlDecode(path);
            if (FileHelper.FileExist(Path.Combine(AppConfiguration.DataPath, path)))
            {
                EntityPermissionManager entityPermissionManager = null;
                try
                {
                    entityPermissionManager = new EntityPermissionManager();
                    DatasetInfo datasetInfo = (DatasetInfo)Session["DatasetInfo"];
                    string entityType = (string)Session["EntityType"];
                    long datasetID = datasetInfo.DatasetId;
                    bool access = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read).Result;
                    if (access)
                    {
                        path = Path.Combine(AppConfiguration.DataPath, path);
                        Session["DatasetInfo"] = datasetInfo;
                        Session["EntityType"] = entityType;
                        return System.IO.File.OpenRead(path);
                    }
                    else
                    {
                        Session["DatasetInfo"] = datasetInfo;
                        Session["EntityType"] = entityType;
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
                finally
                {
                    entityPermissionManager.Dispose();
                }
            }
            else
            {
                try
                {
                    WebRequest request = WebRequest.Create(path);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    return response.GetResponseStream();
                }
                catch
                {
                    return null;
                }
            }
        }

        public FileInformation getFileInfo(string path)
        {
            Stream fileStream = getFileStream(path);
            FileInformation fileInfo = new FileInformation();

            if (fileStream != null)
            {
                FileStream fs = fileStream as FileStream;

                if (fs != null)
                {
                    fileInfo = new FileInformation(fs.Name.Split('\\').LastOrDefault(), MimeMapping.GetMimeMapping(fs.Name), (uint)fs.Length, path);
                }
                else
                {
                    WebRequest request = WebRequest.Create(path);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    fileInfo = new FileInformation(response.ResponseUri.Segments.LastOrDefault(), response.ContentType, (uint)response.ContentLength, path);
                }

                string[] mimeType = fileInfo.MimeType.Split('/');
                if (mimeType[1] == "tiff")
                    fileInfo.MimeType = "application/" + mimeType[1];

                switch (mimeType[0])
                {
                    case "image":
                        fileInfo.EXIF = getExif(fileStream);
                        break;

                    case "video":
                        fileInfo.EXIF = getVideoInfo(fileStream);
                        break;

                    case "audio":
                        fileInfo.EXIF = getVideoInfo(fileStream);
                        break;


                    default:
                        break;
                }
                fileStream.Close();
            }
            else
            {
                fileInfo = new FileInformation(path.Split('\\').LastOrDefault(), null, 0, path, null);
            }
            return fileInfo;
        }

        public List<FileInformation> getFilesByDatasetId(long datasetId, string entityType, long versionNo = 0)
        {
            EntityPermissionManager entityPermissionManager = null;
            DatasetManager datasetManager = null;
            try
            {
                entityPermissionManager = new EntityPermissionManager();
                datasetManager = new DatasetManager();
                bool access = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), datasetId, RightType.Read).Result;
                if (access)
                {
                    Session["EntityType"] = entityType;
                    return getFilesByDataset(datasetManager.DatasetRepo.Get(datasetId), datasetManager, entityType, versionNo);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                entityPermissionManager.Dispose();
                datasetManager.Dispose();
            }
        }

        public List<FileInformation> getFilesByDataset(Dataset dataset, DatasetManager datasetManager, string entityType, long versionId = 0)
        {
            EntityPermissionManager entityPermissionManager = null;
            try
            {
                List<FileInformation> fileInfos = new List<FileInformation>();
                entityPermissionManager = new EntityPermissionManager();
                bool access = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), dataset.Id, RightType.Read).Result;
                if (dataset != null && access)
                {
                    DatasetVersion datasetVersion = new DatasetVersion();
                    if (versionId > 0)
                        datasetVersion = datasetManager.GetDatasetVersion(versionId);
                    else
                        datasetVersion = datasetManager.GetDatasetLatestVersion(dataset);

                    if (datasetVersion != null)
                    {
                        List<ContentDescriptor> contentDescriptors = datasetVersion.ContentDescriptors.ToList();

                        if (contentDescriptors.Count > 0)
                        {
                            foreach (ContentDescriptor cd in contentDescriptors)
                            {
                                if (cd.Name.ToLower().Equals("unstructureddata"))
                                    fileInfos.Add(getFileInfo(cd));
                            }
                        }
                    }
                }
                return fileInfos;
            }
            finally
            {
                entityPermissionManager.Dispose();
            }
        }

        public ActionResult ImageView(string path)
        {
            path = Server.UrlDecode(path);
            return PartialView("_imageView", getFileInfo(path));
        }

        public ActionResult DocumentView(string path)
        {
            path = Server.UrlDecode(path);
            return PartialView("_documentView", getFileInfo(path));
        }
       

        private Measurement getParent(List<Measurement> measurements, long parentId)
        {
            foreach (Measurement m in measurements)
            {
                if (m.Id == parentId)
                    return m;
                else if (m.Children != null)
                    getParent(m.Children, parentId);
            }
            return new Measurement();
        }

        private XmlDocument setSystemValuesToMetadata(long version, long datasetId, long metadataStructureId, XmlDocument metadata)
        {
            SystemMetadataHelper SystemMetadataHelper = new SystemMetadataHelper();

            Key[] myObjArray = { };

            myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.DataLastModified };

            metadata = SystemMetadataHelper.SetSystemValuesToMetadata(datasetId, version, metadataStructureId, metadata, myObjArray);

            return metadata;
        }

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }
    }
}