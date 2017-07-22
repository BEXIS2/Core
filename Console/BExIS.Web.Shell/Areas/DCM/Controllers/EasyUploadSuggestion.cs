using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class EasyUploadSuggestion
    {
        public string attributeName;
        public long unitID;
        public long dataTypeID;
        public string unitName;
        public string datatypeName;
        public Boolean show;
        
        public EasyUploadSuggestion(string attributeName, long unitID, long dataTypeID, string unitName, string datatypeName, Boolean show)
        {
            this.attributeName = attributeName;
            this.unitID = unitID;
            this.dataTypeID = dataTypeID;
            this.unitName = unitName;
            this.datatypeName = datatypeName;
            this.show = show;
        }
    }
}