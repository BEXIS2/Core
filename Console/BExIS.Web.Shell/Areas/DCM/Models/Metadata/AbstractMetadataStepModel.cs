using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Common;
using BExIS.Xml.Helpers;

namespace BExIS.Web.Shell.Areas.DCM.Models.Metadata
{
    public class AbstractMetadataStepModel:AbstractStepModel
    {
        public BaseUsage Source { get; set; }
        public int Number { get; set; }
        public long Id { get; set; }

        public String DisplayName { get; set; }
        public String Discription { get; set; }
        public int MinCardinality { get; set; }
        public int MaxCardinality { get; set; }

        public List<MetadataAttributeModel> MetadataAttributeModels { get; set; }
        public List<MetadataInstanceModel> Instance { get; set; }

        public AbstractMetadataStepModel()
        {
            Instance = new List<MetadataInstanceModel>();
            MetadataAttributeModels = new List<MetadataAttributeModel>();
        }

        public void ConvertInstance(XDocument metadata)
        {
            var x = XmlUtility.GetXElementByAttribute(Source.Label, "type", BExIS.Xml.Helpers.XmlNodeType.MetadataAttributeUsage.ToString(), metadata);

            if (x == null)
                x = XmlUtility.GetXElementByAttribute(Source.Label, "type", BExIS.Xml.Helpers.XmlNodeType.MetadataPackageUsage.ToString(), metadata);

            if (x != null)
            {


                IEnumerable<XElement> xelements = x.Elements();

                if (xelements.Count() > 0)
                {
                    int counter = 0;

                    foreach (XElement element in xelements)
                    {
                        long id = Convert.ToInt64(element.Attribute("id").Value.ToString());
                        string name = element.Attribute("name").Value.ToString();
                        long usageId = Convert.ToInt64(element.Attribute("roleId").Value.ToString());
                        int number = Convert.ToInt32(element.Attribute("number").Value.ToString());

                        Instance.Add(

                            new MetadataInstanceModel()
                            {
                                Id = id,
                                UsageId = usageId,
                                Name = name,
                                Number = number
                            }

                            );

                    }
                }
            }

        }
    }
}