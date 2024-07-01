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
    public class EasyUploadTaskManager : AbstractTaskManager
    {
        public static string FILENAME = "FileName";
        public static string FILEPATH = "FilePath";
        public static string EXTENTION = "Extention";

        public static string IS_TEMPLATE = "IsTemplate";

        //Dataset
        public static string DATASET_ID = "DatasetId";

        public static string DATASET_TITLE = "DatasetTitle";

        // Datastructure
        public static string DATASTRUCTURE_ID = "DataStructureId";

        public static string DATASTRUCTURE_TITLE = "DataStructureTitle";

        //ResearchPlan
        public static string RESEARCHPLAN_ID = "ResearchPlanId";

        public static string RESEARCHPLAN_TITLE = "ResearchPlanTitle";

        //Easy Upload Metadata
        public static string TITLE = "Title";

        public static string SCHEMA = "Schema";
        public static string DESCRIPTIONTITLE = "DescriptionTitle";

        //Easy Upload Sheet Structure
        public static string SHEET_FORMAT = "SheetFormat";

        //Easy Upload Sheet Area Selection
        public static string SHEET_HEADER_AREA = "SheetHeaderArea";

        public static string SHEET_DATA_AREA = "SheetDataArea";
        public static string SHEET_JSON_DATA = "SheetJsonData";
        public static string ACTIVE_WOKSHEET_URI = "ActiveWorksheetUri";

        //Easy Upload Data Validation
        public static string VERIFICATION_AVAILABLEUNITS = "VerificationAvailableUnits";

        public static string VERIFICATION_HEADERFIELDS = "VerificationHeaderFields";
        public static string VERIFICATION_MAPPEDHEADERUNITS = "VerificationMappedHeaderUnits";
        public static string VERIFICATION_ATTRIBUTESUGGESTIONS = "VerificationAttributeSuggestions";

        public static string ROWS = "ROWS";
        public static string ALL_DATATYPES = "ALL_DATATYPES";

        public static string NUMBERSOFROWS = "NUMBERSOFROWS";
        public static string CURRENTPACKAGE = "CURRENTPACKAGE";
        public static string CURRENTPACKAGESIZE = "CURRENTPACKAGE";

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public new static EasyUploadTaskManager Bind(XmlDocument xmlDocument)
        {
            XmlNodeList xmlStepInfos = xmlDocument.GetElementsByTagName("stepInfo");

            EasyUploadTaskManager tm = new EasyUploadTaskManager();
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
}