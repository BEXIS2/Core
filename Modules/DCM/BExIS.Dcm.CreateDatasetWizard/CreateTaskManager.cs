using BExIS.Dcm.Wizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

/// <summary>
///
/// </summary>
namespace BExIS.Dcm.CreateDatasetWizard
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class CreateTaskmanager : AbstractTaskManager
    {
        public static string ENTITY_ID = "ENTITY_ID";
        public static string ENTITY_TITLE = "ENTITY_TITLE";
        public static string ENTITY_CLASS_PATH = "ENTITY_CLASS_PATH";
        public static string COPY_OF_ENTITY_ID = "COPY_OF_ENTITY_ID";

        public static string METADATA_ATTRIBUTE_USAGE_VALUE_LIST = "METADATA_ATTRIBUTE_VALUE_LIST";

        public static string METADATA_PACKAGE_MODEL_LIST = "METADATA_PACKAGE_MODEL_LIST";
        public static string METADATA_STEP_MODEL_HELPER = "METADATA_STEP_MODEL_HELPER";
        public static string METADATA_XML = "METADATA_XML";
        public static string METADATA_IMPORT_XML_FILEPATH = "METADATA_IMPORT_XML_FILEPATH";

        // Datastructure
        public static string DATASTRUCTURE_ID = "DataStructureId";

        public static string DATASTRUCTURE_TITLE = "DataStructureTitle";
        public static string DATASTRUCTURE_TYPE = "DataStructureType";

        //ResearchPlan
        public static string RESEARCHPLAN_ID = "ResearchPlanId";

        public static string RESEARCHPLAN_TITLE = "ResearchPlanTitle";

        public static string METADATASTRUCTURE_ID = "MetadataStructureId";
        public static string METADATAPACKAGE_IDS = "MetadataPackageIds";

        // technical parameters
        public static string SETUP_LOADED = "SETUP_LOADED";

        public static string EDIT_MODE = "EDIT_MODE";
        public static string FORM_STEPS_LOADED = "FORM_STEPS_LOADED";

        // addtionally actionKeys
        public static string CANCEL_ACTION = "CANCEL_ACTION";

        public static string RESET_ACTION = "RESET_ACTION";
        public static string COPY_ACTION = "COPY_ACTION";
        public static string SUBMIT_ACTION = "SUBMIT_ACTION";

        public static string ERROR_DIC = "Error_Dic";
        public static string SAVE_WITH_ERRORS = "SAVE_WITH_ERRORS";

        //Action in Form
        public static string NO_IMPORT_ACTION = "NO_IMPORT_ACTION";

        public static string LOCKED = "LOCKED";

        //Alternativ info text and title on top
        public static string INFO_ON_TOP_TITLE = "INFO_ON_TOP_TITLE";

        public static string INFO_ON_TOP_DESCRIPTION = "INFO_ON_TOP_DESCRIPTION";

        private int MaxStepId;

        public StepInfo Root { get; set; }

        public CreateTaskmanager()
        {
            MaxStepId = 0;

            Root = new StepInfo("ROOT")
            {
                Id = GenerateStepId()
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static CreateTaskmanager Bind(XmlDocument xmlDocument)
        {
            XmlNodeList xmlStepInfos = xmlDocument.GetElementsByTagName("stepInfo");
            CreateTaskmanager tm = new CreateTaskmanager();
            tm.StepInfos = new List<StepInfo>();

            foreach (XmlNode xmlStepInfo in xmlStepInfos)
            {
                StepInfo si = new StepInfo(xmlStepInfo.Attributes.GetNamedItem("title").Value)
                {
                    Id = tm.GenerateStepId(),
                    Parent = tm.Root,
                    IsInstanze = true,
                    HasContent = true,
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
                tm.Root.Children.Add(si);
            }

            tm.currentStepInfo = tm.Root.Children.First();

            return tm;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetCurrentStepInfoIndex()
        {
            return Current().Id;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="index"></param>
        public void SetCurrent(int id)
        {
            currentStepInfo = Get(id);
            currentStepInfo.Expand = true;
        }

        public bool IsCurrent(StepInfo stepInfo)
        {
            if (this.currentStepInfo.Id.Equals(stepInfo.Id))
                return true;
            else
                return false;
        }

        public bool IsChildCurrent(StepInfo stepInfo)
        {
            if (IsCurrent(stepInfo))
            {
                return true;
            }

            if (stepInfo.Children.Count > 0)
            {
                foreach (StepInfo child in stepInfo.Children)
                {
                    if (IsChildCurrent(child) == true) return true;
                }
            }

            return false;
        }

        public bool IsChildExpand(StepInfo stepInfo)
        {
            if (stepInfo.Children.Count > 0)
            {
                foreach (StepInfo child in stepInfo.Children)
                {
                    if (child.Expand == true) return true;
                }
            }

            return false;
        }

        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <returns></returns>
        public StepInfo Prev()
        {
            StepInfo newStep = currentStepInfo;

            do
            {
                newStep = findPrev(newStep);
            }
            while (newStep.HasContent == false || newStep.IsInstanze == false);

            this.prevStepInfo = newStep;

            return this.prevStepInfo;
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
            StepInfo newStep = currentStepInfo;

            do
            {
                if (newStep.Children.Count > 0)
                {
                    newStep = newStep.Children.First();
                }
                else
                {
                    newStep = findNext(newStep);
                }
            }
            while (newStep.HasContent == false || newStep.IsInstanze == false);

            return newStep;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <returns></returns>
        //public StepInfo Prev()
        //{
        //    this.prevStepInfo = findPrev(this.currentStepInfo);
        //    return this.prevStepInfo;
        //}

        //public StepInfo Next()
        //{
        //    if (this.currentStepInfo.Children.Count > 0)
        //    {
        //        return this.currentStepInfo.Children.First();
        //    }
        //    else
        //    {
        //        return findNext(this.currentStepInfo);
        //    }

        //}

        private StepInfo findPrev(StepInfo child)
        {
            StepInfo parent = child.Parent;
            int index = parent.Children.IndexOf(child);

            if (IsRoot(parent))
            {
                if (index > 0)
                    return Last(parent.Children.ElementAt(index - 1));
                else
                    return new StepInfo("");
            }
            else
            {
                if (index > 0)
                {
                    StepInfo pStep = parent.Children.ElementAt(index - 1);
                    if (pStep.Children.Count == 0)
                        return pStep;
                    else
                        return findLast(pStep);
                }
                else
                    return parent;
            }
        }

        private StepInfo findPrevInstanze(StepInfo child)
        {
            StepInfo parent = child.Parent;
            int index = parent.Children.IndexOf(child);

            if (IsRoot(parent))
            {
                if (index > 0)
                    return Last(parent.Children.ElementAt(index - 1));
                else
                    return new StepInfo("");
            }
            else
            {
                if (!parent.IsInstanze)
                    return findPrevInstanze(parent);

                if (index > 0)
                    return parent.Children.ElementAt(index - 1);
                else
                    return parent;
            }
        }

        private StepInfo findNext(StepInfo child)
        {
            StepInfo parent = child.Parent;

            int index = parent.Children.IndexOf(child);

            if (index + 1 < parent.Children.Count)
            {
                // return next child
                return parent.Children.ElementAt(index + 1);
            }
            else
            {
                if (IsRoot(parent)) return new StepInfo("");
                else
                {
                    return findNext(parent);
                }
            }
        }

        private StepInfo findNextInstanze(StepInfo child)
        {
            //if (child.IsInstanze)
            //{
            StepInfo parent = child.Parent;

            int index = parent.Children.IndexOf(child);

            if (index + 1 < parent.Children.Count)
            {
                // return next child
                return parent.Children.ElementAt(index + 1);
            }
            else
            {
                if (IsRoot(parent)) return new StepInfo("");
                else
                {
                    return findNextInstanze(parent);
                }
            }
            //}
            //else
            //{
            //    return findNextInstanze(child.Parent);
            //}
        }

        /// <summary>
        /// find last step from root
        /// </summary>
        /// <returns></returns>
        public StepInfo Last()
        {
            return findLast(this.Root);
        }

        /// <summary>
        ///  find last step from step
        /// </summary>
        /// <param name="stepInfo"></param>
        /// <returns></returns>
        public StepInfo Last(StepInfo stepInfo)
        {
            return findLast(stepInfo);
        }

        private StepInfo findLast(StepInfo stepInfo)
        {
            if (stepInfo.Children.Count == 0)
                return stepInfo;
            else
                return findLast(stepInfo.Children.Last());
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
            //if (newIndex > GetCurrentStepInfoIndex() && newIndex>=GetIndex(Next()))
            //{
            //    for (int i = GetCurrentStepInfoIndex(); i < newIndex;i++)
            //    {
            //        if (StepInfos[i].stepStatus!=StepStatus.success) StepInfos[i].SetStatus(StepStatus.error);
            //    }
            //}

            //// new index lower than currentindex && lower than prev index
            //if (newIndex < GetCurrentStepInfoIndex() && newIndex <= GetIndex(Next()))
            //{
            //    for (int i = GetCurrentStepInfoIndex(); i > newIndex; i--)
            //    {
            //        if (StepInfos[i].stepStatus != StepStatus.success) StepInfos[i].SetStatus(StepStatus.error);
            //    }
            //}
        }

        public int GenerateStepId()
        {
            return MaxStepId++;
        }

        public bool IsRoot(StepInfo stepInfo)
        {
            if (stepInfo.Id.Equals(this.Root.Id)) return true;
            else return false;
        }

        public StepInfo Get(int id)
        {
            StepInfo stepInfo;

            foreach (StepInfo si in this.StepInfos)
            {
                if (si.Id.Equals(id)) return si;

                if (si.Children.Count > 0)
                {
                    stepInfo = get(si, id);

                    if (stepInfo != null) return stepInfo;
                }
            }

            return null;
        }

        private StepInfo get(StepInfo parent, int id)
        {
            foreach (StepInfo child in parent.Children)
            {
                if (child.Id.Equals(id)) return child;

                if (child.Children.Count > 0)
                {
                    StepInfo s = get(child, id);
                    if (s != null) return s;
                }
            }

            return null;
        }

        public int CountSteps()
        {
            return countSteps(this.Root);
        }

        private int countSteps(StepInfo step)
        {
            int i = 0;

            if (step.Children.Count > 0)
            {
                i = step.Children.Count;

                foreach (StepInfo s in step.Children)
                {
                    i += countSteps(s);
                }
            }

            return i;
        }

        public int GetPosition()
        {
            return GetX(Current(), 1);
        }

        public int GetX(StepInfo step, int position)
        {
            StepInfo parent = step.Parent;

            if (IsRoot(parent))
            {
                int stepIndex = parent.Children.IndexOf(step);

                for (int i = 0; i < stepIndex; i++)
                {
                    position += countSteps(parent.Children.ElementAt(i)) + 1;
                }
            }
            else
            {
                position += parent.Children.IndexOf(step) + 1;
                position = GetX(parent, position);
            }

            return position;
        }

        /// <summary>
        /// Remove stepInfo from parent stepInfo
        ///
        /// </summary>
        /// <param name="parent">parent step</param>
        /// <param name="index">position of the stepinfo in children</param>
        /// <returns></returns>
        public StepInfo Remove(StepInfo parent, int index)
        {
            try
            {
                parent.Children.RemoveAt(index);

                for (int i = index; i < parent.Children.Count; i++)
                {
                    int position = i + 1;
                    parent.Children.ElementAt(i).title = position.ToString();
                }
            }
            catch (Exception ex)
            {
            }

            return parent;
        }

        /// <summary>
        /// XXX
        /// this funktion needs to be replaced
        /// its only to know if a usage schould bee a metadataAttributeUsage or
        /// MetadataNestedAttributeUsage
        /// </summary>
        /// <returns></returns>
        public bool IsParentChildfromRoot()
        {
            if (IsRoot(Current().Parent) ||
                IsRoot(Current().Parent.Parent) ||
                IsRoot(Current().Parent.Parent.Parent) ||
               IsRoot(Current().Parent.Parent.Parent.Parent))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}