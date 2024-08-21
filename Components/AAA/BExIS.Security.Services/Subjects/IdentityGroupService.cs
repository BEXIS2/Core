using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity;

namespace BExIS.Security.Services.Subjects
{
    public class IdentityGroupService : RoleManager<Group, long>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        public IdentityGroupService() : base(new GroupManager())
        {
            RoleValidator = new RoleValidator<Group, long>(this)
            {
            };
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Store.Dispose();
        }
    }
}