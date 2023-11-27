using BExIS.UI.Models;
using System.Collections.Generic;

namespace BExIS.UI.Helpers
{
    public static class BexisModelManager
    {
        #region EntitySelectorModel

        public static EntitySelectorModel LoadEntitySelectorModel<T>(this IList<T> data)
        {
            EntitySelectorModel tmp = new EntitySelectorModel();

            tmp.Data = BexisDataHelper.ToDataTable<T>(data);
            tmp.IDKey = "Id";
            return tmp;
        }

        public static EntitySelectorModel LoadEntitySelectorModel<T>(this IList<T> data, string idKey = "Id")
        {
            EntitySelectorModel tmp = new EntitySelectorModel();

            tmp.Data = BexisDataHelper.ToDataTable<T>(data);
            tmp.IDKey = idKey;
            return tmp;
        }

        public static EntitySelectorModel LoadEntitySelectorModel<T>(this IList<T> data, EntitySelectorModelAction reciever, string idKey = "Id", string targetId = "")
        {
            EntitySelectorModel tmp = new EntitySelectorModel();

            tmp.Data = BexisDataHelper.ToDataTable<T>(data);
            tmp.Reciever = reciever;
            tmp.IDKey = idKey;
            tmp.TargetId = targetId;

            return tmp;
        }

        public static EntitySelectorModel LoadEntitySelectorModel<T>(this IList<T> data, List<string> columns, EntitySelectorModelAction reciever, string idKey = "Id", string targetId = "", Dictionary<string, string> parameters = null)
        {
            EntitySelectorModel tmp = new EntitySelectorModel();

            tmp.Data = BexisDataHelper.ToDataTable<T>(data, columns);
            tmp.Reciever = reciever;
            tmp.IDKey = idKey;
            tmp.TargetId = targetId;
            tmp.Parameters = parameters;
            return tmp;
        }

        #endregion EntitySelectorModel
    }
}