using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.Common;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.IO.Transform.Validation.Exceptions;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace BExIS.Web.Shell.Areas.DCM.Models.CreateDataset
{
    public class StepModelHelper
    {
        public int StepId { get; set; }
        public BaseUsage Usage { get; set; }
        public int Number { get; set; }
        
        public string XPath { get; set; }
        public List<StepModelHelper> Childrens { get; set; }
        public StepModelHelper Parent { get; set; }

        public bool Activated { get; set; }

        private AbstractMetadataStepModel _model;

        public AbstractMetadataStepModel Model {
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
        }

        public StepModelHelper(int stepId, int number, BaseUsage usage, string xpath, StepModelHelper parent)
        {
            StepId = stepId;
            Usage = usage;
            Number = number;
            XPath = xpath;
            Childrens = new List<StepModelHelper>();
            Parent = parent;
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
            if(Model != null && Model.MetadataAttributeModels != null && Model.MetadataAttributeModels.Any())
            {
                MetadataAttributeModel temp =  Model.MetadataAttributeModels.Where(a => a.Id.Equals(id)).First();

                return XPath + "//" + temp.Source.Label + "[1]//" + temp.GetMetadataAttribute().Self.Name+"["+number+"]";
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

        private bool SetActiveByPreload()
        {
            if (Model != null && Model.MinCardinality > 0) Activated = true;

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
    }
}