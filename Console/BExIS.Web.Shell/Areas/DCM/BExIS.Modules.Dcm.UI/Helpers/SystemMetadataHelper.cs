using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class SystemMetadataHelper
    {
        public XmlDocument SetSystemValuesToMetadata(long datasetid, long version,double tag, long metadataStructureId, XmlDocument metadata, params Key[] systemKeyList)
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
                                case Key.Tag:
                                    {
                                        metadata = setValue(mapping.Target.XPath, tag.ToString(), metadata); break;
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

        public XmlDocument SetSytemValueToMetadata(string value, Key key, long metadataStructureId, XmlDocument metadata)
        {
            if(string.IsNullOrEmpty(value)) return metadata;
            if(metadata==null) return metadata;
            if (metadataStructureId <= 0) return metadata;

            var mappings = MappingUtils.GetMappingsWhereSource((int)key, LinkElementType.Key, 2);

            if (mappings != null && mappings.Any())
            {
                var m = mappings.FirstOrDefault();
                metadata = setValue(m.Target.XPath, value, metadata); 
            }

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