using BExIS.Modules.Dcm.UI.Models.Metadata;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace BExIS.Modules.Dcm.UI.Models.CreateDataset
{
    public class StepModelHelper
    {
        public int StepId { get; set; }

        //public BaseUsage Usage { get; set; }
        public long UsageId { get; set; }

        public Type UsageType { get; set; }
        public string UsageName { get; set; }
        public string UsageAttrName { get; set; }
        public int Number { get; set; }
        public int Level { get; set; }

        public string XPath { get; set; }
        public List<StepModelHelper> Childrens { get; set; }
        public StepModelHelper Parent { get; set; }

        public bool Activated { get; set; }
        public bool Choice { get; set; }
        public long ChoiceMax { get; set; }

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
            UsageId = 0;
            Number = 0;
            XPath = "";
            Childrens = new List<StepModelHelper>();
            Activated = SetActiveByPreload();
            Level = 0;
        }

        public StepModelHelper(int stepId, int number, long usageId, string usageName, string usageAttrName, Type usageType, string xpath, StepModelHelper parent, XmlNode extra)
        {
            StepId = stepId;
            UsageId = usageId;
            UsageType = usageType;
            UsageName = usageName;
            UsageAttrName = usageAttrName;
            Number = number;
            XPath = xpath;
            Childrens = new List<StepModelHelper>();
            Parent = parent;
            Choice = IsChoice(extra);

            if (Choice) ChoiceMax = GetChoiceMax(extra);

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

            foreach (char letter in UsageName)
            {
                if (UsageName.First() == letter)
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
                Model.MinCardinality > 0) Activated = true;

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

        private bool IsChoice(XmlNode xmlNode)
        {
            if (xmlNode != null)
            {
                XmlNode element = XmlUtility.GetXmlNodeByAttribute(xmlNode, "type", "name", "choice");
                if (element != null) return true;
            }
            return false;
        }

        private long GetChoiceMax(XmlNode xmlNode)
        {
            if (xmlNode != null)
            {
                XmlNode element = XmlUtility.GetXmlNodeByAttribute(xmlNode, "type", "name", "choice");
                if (element == null) return 0;
                else
                {
                    if (element.Attributes.Count > 0)
                    {
                        foreach (XmlAttribute attr in element.Attributes)
                        {
                            if (attr.Name.ToLower().Equals("max")) return Convert.ToInt64(attr.Value);
                        }
                    }
                }
            }
            return 0;
        }
    }
}