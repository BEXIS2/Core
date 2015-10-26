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

        //public void UpdatePosition(int position)
        //{
        //    Number = position;
        //    //XPath = parentStepModelHelper.XPath + "//" + label.Replace(" ", string.Empty) + "[" + model.Number + "]"
        //    string[] temp = XPath.Split('/');
        //    XPath="";
        //    for (int i = 0; i < temp.Count()-2; i++)
        //    {
        //        XPath +="/"+temp[i];
        //    }

        //    XPath +="/"+Usage.Label.Replace(" ", string.Empty)+"Type" + "[" + Number + "]";
            
        //}
    }
}