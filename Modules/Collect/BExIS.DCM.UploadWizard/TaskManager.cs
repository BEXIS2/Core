using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;




namespace BExIS.DCM.UploadWizard
{
    public class TaskManager
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
        //ResearchPlan
        public static string RESEARCHPLAN_ID = "ResearchPlanId";
        public static string RESEARCHPLAN_TITLE = "ResearchPlanTitle";
        //Data
        public static string PRIMARY_KEYS = "PrimaryKeys";
        public static string VALID = "Valid";
        //metadata
        public static string TITLE = "Title";
        public static string AUTHOR = "Author";
        public static string OWNER = "Owner";
        public static string PROJECTNAME = "ProjectName";
        public static string INSTITUTE = "Institute";







        public List<StepInfo> StepInfos;
        public List<StepInfo> TaskInfos;
        private StepInfo currentStepInfo;
        private StepInfo prevStepInfo;

        public Dictionary<string, object> Bus = new Dictionary<string, object>();
  
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


        public StepInfo Current()
        {
            return currentStepInfo;
        }

        public void SetCurrent(StepInfo stepInfo)
        {
            currentStepInfo = stepInfo;
            currentStepInfo.SetStatus(StepStatus.inProgress);
        }

        public void SetCurrent(int index)
        {
            currentStepInfo = StepInfos.ElementAt(index);
            currentStepInfo.SetStatus(StepStatus.inProgress);

            for (int i = index+1; i < StepInfos.Count(); i++)
            {
                StepInfos.ElementAt(i).SetStatus(StepStatus.none);
            }

        }

        public StepInfo Start()
        {
            return StepInfos.First();
        }

        public StepInfo Finish()
        {
            return StepInfos.Last();
        }

        public StepInfo Next()
        {
            int currentIndex = StepInfos.IndexOf(currentStepInfo);
            if (currentIndex == StepInfos.Count-1) return new StepInfo("");

            return StepInfos.ElementAt(currentIndex+1);
        }

        public void GoToNext()
        {
            this.currentStepInfo.SetStatus(StepStatus.success);
            StepInfo temp = this.currentStepInfo;
            this.currentStepInfo = this.Next();
        }

        
        public StepInfo Prev()
        {
            int currentIndex = StepInfos.IndexOf(currentStepInfo);
            if (currentIndex == 0) return new StepInfo("");

            //prevStepInfo = TaskInfos.Last();
            //TaskInfos.Remove(TaskInfos.Last());

            return prevStepInfo;
        }

        public void AddExecutedStep(StepInfo stepinfo)
        {
            prevStepInfo = stepinfo;
            if (TaskInfos == null)
                TaskInfos = new List<StepInfo>();

            if (TaskInfos.Count > 0)
            {
                if (TaskInfos.Select(p => p.title.Equals(stepinfo.title)).First())
                {
                    StepInfo tempStep = TaskInfos.Where(p => p.title.Equals(stepinfo.title)).First();
                    TaskInfos.Remove(tempStep);
                }
            }   

            TaskInfos.Add(stepinfo);
        }

        public void RemoveExecutedStep(StepInfo stepinfo)
        {

            if (TaskInfos == null)
                TaskInfos = new List<StepInfo>();
            else
            {
                if(TaskInfos.Contains(stepinfo))
                    TaskInfos.Remove(stepinfo);

                prevStepInfo = TaskInfos.Last();
            }
        }

        public void GoToPrev()
        {
            StepInfo temp = this.currentStepInfo;
            this.currentStepInfo = this.Prev();
        }

        public StepInfo Jump(int index)
        {
            if (index < 0 || index > StepInfos.Count) return null;

            return StepInfos.ElementAt(index);
        }

        public List<Tuple<string, StepStatus>> GetStatusOfStepInfos()
        {
            List<Tuple<string, StepStatus>> list = new List<Tuple<string, StepStatus>>();

            foreach (StepInfo si in StepInfos)
            {
                list.Add(si.GetStatusWithName());
            }

            return list;
        }

        public int GetIndex(StepInfo stepInfo)
        {
            return this.StepInfos.IndexOf(stepInfo);
        }

        public int GetCurrentStepInfoIndex()
        {
            return this.StepInfos.IndexOf(Current());
        }

        public void AddToBus(string key, object value)
        {
            if (!this.Bus.ContainsKey(key))
            {
                this.Bus.Add(key, value);
            }
            else
            {
                this.Bus[key] = value;
            }
        }

        public void AddToBus(object[] data)
        {
            foreach (object o in data)
            {
                string[] temp = o.ToString().Split(':');
                AddToBus(temp.First(), temp.Last());
            }
        }
    }
}