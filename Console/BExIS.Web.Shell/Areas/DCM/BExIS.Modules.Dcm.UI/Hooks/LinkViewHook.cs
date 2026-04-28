using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class LinkViewHook : Hook
    {
        public LinkViewHook()
        {
            Start = "/dcm/entityreference/startview";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkStatus(id, username);
        }

        private void checkStatus(long id, string username)
        {

             Status = HookStatus.Open;

        }
    }
}