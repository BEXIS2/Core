using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.Common;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.IO.Transform.Validation.Exceptions;

namespace BExIS.Web.Shell.Areas.DCM.Models.Create
{
    public class StepModelHelper
    {
        public int StepId { get; set; }
        public BaseUsage Usage { get; set; }
        public int Number { get; set; }
        public AbstractMetadataStepModel Model { get; set; }
        public string XPath { get; set; }
        public List<StepModelHelper> Childrens { get; set; }
        

        public StepModelHelper()
        {
            StepId = 0;
            Usage = new BaseUsage();
            Number = 0;
            XPath = "";
            Childrens = new List<StepModelHelper>();
        }

        public StepModelHelper(int stepId, int number, BaseUsage usage, string xpath)
        {
            StepId = stepId;
            Usage = usage;
            Number = number;
            XPath = xpath;
            Childrens = new List<StepModelHelper>();
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
    }
}