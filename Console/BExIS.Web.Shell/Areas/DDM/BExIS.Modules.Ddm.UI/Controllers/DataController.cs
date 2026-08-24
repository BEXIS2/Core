using System.Drawing;
using ICSharpCode.SharpZipLib.Zip;
using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc;


namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DataController : BaseController
    {
        private readonly UserManager _userManager;

        public DataController(UserManager userManager)
        {
            _userManager = userManager;
        }
        public ActionResult ShowData(long id, int version = 0, bool asPartial = false, string versionName = "", double tag = 0)
        {

            string url = $"/dcm/view/?id={id}";
            var queryParams = new List<string>();
            if (version > 0) queryParams.Add($"version={version}");
            if (!string.IsNullOrEmpty(versionName)) queryParams.Add($"versionName={Uri.EscapeDataString(versionName)}");
            if (tag > 0) queryParams.Add($"tag={tag}");
            if (queryParams.Count > 0) url += "&" + string.Join("&", queryParams);
            return Redirect(url);
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult DownloadFile(long id, long version, string path, string mimeType, bool preview = false)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            {
                if (hasUserRights(id, RightType.Read))
                {
                    string title = id + "_" + version + "_" + path.Split('\\').Last();
                    long versionNr = datasetManager.GetDatasetVersionNr(version);

                    if (!preview)
                    {
                        string message = string.Format("dataset {0} version {1} was downloaded as excel.", id, versionNr);
                        LoggerFactory.LogCustom(message);

                        using (var emailService = new EmailService())
                        {
                            emailService.Send(MessageHelper.GetDownloadDatasetHeader(id, versionNr),
                            MessageHelper.GetDownloadDatasetMessage(id, title, GetDisplayName(), mimeType, versionNr),
                                GeneralSettings.SystemEmail
                                );
                        }
                    }

                    var filePath = Path.Combine(AppConfiguration.DataPath, path);

                    if (preview && !string.IsNullOrEmpty(mimeType) && mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        var resizedPath = GetResizedImagePreview(filePath, 1200);
                        if (!string.IsNullOrEmpty(resizedPath))
                            return File(resizedPath, mimeType);
                    }

                    if (preview)
                    {
                        // return inline (no Content-Disposition: attachment) so browsers display it
                        return File(filePath, mimeType);
                    }

                    return File(filePath, mimeType, title);
                }
            }

            return Content("User has no rights.");
        }

        /// <summary>
        /// Creates a resized preview image (max dimension) and caches it on disk.
        /// Returns the path to the cached resized image, or null if resizing failed.
        /// </summary>
        private string GetResizedImagePreview(string originalPath, int maxDimension)
        {
            try
            {
                if (!System.IO.File.Exists(originalPath))
                    return null;

                var previewDir = Path.Combine(Path.GetDirectoryName(originalPath), ".previews");
                if (!Directory.Exists(previewDir))
                    Directory.CreateDirectory(previewDir);

                var resizedName = $"{Path.GetFileNameWithoutExtension(originalPath)}_{maxDimension}{Path.GetExtension(originalPath)}";
                var resizedPath = Path.Combine(previewDir, resizedName);

                // check cache — if resized file exists and is newer than original, use it
                if (System.IO.File.Exists(resizedPath) &&
                    System.IO.File.GetLastWriteTime(resizedPath) >= System.IO.File.GetLastWriteTime(originalPath))
                {
                    return resizedPath;
                }

                using (var originalImage = System.Drawing.Image.FromFile(originalPath))
                {
                    int width = originalImage.Width;
                    int height = originalImage.Height;

                    // skip resizing if already small enough
                    if (width <= maxDimension && height <= maxDimension)
                        return originalPath;

                    // calculate new dimensions maintaining aspect ratio
                    if (width > height)
                    {
                        height = (int)((float)maxDimension / width * height);
                        width = maxDimension;
                    }
                    else
                    {
                        width = (int)((float)maxDimension / height * width);
                        height = maxDimension;
                    }

                    using (var bitmap = new System.Drawing.Bitmap(width, height))
                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.DrawImage(originalImage, 0, 0, width, height);

                        var format = originalImage.RawFormat;
                        bitmap.Save(resizedPath, format);
                    }
                }

                return resizedPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image resize failed: {ex.Message}");
                return null;
            }
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public JsonResult GetZipContents(long id, string path)
        {
            var result = new List<object>();
            var filePath = Path.Combine(AppConfiguration.DataPath, path);

            if (!System.IO.File.Exists(filePath))
                return Json(result, JsonRequestBehavior.AllowGet);

            try
            {
                using (var fileStream = System.IO.File.OpenRead(filePath))
                using (var zipFile = new ZipFile(fileStream))
                {
                    foreach (ZipEntry entry in zipFile)
                    {
                        if (!entry.IsFile) continue;
                        result.Add(new
                        {
                            name = entry.Name,
                            size = entry.Size,
                            compressedSize = entry.CompressedSize,
                            date = entry.DateTime
                        });
                    }
                }
            }
            catch
            {
                // not a valid zip file
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private bool hasUserRights(long entityId, RightType rightType)
        {
            #region security permissions and authorizations check

            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            return entityPermissionManager.HasEffectiveRightsAsync(GetUsernameOrDefault(), typeof(Dataset), entityId, rightType).Result;

            #endregion security permissions and authorizations check
        }

        public string GetDisplayName()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
                User user = _userManager.FindByNameAsync(username).Result;

                return user.DisplayName;
            }
            catch
            {
                return "DEFAULT";
            }
        }
        public string GetUsernameOrDefault()
        {
            var username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

    }
}

         