using BExIS.Dcm.Wizard;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace BExIS.Dcm.ImportMetadataStructureWizard
{
    public class ImportMetadataStructureTaskManager : AbstractTaskManager
    {
        public static string FILENAME = "filename";
        public static string FILEPATH = "filepath";
        public static string EXTENTION = "Extention";

        public static string SOURCE_STRUCTURE = "source_structure";
        public static string SCHEMA_NAME = "schema_name";
        public static string ROOT_NODE = "ROOT_NODE";
        public static string TITLE_NODE = "TITLE_NODE";
        public static string DESCRIPTION_NODE = "DESCRIPTION_NODE";
        public static string ENTITY_TYPE_NODE = "ENTITY_TYPE_NODE";
        public static string SYSTEM_NODES = "SYSTEM_NODES";
        public static string IS_GENERATE = "IS_GENERATE";

        public static string METADATASTRUCTURE_ID = "METADATASTRUCTURE_ID";

        public static string XML_SCHEMA_MANAGER = "XML_SCHEMA_MANAGER";
        public static string XML_ELEMENT_LIST = "XML_ELEMENT_LIST";

        public static string MAPPING_FILE_NAME_IMPORT = "MAPPING_FILE_NAME_IMPORT";
        public static string MAPPING_FILE_NAME_EXPORT = "MAPPING_FILE_NAME_EXPORT";

        public static string ALL_METADATA_NODES = "ALL_METADATA_NODES";

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static ImportMetadataStructureTaskManager Bind(XmlDocument xmlDocument)
        {
            XmlNodeList xmlStepInfos = xmlDocument.GetElementsByTagName("stepInfo");

            ImportMetadataStructureTaskManager tm = new ImportMetadataStructureTaskManager();
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="index"></param>
        public void SetCurrent(int index)
        {
            currentStepInfo = StepInfos.ElementAt(index);
            currentStepInfo.SetStatus(StepStatus.inProgress);

            for (int i = index + 1; i < StepInfos.Count(); i++)
            {
                if (!StepInfos.ElementAt(i).stepStatus.Equals(StepStatus.success)) StepInfos.ElementAt(i).SetStatus(StepStatus.none);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <returns></returns>
        public StepInfo Prev()
        {
            int currentIndex = StepInfos.IndexOf(currentStepInfo);
            if (currentIndex == 0) return new StepInfo("");

            return StepInfos.ElementAt(currentIndex - 1);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public void GoToNext()
        {
            //this.currentStepInfo.SetStatus(StepStatus.success);
            StepInfo temp = this.currentStepInfo;
            this.currentStepInfo = this.Next();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="newIndex"></param>
        public void UpdateStepStatus(int newIndex)
        {
            // new index higher than current Index and next Index
            if (newIndex > GetCurrentStepInfoIndex() && newIndex >= GetIndex(Next()))
            {
                for (int i = GetCurrentStepInfoIndex(); i < newIndex; i++)
                {
                    if (StepInfos[i].stepStatus != StepStatus.success) StepInfos[i].SetStatus(StepStatus.error);
                }
            }

            // new index lower than currentindex && lower than prev index
            if (newIndex < GetCurrentStepInfoIndex() && newIndex <= GetIndex(Next()))
            {
                for (int i = GetCurrentStepInfoIndex(); i > newIndex; i--)
                {
                    if (StepInfos[i].stepStatus != StepStatus.success) StepInfos[i].SetStatus(StepStatus.error);
                }
            }
        }
    }
}