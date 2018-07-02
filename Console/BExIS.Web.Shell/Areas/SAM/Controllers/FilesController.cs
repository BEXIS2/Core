using BExIS.Modules.Sam.UI.Helpers;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class FilesController : BaseController
    {
        


        /// <summary>
        /// Loads the files and folders structure from the current tenants files.catalog file and renders a tree view from it.
        /// The tree view is actionable, so that a user can traverse the folder structure and browse the files and folders in any selected folder.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("File Manager", this.Session.GetTenant());
            FileManager fileManger = new FileManager(this.Session.GetTenant().Id);
            fileManger.Load();
            return View(new List<FolderModel>() { fileManger.TreeRoot });
        }

        public ActionResult FolderContent(string path)
        {
            return PartialView("_FolderContent", path);
        }

        public ActionResult FolderTree()
        {
            FileManager fileManger = new FileManager(this.Session.GetTenant().Id);
            fileManger.Load();
            return PartialView("_FolderTree", new List<FolderModel>() { fileManger.TreeRoot });
        }
        
        [GridAction]
        public ActionResult FolderContent_Select(string path)
        {
            FileManager fileManger = new FileManager(this.Session.GetTenant().Id);
            //fileManger.Load(); // loading should not be needed here
            List<FileOrFolderModel> rows = fileManger.GetDirectChildrenByFolderPath(path);
            return View(new GridModel<FileOrFolderModel> { Data = rows });
        }

        public ActionResult CreateFolder(string path)
        {
            FolderModel model = new FolderModel() { Path = path };
            return PartialView("_CreateFolder", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFolder(FolderModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Path))
            {
                AddErrors(new string[] { "No parent path is selected." });
            }
            if (ModelState.IsValid)
            {
                // the current selected node of the tree must be known, it is in the Path attribute
                // add the folder to the right location on file system and in the catalog, check for duplicates
                try
                {
                    FileManager fileManger = new FileManager(this.Session.GetTenant().Id);
                    fileManger.AddFolder(model.Name, model.DisplayName, model.Description, model.Path);
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    AddErrors(new string[] { ex.Message });
                }
            }
            return PartialView("_CreateFolder", model);
        }

        [HttpPost]
        public ActionResult Delete(string path)
        {
            try
            {
                FileManager fileManger = new FileManager(this.Session.GetTenant().Id);
                fileManger.Delete(path);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message});
            }
        }

        public ActionResult UploadFile(string path)
        {
            FileModel model = new FileModel() { Name = "empty", Path = path, MimeType = "application/text" };
            return PartialView("_UploadFile", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFile(FileModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Path))
            {
                AddErrors(new string[] { "No parent path is selected." });
            }
            if(Request.Files.Count != 1)
                AddErrors(new string[] { "No or more than one file(s) is chosen." });

            HttpPostedFileBase file = Request.Files[0];
            model.MimeType = determineMimeType(file.ContentType, file.FileName);

            if (ModelState.IsValid)
            {
                // the current selected node of the tree must be known, it is in the Path attribute
                // add the folder to the right location on file system and in the catalog, check for duplicates
                try
                {
                    FileManager fileManger = new FileManager(this.Session.GetTenant().Id);
                    fileManger.AddFile(file.FileName, model.DisplayName, model.Description, model.MimeType, model.Path, file);
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    AddErrors(new string[] { string.Concat(ex.Message, ex.InnerException != null ? " " + ex.InnerException.Message : "") });
                }
            }
            return PartialView("_UploadFile", model);
        }

        private string determineMimeType(string contentType, string fileName)
        {
            return "application/text";
        }

        private void AddErrors(string[] errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}