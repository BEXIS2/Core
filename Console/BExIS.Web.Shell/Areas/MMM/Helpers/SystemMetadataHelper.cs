using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using System;
using System.Collections.Generic;
using System.Xml;

namespace BExIS.Modules.Mmm.UI.Helpers
{
    public class SystemMetadataHelper
    {
        public XmlDocument SetSystemValuesToMetadata(long datasetid, long version, long metadataStructureId, XmlDocument metadata, params Key[] systemKeyList)
        {
            foreach (var t in systemKeyList)
            {
                //get all mappings to automatic system from the metadata structure
                var mappings = MappingUtils.GetMappingsWhereSource((int)t, LinkElementType.Key);

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
                var xmlElement = metadata.SelectSingleNode(xpath);
                if (xmlElement != null && (XmlElement)xmlElement != null)
                {
                    ((XmlElement)xmlElement).InnerText = value;
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