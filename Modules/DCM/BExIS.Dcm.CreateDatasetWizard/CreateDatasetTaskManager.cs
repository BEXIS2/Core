using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using BExIS.Dcm.Wizard;



namespace BExIS.Dcm.CreateDatasetWizard
{
    public class CreateDatasetTaskmanager : AbstractTaskManager
    {
        public static string DATASET_ID = "DataSetId";
        public static string DATASET_TITLE = "DataSetTitle";

        public static string METADATA_ATTRIBUTE_USAGE_VALUE_LIST = "METADATA_ATTRIBUTE_VALUE_LIST";
        public static string METADATA_PACKAGE_MODEL_LIST = "METADATA_PACKAGE_MODEL_LIST";
        public static string METADATA_XML = "METADATA_XML";
        // Datastructure
        public static string DATASTRUCTURE_ID = "DataStructureId";
        public static string DATASTRUCTURE_TITLE = "DataStructureTitle";
        //ResearchPlan
        public static string RESEARCHPLAN_ID = "ResearchPlanId";
        public static string RESEARCHPLAN_TITLE = "ResearchPlanTitle";

        public static string METADATASTRUCTURE_ID = "MetadataStructureId";
        public static string METADATAPACKAGE_IDS = "MetadataPackageIds";

        public static string ERROR_DIC = "Error_Dic";

        public static CreateDatasetTaskmanager Bind(XmlDocument xmlDocument)
        {
            XmlNodeList xmlStepInfos = xmlDocument.GetElementsByTagName("stepInfo");

            CreateDatasetTaskmanager tm = new CreateDatasetTaskmanager(); 
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

        public void SetCurrent(int index)
        {
            currentStepInfo = StepInfos.ElementAt(index);
            currentStepInfo.SetStatus(StepStatus.inProgress);

            for (int i = index + 1; i < StepInfos.Count(); i++)
            {
                if(!StepInfos.ElementAt(i).stepStatus.Equals(StepStatus.success))StepInfos.ElementAt(i).SetStatus(StepStatus.none);
            }

        }

        public StepInfo Prev()
        {
            int currentIndex = StepInfos.IndexOf(currentStepInfo);
            if (currentIndex == 0) return new StepInfo("");

            //prevStepInfo = TaskInfos.Last();
            //TaskInfos.Remove(TaskInfos.Last());

            return StepInfos.ElementAt(currentIndex - 1);
        }

        public void GoToNext()
        {
            //this.currentStepInfo.SetStatus(StepStatus.success);
            StepInfo temp = this.currentStepInfo;
            this.currentStepInfo = this.Next();
        }

        public void UpdateStepStatus(int newIndex)
        {
            // new index higher than current Index and next Index
            if (newIndex > GetCurrentStepInfoIndex() && newIndex>=GetIndex(Next()))
            {
                for (int i = GetCurrentStepInfoIndex(); i < newIndex;i++)
                {
                    if (StepInfos[i].stepStatus!=StepStatus.success) StepInfos[i].SetStatus(StepStatus.error);
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