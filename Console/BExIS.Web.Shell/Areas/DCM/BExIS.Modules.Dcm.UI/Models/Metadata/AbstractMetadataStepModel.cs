using BExIS.Dlm.Entities.Common;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public class AbstractMetadataStepModel : AbstractStepModel
    {
        public BaseUsage Source { get; set; }
        public int Number { get; set; }
        public long Id { get; set; }
        public long PartyId { get; set; }

        public String DisplayName { get; set; }
        public String Discription { get; set; }
        public int MinCardinality { get; set; }
        public int MaxCardinality { get; set; }

        public List<MetadataAttributeModel> MetadataAttributeModels { get; set; }
        public List<MetadataParameterModel> MetadataParameterModels { get; set; }
        public List<MetadataInstanceModel> Instance { get; set; }

        public AbstractMetadataStepModel()
        {
            Instance = new List<MetadataInstanceModel>();
            MetadataAttributeModels = new List<MetadataAttributeModel>();
            MetadataParameterModels = new List<MetadataParameterModel>();
        }

        public void ConvertInstance(XDocument metadata, string xpath)
        {
            var x = XmlUtility.GetXElementByXPath(xpath, metadata);

            if (x != null)
            {
                IEnumerable<XElement> xelements = x.Elements();

                if (xelements.Count() > 0)
                {
                    int counter = 0;
                    ;
                    foreach (XElement element in xelements)
                    {
                        long id = Convert.ToInt64(element.Attribute("id").Value.ToString());
                        string name = element.Parent.Attribute("name").Value.ToString();
                        long usageId = Convert.ToInt64(element.Attribute("roleId").Value.ToString());
                        int number = Convert.ToInt32(element.Attribute("number").Value.ToString());

                        MetadataInstanceModel temp = new MetadataInstanceModel()
                        {
                            Id = id,
                            UsageId = usageId,
                            Name = name,
                            Number = number
                        };

                        if (!Instance.Where(t => t.Id.Equals(temp.Id) && t.Number.Equals(temp.Number)).Any())
                        {
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
}