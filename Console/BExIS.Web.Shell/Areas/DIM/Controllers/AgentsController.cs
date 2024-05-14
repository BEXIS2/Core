using BExIS.Dim.Entities.Submissions;
using BExIS.Dim.Services.Publication;
using BExIS.Dim.Services.Submissions;
using BExIS.Modules.Dim.UI.Models.Submissions;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.Utils.NH.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class AgentsController : Controller
    {
        // GET: Agents
        public ActionResult Index()
        {
            return View();
        }


        [GridAction(EnableCustomBinding = true)]
        public ActionResult Agents_Select(GridCommand command)
        {
            using (var agentManager = new AgentManager())
            {
                try
                {
                    var agents = new List<ReadGridRowAgentModel>();
                    int count = agentManager.Agents.Count();
                    if (command != null)// filter subjects based on grid filter settings
                    {
                        FilterExpression filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                        OrderByExpression orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());

                        agents = agentManager.Find(filter, orderBy, command.Page, command.PageSize, out count).Select(ReadGridRowAgentModel.Convert).ToList();
                    }
                    else
                    {
                        agents = agentManager.Agents.Select(ReadGridRowAgentModel.Convert).ToList();
                        count = agentManager.Agents.Count();
                    }

                    return View(new GridModel<ReadGridRowAgentModel> { Data = agents, Total = count });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public ActionResult Create()
        {
            return PartialView("_Create", new CreateAgentModel());
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateAgentModel model)
        {
            using (var agentManager = new AgentManager())
            using (var repositoryManager = new RepositoryManager())
            {
                if (!ModelState.IsValid) return PartialView("_Create", model);

                var agent = new Agent()
                {
                    Name = model.Name,
                    Username = model.Username,
                    Password = model.Password,
                    Repository = repositoryManager.FindById(model.RepositoryId)
                };

                var result = agentManager.Create(agent);
                if (result != null)
                {
                    return Json(new { success = true });
                }

                AddErrors(result);

                return PartialView("_Create", model);
            }

        }
    }
}