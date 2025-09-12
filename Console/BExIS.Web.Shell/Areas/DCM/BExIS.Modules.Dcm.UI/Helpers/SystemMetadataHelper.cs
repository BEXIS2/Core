using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using System;
using System.Collections.Generic;
using System.Xml;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class SystemMetadataHelper
    {
        public XmlDocument SetSystemValuesToMetadata(long datasetid, long version, long metadataStructureId, XmlDocument metadata, params Key[] systemKeyList)
        {
            foreach (var t in systemKeyList)
            {
                //get all mappings to automatic system from the metadata structure
                var mappings = MappingUtils.GetMappingsWhereSource((int)t, LinkElementType.Key, 2);

                if (mappings != null)
                {
                    if (mappings is List<Mapping>)
                    {
                        foreach (var mapping in mappings)
                        {
                            switch (t)
                            {
                                case Key.Id:
                                    {
                                        metadata = setValue(mapping.Target.XPath, datasetid.ToString(), metadata); break;
                                    }
                                case Key.Version:
                                    {
                                        metadata = setValue(mapping.Target.XPath, version.ToString(), metadata); break;
                                    }
                                case Key.DateOfVersion:
                                    {
                                        metadata = setValue(mapping.Target.XPath, DateTime.Now.ToString(), metadata); break;
                                    }
                                case Key.MetadataCreationDate:
                                    {
                                        metadata = setValue(mapping.Target.XPath, DateTime.Now.ToString(), metadata); break;
                                    }
                                case Key.MetadataLastModfied:
                                    {
                                        metadata = setValue(mapping.Target.XPath, DateTime.Now.ToString(), metadata); break;
                                    }
                                case Key.DataCreationDate:
                                    {
                                        metadata = setValue(mapping.Target.XPath, DateTime.Now.ToString(), metadata); break;
                                    }
                                case Key.DataLastModified:
                                    {
                                        metadata = setValue(mapping.Target.XPath, DateTime.Now.ToString(), metadata); break;
                                    }
                            }
                        }
                    }
                }
            }

            //switch(key)
            //....

            // set values

            return metadata;
        }

        private XmlDocument setValue(string xpath, string value, XmlDocument metadata)
        {
            try
            {
                var xmlobj = metadata.SelectSingleNode(xpath);
                if (xmlobj != null && xmlobj is XmlElement)
                {
                    ((XmlElement)xmlobj).InnerText = value;
                }

                if (xmlobj != null && xmlobj is XmlAttribute)
                {
                    ((XmlAttribute)xmlobj).InnerText = value;
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return metadata;
        }
    }
}