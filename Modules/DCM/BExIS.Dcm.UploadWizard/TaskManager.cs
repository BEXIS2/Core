using BExIS.Dcm.Wizard;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

/// <summary>
///
/// </summary>
namespace BExIS.Dcm.UploadWizard
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class TaskManager : AbstractTaskManager
    {
        public static string FILENAME = "FileName";
        public static string FILEPATH = "FilePath";
        public static string EXTENTION = "Extention";

        public static string FILE_READER_INFO = "FileReaderInfo";
        public static string IS_TEMPLATE = "IsTemplate";

        //Dataset
        public static string DATASET_ID = "DatasetId";

        public static string DATASET_TITLE = "DatasetTitle";

        // value = "new"
        public static string DATASET_STATUS = "DatasetStatus";

        // Datastructure
        public static string DATASTRUCTURE_ID = "DataStructureId";

        public static string DATASTRUCTURE_TITLE = "DataStructureTitle";
        public static string DATASTRUCTURE_TYPE = "DataStructureType";

        //ResearchPlan
        public static string RESEARCHPLAN_ID = "ResearchPlanId";

        public static string RESEARCHPLAN_TITLE = "ResearchPlanTitle";

        //Data
        public static string PRIMARY_KEYS = "PrimaryKeys";

        public static string PRIMARY_KEYS_UNIQUE = "PrimaryKeysUnique";
        public static string VALID = "Valid";

        //metadata
        public static string TITLE = "Title";

        public static string AUTHOR = "Author";
        public static string OWNER = "Owner";
        public static string PROJECTNAME = "ProjectName";
        public static string INSTITUTE = "Institute";

        //Easy Upload Sheet Structure
        public static string SHEET_FORMAT = "SheetFormat";

        //Easy Upload Sheet Area Selection
        public static string SHEET_HEADER_AREA = "SheetHeaderArea";

        public static string SHEET_DATA_AREA = "SheetDataArea";
        public static string SHEET_JSON_DATA = "SheetJsonData";
        public static string ACTIVE_WOKSHEET_URI = "ActiveWorksheetUri";

        public static string UPLOAD_METHOD = "UPLOAD_METHOD";
        public static string NUMBERSOFROWS = "NUMBERSOFROWS";
        public static string NUMBERSOFVARIABLES = "NUMBERSOFVARIABLES";
        public static string CURRENTPACKAGE = "CURRENTPACKAGE";
        public static string CURRENTPACKAGESIZE = "CURRENTPACKAGE";

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static TaskManager Bind(XmlDocument xmlDocument)
        {
            XmlNodeList xmlStepInfos = xmlDocument.GetElementsByTagName("stepInfo");

            TaskManager tm = new TaskManager();
            tm.StepInfos = new List<StepInfo>();

            foreach (XmlNode xmlStepInfo in xmlStepInfos)
            {
                StepInfo si = new StepInfo(xmlStepInfo.Attributes.GetNamedItem("title").Value)
                {
                    GetActionInfo = new ActionInfo
                    {
                        ActionName = xmlStepInfo.Attributes.GetNamedItem("action").Value,
                        ControllerName = xmlStepInfo.Attributes.GetNamedItem("controller").Value,
                        AreaName = xmlStepInfo.Attributes.GetNamedItem("area").Value
                    },

                    PostActionInfo = new ActionInfo
                    {
                        ActionName = xmlStepInfo.Attributes.GetNamedItem("action").Value,
                        ControllerName = xmlStepInfo.Attributes.GetNamedItem("controller").Value,
                        AreaName = xmlStepInfo.Attributes.GetNamedItem("area").Value
                    }
                };

                tm.StepInfos.Add(si);
            }

            tm.currentStepInfo = tm.StepInfos.First();

            return tm;
        }
    }

    ///// <summary>
    /////
    ///// </summary>
    //public enum DataStructureType
    //{
    //    Structured,
    //    Unstructured
    //}

    //public enum UploadMethod
    //{
    //    Append,
    //    Update
    //}
}