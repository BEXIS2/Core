using BExIS.Dlm.Entities.Common;
using BExIS.Modules.Dcm.UI.Models.Metadata;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace BExIS.Modules.Dcm.UI.Models.CreateDataset
{
    public class StepModelHelper
    {
        public int StepId { get; set; }
        public BaseUsage Usage { get; set; }
        public int Number { get; set; }
        public int Level { get; set; }

        public string XPath { get; set; }
        public List<StepModelHelper> Childrens { get; set; }
        public StepModelHelper Parent { get; set; }

        public bool Activated { get; set; }
        public bool Choice { get; set; }

        private AbstractMetadataStepModel _model;

        public AbstractMetadataStepModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                Activated = SetActiveByPreload();
            }
        }

        public StepModelHelper()
        {
            StepId = 0;
            Usage = new BaseUsage();
            Number = 0;
            XPath = "";
            Childrens = new List<StepModelHelper>();
            Activated = SetActiveByPreload();
            Level = 0;
        }

        public StepModelHelper(int stepId, int number, BaseUsage usage, string xpath, StepModelHelper parent)
        {
            StepId = stepId;
            Usage = usage;
            Number = number;
            XPath = xpath;
            Childrens = new List<StepModelHelper>();
            Parent = parent;
            Choice = IsChoice(usage);

            if (parent != null)
                Level = parent.Level + 1;
        }

        public string GetXPathFromSimpleAttribute(long id)
        {
            if (Model != null && Model.MetadataAttributeModels != null && Model.MetadataAttributeModels.Any())
            {
                MetadataAttributeModel temp = Model.MetadataAttributeModels.Where(a => a.Id.Equals(id) && a.Number.Equals(Number)).First();

                return XPath + "//" + temp.Source.Label + "[1]//" + temp.GetMetadataAttribute().Self.Name;
            }

            return "";
        }

        public string GetXPathFromSimpleAttribute(long id, long number)
        {
            if (Model != null && Model.MetadataAttributeModels != null && Model.MetadataAttributeModels.Any())
            {
                MetadataAttributeModel temp = Model.MetadataAttributeModels.Where(a => a.Id.Equals(id)).First();

                return XPath + "//" + temp.Source.Label + "[1]//" + temp.GetMetadataAttribute().Self.Name + "[" + number + "]";
            }

            return "";
        }

        public bool IsEmpty()
        {
            foreach (MetadataAttributeModel simpleAttr in Model.MetadataAttributeModels)
            {
                if (simpleAttr.IsEmpty != true) return false;
            }

            foreach (StepModelHelper smh in Childrens)
            {
                if (smh.IsEmpty() != true) return false;
            }

            return true;

        }

        public string DisplayName()
        {
            string displayName = "";

            char tmp = ' ';

            foreach (char letter in Usage.Label)
            {
                if (Usage.Label.First() == letter)
                {
                    tmp = letter;
                    displayName += letter;
                }
                else
                {
                    if (Char.IsUpper(letter) && Char.IsLower(tmp))
                    {
                        displayName += " " + letter;
                    }
                    else
                    {
                        displayName += letter;
                    }

                    tmp = letter;
                }
            }

            return displayName;
        }

        /// <summary>
        /// When a Component is required and not in a choice object
        /// then it will actived automaticlly
        /// </summary>
        /// <returns></returns>
        private bool SetActiveByPreload()
        {
            if (Model != null &&
                Model.MinCardinality > 0
                && !IsChoice(Model.Source)) Activated = true;

            return Activated;
        }

        public bool IsParentActive()
        {
            if (Parent == null)
                return Activated;

            if (Parent.Activated)
                return Parent.IsParentActive();

            return false;

        }

        private bool IsChoice(BaseUsage usage)
        {
            if (usage.Extra != null)
            {
                XmlDocument doc = usage.Extra as XmlDocument;
                XElement element = XmlUtility.GetXElementByAttribute("type", "name", "choice", XmlUtility.ToXDocument(doc));
                if (element != null) return true;
            }

            return false;
        }


    }
}