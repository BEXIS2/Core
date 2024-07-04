using Vaiona.Entities.Common;

namespace BExIS.Ext.Model
{
    public class BexisException
    {
        public static void Throw(object entityObj = null, string reason = null, ExceptionType exceptionType = ExceptionType.None, bool GroupFailed = false)
        {
            //What should be happen here
            if (entityObj == null)
                throw new System.Exception(reason);
            var entity = (BaseEntity)entityObj;
            throw new System.Exception(string.Format("{0} {1} (ID: {2}) failed. Reason: {3} . {4}", exceptionType.ToString(), entityObj.GetType().Name, entity.Id, reason, GroupFailed ? " All operations canceled" : ""));
        }

        public enum ExceptionType
        {
            None = -1,
            Add = 0,
            Edit = 1,
            Delete = 2
        }
    }
}