using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;




namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class TaskManager
    {
        public List<StepInfo> TaskInfos;
        private StepInfo currentStepInfo;

        public Dictionary<string, object> Bus = new Dictionary<string, object>();
  
        public static TaskManager Bind(XmlDocument xmlDocument)
        {
            
            XmlNodeList xmlStepInfos = xmlDocument.GetElementsByTagName("stepInfo");

            TaskManager tm = new TaskManager();
            tm.TaskInfos = new List<StepInfo>();


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

                tm.TaskInfos.Add(si);
            }

            tm.currentStepInfo = tm.TaskInfos.First();

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
            currentStepInfo = TaskInfos.ElementAt(index);
            currentStepInfo.SetStatus(StepStatus.inProgress);

            for (int i = index+1; i < TaskInfos.Count(); i++)
            {
                TaskInfos.ElementAt(i).SetStatus(StepStatus.none);
            }

        }

        public StepInfo Start()
        {
            return TaskInfos.First();
        }

        public StepInfo Finish()
        {
            return TaskInfos.Last();
        }

        public StepInfo Next()
        {
            int currentIndex = TaskInfos.IndexOf(currentStepInfo);
            if (currentIndex == TaskInfos.Count-1) return new StepInfo("");

            return TaskInfos.ElementAt(currentIndex+1);
        }

        public void GoToNext()
        {
            this.currentStepInfo.SetStatus(StepStatus.success);
            StepInfo temp = this.currentStepInfo;
            this.currentStepInfo = this.Next();
        }

        
        public StepInfo Prev()
        {
            int currentIndex = TaskInfos.IndexOf(currentStepInfo);

            // think about :D
            if (currentIndex == 0) return new StepInfo("");
            return TaskInfos.ElementAt(currentIndex-1);
        }

        public void GoToPrev()
        {
            StepInfo temp = this.currentStepInfo;
            this.currentStepInfo = this.Prev();
        }



        public StepInfo Jump(int index)
        {
            if (index < 0 || index > TaskInfos.Count) return null;

            return TaskInfos.ElementAt(index);
        }


        public List<Tuple<string, StepStatus>> GetStatusOfStepInfos()
        {
            List<Tuple<string, StepStatus>> list = new List<Tuple<string, StepStatus>>();

            foreach (StepInfo si in TaskInfos)
            {
                list.Add(si.GetStatusWithName());
            }

            return list;
        }

        public int GetIndex(StepInfo stepInfo)
        {
            return this.TaskInfos.IndexOf(stepInfo);
        }

        public int GetCurrentStepInfoIndex()
        {
            return this.TaskInfos.IndexOf(Current());
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