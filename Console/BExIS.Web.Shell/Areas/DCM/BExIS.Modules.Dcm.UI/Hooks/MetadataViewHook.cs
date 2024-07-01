using BExIS.UI.Hooks;

namespace BExIS.Modules.Dcm.UI.Hooks
{
    public class MetadataViewHook : Hook
    {
        public MetadataViewHook()
        {
            Start = "/dcm/view/start";
        }

        public override void Check(long id, string username)
        {
            // check status
            checkStatus(id, username);
        }

        private void checkStatus(long id, string username)
        {
            //Metadata view is always open
            Status = HookStatus.Open;
        }
    }
}