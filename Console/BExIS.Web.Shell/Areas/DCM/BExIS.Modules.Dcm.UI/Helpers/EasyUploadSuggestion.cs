using System;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class EasyUploadSuggestion
    {
        public long Id;
        public string attributeName;
        public long unitID;
        public long dataTypeID;
        public string unitName;
        public string datatypeName;
        public Boolean show;

        public EasyUploadSuggestion(long id, string attributeName, long unitID, long dataTypeID, Boolean show)
        {
            this.Id = id;
            this.attributeName = attributeName;
            this.unitID = unitID;
            this.dataTypeID = dataTypeID;
            this.show = show;
        }
    }
}