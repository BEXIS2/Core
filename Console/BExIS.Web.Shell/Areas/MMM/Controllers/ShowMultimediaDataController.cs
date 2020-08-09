﻿using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Hosting;
using Vaiona.Utils.Cfg;
using System.Net;
using MetadataExtractor;
using IDIV.Modules.Mmm.UI.Models;
using MediaInfoLib;
using System.Xml;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Entities.Authorization;
using Vaiona.Persistence.Api;
using Vaiona.Entities.Common;
using BExIS.Utils.Data.Upload;

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

                ViewData["edit"] = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Write);

                bool access = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read);

                if (access)
                {
                    
                    return View(getFilesByDatasetId(datasetID , entityType));
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

            EntityPermissionManager entityPermissionManager = null;
            DatasetManager datasetManager = null;
            try
            {
                entityPermissionManager = new EntityPermissionManager();
                datasetManager = new DatasetManager();
                bool isLatestVersion = false;
                if (versionId == datasetManager.GetDatasetLatestVersion(datasetID).Id)
                    isLatestVersion = true;
                
                ViewData["edit"] = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Write);

                bool access = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read);
                Session["DatasetInfo"] = new DatasetInfo(datasetID, versionId, isLatestVersion, access, entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Delete));
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
                entityPermissionManager.Dispose();
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

                case "bundle":
                    FileInformation fileInfo = getFileInfo(Server.UrlDecode(path));
                    if (fileInfo.MimeType == "application/x-zip-compressed")
                    {
                        ZipFile zipFile = new ZipFile(getFileStream(Server.UrlDecode(path)));
                        foreach (ZipEntry zipEntry in zipFile)
                        {
                            if (zipEntry.IsFile)
                            {
                                if (zipEntry.Name.Length > 0 && zipEntry.Name.ToLower() == ("Manifest.xml").ToLower())
                                {
                                    List<KeyValuePair<string, string>> tmp = new List<KeyValuePair<string, string>>();
                                    Stream zipStream = zipFile.GetInputStream(zipEntry);
                                    XmlDocument doc = new XmlDocument();
                                    doc.Load(zipStream);
                                    XmlDocument tmpXml = new XmlDocument();
                                    XmlNode imgs = doc.GetElementsByTagName("Images")[0];
                                    for (int i = 0; i < imgs.ChildNodes.Count && i < 5; i++)
                                    {
                                        foreach (XmlNode xn in imgs.ChildNodes[i])
                                        {
                                            if (xn.Name == "Thumbnail")
                                            {
                                                tmpXml.LoadXml(xn.OuterXml);
                                                tmp.Add(new KeyValuePair<string, string>(path, tmpXml.GetElementsByTagName("File")[0].InnerText));
                                            }
                                        }
                                    }
                                    return PartialView("_bundlePreview", tmp);
                                }
                            }
                        }
                    }
                    return PartialView("_defaultPreview", path);

                default:
                    return PartialView("_defaultPreview", path);
            }
        }

        public FileResult getFile(string path)
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
                    bool access = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read);
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

                        return File(path, MimeMapping.GetMimeMapping(fileInfo.Name), filename);
                    }
                    else
                    {
                        Session["DatasetInfo"] = datasetInfo;
                        return null;
                    }
                }
                catch(Exception ex)
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
                EntityPermissionManager entityPermissionManager = null;
                DatasetManager datasetManager = null;
                try
                {
                    entityPermissionManager = new EntityPermissionManager();
                    datasetManager = new DatasetManager();
                    DatasetInfo datasetInfo = (DatasetInfo)Session["DatasetInfo"];
                    string entityType = (string)Session["EntityType"];

                    DatasetVersion workingCopy = new DatasetVersion();
                    string status = DatasetStateInfo.NotValid.ToString();
                    string[] temp = path.Split('\\');
                    long datasetID = datasetInfo.DatasetId;
                    status = datasetManager.GetDatasetLatestVersion(datasetID).StateInfo.State;
                    bool access = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Delete);

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
                finally
                {
                    entityPermissionManager.Dispose();
                }
            }
        }

        public FileResult getFileFromZip(string path, string file)
        {
            ZipFile zipFile = new ZipFile(getFileStream(Server.UrlDecode(path)));
            foreach (ZipEntry zipEntry in zipFile)
            {
                if (zipEntry.IsFile)
                {
                    if (zipEntry.Name == Server.UrlDecode(file))
                        return File(zipFile.GetInputStream(zipEntry), MimeMapping.GetMimeMapping(zipEntry.Name), zipEntry.Name);
                }
            }
            return null;
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
                    bool access = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read);
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
                    MediaInfo mediaInfo = new MediaInfo();
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

        public Dictionary<string, Dictionary<string, string>> getBundleInfo(Stream fileStream)
        {
            Dictionary<string, Dictionary<string, string>> exif = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                ZipFile zipFile = new ZipFile(fileStream);

                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (zipEntry.IsFile)
                    {
                        if (zipEntry.Name.Length > 0 && zipEntry.Name.ToLower() == ("Manifest.xml").ToLower())
                        {
                            Dictionary<string, string> tmp = new Dictionary<string, string>();
                            Stream zipStream = zipFile.GetInputStream(zipEntry);
                            XmlDocument doc = new XmlDocument();
                            doc.Load(zipStream);

                            tmp.Add("Name", doc.GetElementsByTagName("Name")[0].InnerText.ToString());
                            tmp.Add("Description", doc.GetElementsByTagName("Description")[0].InnerText.ToString());
                            exif.Add("Bundle", tmp);

                            return exif;
                        }
                    }
                }
                return exif;
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
                    return getFileInfo(contentDescriptor.URI);
                else
                    return new FileInformation()
                    {
                        Name = contentDescriptor.Name,
                        Path = contentDescriptor.URI,
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
                    bool access = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read);
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

                    case "application":
                        if (fileInfo.MimeType == "application/x-zip-compressed")
                        {
                            fileInfo.EXIF = getBundleInfo(fileStream);
                        }
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
                bool access = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetId, RightType.Read);
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

        public List<FileInformation> getFilesByDataset(Dataset dataset, DatasetManager datasetManager,string entityType, long versionId = 0)
        {
            EntityPermissionManager entityPermissionManager = null;
            try
            {
                List<FileInformation> fileInfos = new List<FileInformation>();
                entityPermissionManager = new EntityPermissionManager();
                bool access = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), dataset.Id, RightType.Read);
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

        public ActionResult BundleView(string path)
        {
            path = Server.UrlDecode(path);
            BundleInformation bundleInfo = new BundleInformation();
            try
            {
                ZipFile zipFile = new ZipFile(getFileStream(path));
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (zipEntry.IsFile)
                    {
                        if (zipEntry.Name.Length > 0 && zipEntry.Name.ToLower() == ("Manifest.xml").ToLower())
                        {
                            bundleInfo.BundlePath = path;
                            Stream zipStream = zipFile.GetInputStream(zipEntry);
                            XmlDocument doc = new XmlDocument();
                            doc.Load(zipStream);
                            XmlDocument tmpXml = new XmlDocument();
                            XmlNode imgs = doc.GetElementsByTagName("Images")[0];
                            for (int i = 0; i < imgs.ChildNodes.Count; i++)
                            {
                                ImageInformation imageInfo = new ImageInformation();
                                imageInfo.BundlePath = bundleInfo.BundlePath;
                                foreach (XmlNode xn in imgs.ChildNodes[i])
                                {
                                    if (xn.Name == "Name")
                                        imageInfo.Name = xn.InnerText;
                                    if (xn.Name == "Original")
                                    {
                                        tmpXml.LoadXml(xn.OuterXml);
                                        imageInfo.Original = tmpXml.GetElementsByTagName("File")[0].InnerText;
                                    }
                                    if (xn.Name == "Thumbnail")
                                    {
                                        tmpXml.LoadXml(xn.OuterXml);
                                        imageInfo.Thumbnail = tmpXml.GetElementsByTagName("File")[0].InnerText;
                                    }
                                }
                                imageInfo.Measurements = getMeasurements(path, imageInfo.Name);
                                bundleInfo.Images.Add(imageInfo);
                            }
                        }
                    }
                }
                return PartialView("_bundleView", bundleInfo);
            }
            catch
            {
                return null;
            }
        }

        public ActionResult overlayBinding(string path, string Name)
        {
            return PartialView("_bundleViewOverlay", getShapes(path, Name));
        }

        public List<Shape> getShapes(string path, string imgName)
        {
            List<Shape> shapes = new List<Shape>();
            double x = 0;
            double y = 0;
            ZipFile zipFile = new ZipFile(getFileStream(path));
            string file = "";

            foreach (ZipEntry zipEntry in zipFile)
            {
                if (zipEntry.IsFile)
                {
                    if (zipEntry.Name.Length > 0 && zipEntry.Name.ToLower() == ("Manifest.xml").ToLower())
                    {
                        Stream zipStream = zipFile.GetInputStream(zipEntry);
                        XmlDocument doc = new XmlDocument();
                        doc.Load(zipStream);

                        XmlNode imgs = doc.GetElementsByTagName("Images")[0];
                        XmlDocument tmp = new XmlDocument();
                        for (int i = 0; i < imgs.ChildNodes.Count; i++)
                        {
                            tmp.LoadXml(imgs.ChildNodes[i].OuterXml);
                            if (tmp.GetElementsByTagName("Name")[0].InnerText.ToLower() == imgName.ToLower())
                            {
                                tmp.LoadXml(tmp.GetElementsByTagName("Shape")[0].OuterXml);
                                file = tmp.GetElementsByTagName("File")[0].InnerText;
                                Double.TryParse(tmp.GetElementsByTagName("Resolution")[0].InnerText.Split(',')[0], out x);
                                Double.TryParse(tmp.GetElementsByTagName("Resolution")[0].InnerText.Split(',')[1], out y);
                            }
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(file))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (zipEntry.IsFile)
                    {
                        if (zipEntry.Name.Length > 0 && zipEntry.Name.ToLower() == (file).ToLower())
                        {
                            Stream zipStream = zipFile.GetInputStream(zipEntry);
                            using (TextFieldParser parser = new TextFieldParser(zipStream))
                            {
                                parser.TextFieldType = FieldType.Delimited;
                                parser.SetDelimiters(";");
                                List<string> columns = parser.ReadFields().ToList();
                                long nameIndex = columns.IndexOf("imgName");
                                long polygonId = 0;
                                Point point = new Point();
                                Shape shape = new Shape();

                                while (!parser.EndOfData)
                                {
                                    string[] fields = parser.ReadFields();
                                    if (nameIndex >= 0 && fields[nameIndex].ToLower() == imgName.ToLower() && Convert.ToInt64(fields[columns.IndexOf("polygonId")]) != polygonId)
                                    {
                                        if (shape.Id > 0)
                                            shapes.Add(shape);
                                        Int64.TryParse(fields[columns.IndexOf("polygonId")], out polygonId);
                                        shape = new Shape();
                                        shape.x = x;
                                        shape.y = y;
                                        Int64.TryParse(fields[columns.IndexOf("polygonId")], out shape.Id);
                                        Int64.TryParse(fields[columns.IndexOf("objReference")], out shape.objRef);
                                    }
                                    if (Int64.TryParse(fields[columns.IndexOf("polygonId")], out polygonId) && Convert.ToInt64(fields[columns.IndexOf("polygonId")]) == polygonId)
                                    {
                                        point = new Point();
                                        Int64.TryParse(fields[columns.IndexOf("pointId")], out point.Id);
                                        Double.TryParse(fields[columns.IndexOf("x")], out point.x);
                                        Double.TryParse(fields[columns.IndexOf("y")], out point.y);
                                        shape.Points.Add(point);
                                    }
                                }
                                if (shape.Id > 0)
                                    shapes.Add(shape);
                            }
                        }
                    }
                }
            }
            return shapes;
        }

        public List<Measurement> getMeasurements(string path, string imgName)
        {
            string file = "";
            List<Measurement> measurements = new List<Measurement>();
            ZipFile zipFile = new ZipFile(getFileStream(path));
            foreach (ZipEntry zipEntry in zipFile)
            {
                if (zipEntry.IsFile)
                {
                    if (zipEntry.Name.Length > 0 && zipEntry.Name.ToLower() == ("Manifest.xml").ToLower())
                    {
                        Stream zipStream = zipFile.GetInputStream(zipEntry);
                        XmlDocument doc = new XmlDocument();
                        doc.Load(zipStream);

                        XmlNode imgs = doc.GetElementsByTagName("Images")[0];
                        XmlDocument tmp = new XmlDocument();
                        for (int i = 0; i < imgs.ChildNodes.Count; i++)
                        {
                            tmp.LoadXml(imgs.ChildNodes[i].OuterXml);
                            if (tmp.GetElementsByTagName("Name")[0].InnerText.ToLower() == imgName.ToLower())
                            {
                                tmp.LoadXml(tmp.GetElementsByTagName("Data")[0].OuterXml);
                                file = tmp.GetElementsByTagName("File")[0].InnerText;
                            }
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(file))
            {
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (zipEntry.IsFile)
                    {
                        if (zipEntry.Name.Length > 0 && zipEntry.Name.ToLower() == (file).ToLower())
                        {
                            Stream zipStream = zipFile.GetInputStream(zipEntry);
                            using (TextFieldParser parser = new TextFieldParser(zipStream))
                            {
                                parser.TextFieldType = FieldType.Delimited;
                                parser.SetDelimiters(",");
                                List<string> columns = parser.ReadFields().ToList();
                                Measurement measurement = new Measurement();

                                while (!parser.EndOfData)
                                {
                                    string[] fields = parser.ReadFields();

                                    measurement = new Measurement();
                                    Int64.TryParse(fields[columns.IndexOf("obj")], out measurement.Id);
                                    measurement.Type = fields[columns.IndexOf("type")];
                                    if (columns.IndexOf("length") >= 0 && !String.IsNullOrEmpty(fields[columns.IndexOf("length")]))
                                    {
                                        Double.TryParse(fields[columns.IndexOf("length")], out measurement.Length);
                                        measurement.Length = Math.Round(measurement.Length, 2);
                                    }

                                    if (columns.IndexOf("width") >= 0 && !String.IsNullOrEmpty(fields[columns.IndexOf("width")]))
                                    {
                                        Double.TryParse(fields[columns.IndexOf("width")], out measurement.Width);
                                        measurement.Width = Math.Round(measurement.Width, 2);
                                    }
                                    if (columns.IndexOf("area") >= 0 && !String.IsNullOrEmpty(fields[columns.IndexOf("area")]))
                                    {
                                        Double.TryParse(fields[columns.IndexOf("area")], out measurement.Area);
                                        measurement.Area = Math.Round(measurement.Area, 2);
                                    }
                                    if (columns.IndexOf("perimeter") >= 0 && !String.IsNullOrEmpty(fields[columns.IndexOf("perimeter")]))
                                    {
                                        Double.TryParse(fields[columns.IndexOf("perimeter")], out measurement.Perimeter);
                                        measurement.Perimeter = Math.Round(measurement.Perimeter, 2);
                                    }
                                    if (columns.IndexOf("circularity") >= 0 && !String.IsNullOrEmpty(fields[columns.IndexOf("circularity")]))
                                    {
                                        Double.TryParse(fields[columns.IndexOf("circularity")], out measurement.Circularity);
                                        measurement.Circularity = Math.Round(measurement.Circularity, 2);
                                    }

                                    if (columns.IndexOf("parent") >= 0 && !String.IsNullOrEmpty(fields[columns.IndexOf("parent")]))
                                    {
                                        Measurement parent = getParent(measurements, Convert.ToInt64(fields[columns.IndexOf("parent")]));
                                        parent.Children.Add(measurement);
                                    }
                                    else
                                    {
                                        measurements.Add(measurement);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return measurements;
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
    }
}