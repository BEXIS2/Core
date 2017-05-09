using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using BExIS.Dcm.Wizard;

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
        //metadata easy upload
        public static string SCHEMA = "Schema";
        public static string DESCRIPTIONTITLE = "DescriptionTitle";
        // sheet structure
        public static string SHEET_FORMAT = "SheetFormat";
        // sheet area selection
        public static string SHEET_HEADER_AREA = "SheetHeaderArea";
        public static string SHEET_DATA_AREA = "SheetDataArea";
        public static string SHEET_JSON_DATA = "SheetJsonData";
        // data validation
        public static string VERIFICATION_AVAILABLEUNITS = "VerificationAvailableUnits";
        public static string VERIFICATION_HEADERFIELDS = "VerificationHeaderFields";
        public static string VERIFICATION_MAPPEDHEADERUNITS = "VerificationMappedHeaderUnits";
        public static string VERIFICATION_ATTRIBUTESUGGESTIONS = "VerificationAttributeSuggestions";

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
        public static EasyUploadTaskManager Bind(XmlDocument xmlDocument)
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