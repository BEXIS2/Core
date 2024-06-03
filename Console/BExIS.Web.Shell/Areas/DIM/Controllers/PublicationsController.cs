//using System.Linq.Dynamic;
using BExIS.Dim.Services.Publications;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Services.Subjects;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
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
    }
}
