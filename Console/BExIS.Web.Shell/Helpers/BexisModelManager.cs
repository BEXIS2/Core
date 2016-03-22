using BExIS.Web.Shell.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Helpers
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

        public static EntitySelectorModel LoadEntitySelectorModel<T>(this IList<T> data, EntitySelectorModelAction reciever, string idKey="Id", string onClick="")
        {
            EntitySelectorModel tmp = new EntitySelectorModel();

            tmp.Data = BexisDataHelper.ToDataTable<T>(data);
            tmp.Reciever = reciever;
            tmp.IDKey = idKey;

            return tmp;
        }

        public static EntitySelectorModel LoadEntitySelectorModel<T>(this IList<T> data, List<string> columns, EntitySelectorModelAction reciever, string idKey = "Id", string onClick = "")
        {
            EntitySelectorModel tmp = new EntitySelectorModel();

            tmp.Data = BexisDataHelper.ToDataTable<T>(data, columns);
            tmp.Reciever = reciever;
            tmp.IDKey = idKey;
 
            return tmp;
        }

        #endregion


    }
}