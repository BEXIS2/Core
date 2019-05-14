using BExIS.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers.API
{
    public class ApiHelper
    {
        private const string sourceFile = "apiConfig.xml";
        private XmlDocument requirementXmlDocument = null;

        public Dictionary<string, string> Settings = null;

        public static string CELLS = "cells";

        public ApiHelper()
        {
            requirementXmlDocument = new XmlDocument();
            Settings = new Dictionary<string, string>();
            Load();
        }

        private void Load()
        {
            try
            {

                string filepath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), sourceFile);

                if (FileHelper.FileExist(filepath))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(filepath);
                    requirementXmlDocument = xmlDoc;
                    XmlNodeList apiSettings = requirementXmlDocument.GetElementsByTagName("apiSettings");

                    if (apiSettings.Count == 1)
                    {
                        var s = apiSettings[0];

                        if (s.HasChildNodes)
                        {
                            foreach (XmlNode c in s.ChildNodes)
                            {
                                if (c is XmlElement)
                                {
                                    XmlElement xmlElement = (XmlElement)c;

                                    if (xmlElement.HasAttribute("value") && xmlElement.HasAttribute("value"))
                                    {
                                        string key = xmlElement.GetAttribute("key");
                                        string value = xmlElement.GetAttribute("value");
                                        Settings.Add(key, value);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("no or more then 1 apiSettings nodes in the xmldocument. have a look in the apiConfig.xml file in the workspace/modules/dim");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }

    }
}
