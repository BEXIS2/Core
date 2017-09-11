using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity;

namespace BExIS.Security.Services.Subjects
{
    public class IdentityGroupManager : RoleManager<Group, long>
    {
        public IdentityGroupManager() : base(new GroupStore())
        {
            RoleValidator = new RoleValidator<Group, long>(this)
            {

            };
        }
    }
}
