//using System.Linq.Dynamic;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.Export;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Services.Publications;
using BExIS.Dlm.Entities.Data;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Services.Subjects;
using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class PublicationsController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return PartialView("_Create", new CreatePublicationModel());
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreatePublicationModel model)
        {
            return null;
        }

        [HttpPost]
        public async Task<bool> Delete(long publicationId)
        {
            try
            {
                using (var publicationManager = new PublicationManager())
                {
                    return publicationManager.DeleteById(publicationId);
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public ActionResult Update(long publicationId)
        {
            try
            {
                using(var publicationManager = new PublicationManager())
                {
                    var publication = publicationManager.FindById(publicationId);
                    return PartialView("_Update", UpdatePublicationModel.Convert(publication));
                }                
            }
            catch(Exception ex)
            {
                // [TODO]
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult> Update(UpdatePublicationModel model)
        {
            var groupManager = new GroupManager();

            try
            {
                using(var publicationManager = new PublicationManager())
                {
                    // check wheter model is valid or not
                    if (!ModelState.IsValid) return PartialView("_Update", model);

                    // check if a publication with the incoming id exist
                    var publication = publicationManager.FindById(model.Id);
                    if (publication == null) return PartialView("_Update", model);

                    // Update all relevant properties of publication
                    // [TODO]
                }

                return null;
            }
            finally
            {
                groupManager.Dispose();
            }
        }

        public async Task<ActionResult> Download(long publicationId)
        {
            try
            {
                using (var publicationManager = new PublicationManager())
                {
                    var publication = publicationManager.FindById(publicationId);

                    if (publication == null)
                        throw new ArgumentException("Publication does not exist", nameof(publicationId));

                    Tuple<string, string> filePath = null;

                    switch (publication.Broker.Name.ToLower())
                    {
                        case "gbif":

                            GbifDataType gbifDataType = GbifDataType.occurrence;

                            switch (publication.Broker.Type.ToLower())
                            {
                                case "occurrence":
                                    gbifDataType = GbifDataType.occurrence;
                                    break;

                                case "samplingevent":
                                    gbifDataType = GbifDataType.samplingEvent;
                                    break;

                                default:
                                    break;
                            }

                            GBIFDataRepoConverter dataRepoConverter = new GBIFDataRepoConverter(publication.Broker, gbifDataType);
                            filePath = new Tuple<string, string>(dataRepoConverter.Convert(publication.DatasetVersion.Id), "application/zip");
                            break;

                        case "pangeae":

                            break;

                        default:
                            return Json(new { success = false, message = "The requested file was not found." }, JsonRequestBehavior.AllowGet);
                    }

                    if(filePath == null)
                        return Json(new { success = false, message = "The requested file was not found." }, JsonRequestBehavior.AllowGet);

                    byte[] fileBytes;
                    using (var fileStream = new FileStream(filePath.Item1, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    {
                        fileBytes = new byte[fileStream.Length];
                        await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);
                    }

                    // Determine the content type
                    string contentType = MimeMapping.GetMimeMapping(filePath.Item2);

                    // Return the file as a download
                    return File(fileBytes, contentType, Path.GetFileName(filePath.Item1));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
