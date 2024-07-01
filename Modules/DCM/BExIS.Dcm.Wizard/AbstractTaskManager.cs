using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

/// <summary>
///
/// </summary>
namespace BExIS.Dcm.Wizard
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class AbstractTaskManager
    {
        public List<StepInfo> StepInfos;
        public List<StepInfo> TaskInfos;
        protected StepInfo currentStepInfo;
        protected StepInfo prevStepInfo;

        public Dictionary<string, object> Bus = new Dictionary<string, object>();
        public Dictionary<string, ActionInfo> Actions = new Dictionary<string, ActionInfo>();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static AbstractTaskManager Bind(XmlDocument xmlDocument)
        {
            XmlNodeList xmlStepInfos = xmlDocument.GetElementsByTagName("stepInfo");

            AbstractTaskManager tm = new AbstractTaskManager();
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
        /// <param>NA</param>
        /// <returns></returns>
        public StepInfo Current()
        {
            return currentStepInfo;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="stepInfo"></param>
        public void SetCurrent(StepInfo stepInfo)
        {
            currentStepInfo = stepInfo;
            currentStepInfo.SetStatus(StepStatus.inProgress);
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
                StepInfos.ElementAt(i).SetStatus(StepStatus.none);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public StepInfo Start()
        {
            return StepInfos.First();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public StepInfo Finish()
        {
            return StepInfos.Last();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public StepInfo Next()
        {
            int currentIndex = StepInfos.IndexOf(currentStepInfo);
            if (currentIndex == StepInfos.Count - 1) return new StepInfo("");

            return StepInfos.ElementAt(currentIndex + 1);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public void GoToNext()
        {
            this.currentStepInfo.SetStatus(StepStatus.success);
            StepInfo temp = this.currentStepInfo;
            this.currentStepInfo = this.Next();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public StepInfo Prev()
        {
            int currentIndex = StepInfos.IndexOf(currentStepInfo);
            if (currentIndex == 0) return new StepInfo("");

            //prevStepInfo = TaskInfos.Last();
            //TaskInfos.Remove(TaskInfos.Last());

            return prevStepInfo;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="stepinfo"></param>
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="stepinfo"></param>
        public void RemoveExecutedStep(StepInfo stepinfo)
        {
            if (TaskInfos == null)
                TaskInfos = new List<StepInfo>();
            else
            {
                if (TaskInfos.Contains(stepinfo))
                    TaskInfos.Remove(stepinfo);

                prevStepInfo = TaskInfos.Last();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public void GoToPrev()
        {
            StepInfo temp = this.currentStepInfo;
            this.currentStepInfo = this.Prev();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="index"></param>
        /// <returns></returns>
        public StepInfo Jump(int index)
        {
            if (index < 0 || index > StepInfos.Count) return null;

            return StepInfos.ElementAt(index);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="title"></param>
        /// <returns></returns>
        public StepInfo Jump(string title)
        {
            if (title == "" || StepInfos.Where(s => s.title.Equals(title)).Count() == 0) return null;

            return StepInfos.Where(s => s.title.Equals(title)).FirstOrDefault();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public List<Tuple<string, StepStatus>> GetStatusOfStepInfos()
        {
            List<Tuple<string, StepStatus>> list = new List<Tuple<string, StepStatus>>();

            foreach (StepInfo si in StepInfos)
            {
                list.Add(si.GetStatusWithName());
            }

            return list;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="stepInfo"></param>
        /// <returns></returns>
        public int GetIndex(StepInfo stepInfo)
        {
            return this.StepInfos.IndexOf(stepInfo);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
        public int GetCurrentStepInfoIndex()
        {
            return this.StepInfos.IndexOf(Current());
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="key"></param>
        /// <param name="value"></param>
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void UpdateBus(string key, object value)
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
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