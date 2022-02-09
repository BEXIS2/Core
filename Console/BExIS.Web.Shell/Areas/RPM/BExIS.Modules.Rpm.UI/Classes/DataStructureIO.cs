﻿using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace BExIS.Modules.Rpm.UI.Classes
{
    public static class DataStructureIO
    {
        public static void deleteTemplate(long dataStructureId)
        {
            ExcelTemplateProvider provider = new ExcelTemplateProvider();
            provider.deleteTemplate(dataStructureId);
        }

        private static XmlDocument createOderNode(StructuredDataStructure structuredDataStructure)
        {
            using (DataStructureManager dsm = new DataStructureManager())
            {
                XmlDocument doc = (XmlDocument)structuredDataStructure.Extra;
                XmlNode order;

                if (doc == null)
                {
                    doc = new XmlDocument();
                    XmlNode root = doc.CreateNode(XmlNodeType.Element, "extra", null);
                    doc.AppendChild(root);
                }
                if (doc.GetElementsByTagName("order").Count == 0)
                {

                    if (structuredDataStructure.Variables.Count > 0)
                    {
                        order = doc.CreateNode(XmlNodeType.Element, "order", null);

                        foreach (Variable v in structuredDataStructure.Variables)
                        {

                            XmlNode variable = doc.CreateNode(XmlNodeType.Element, "variable", null);
                            variable.InnerText = v.Id.ToString();
                            order.AppendChild(variable);
                        }

                        doc.FirstChild.AppendChild(order);
                        structuredDataStructure.Extra = doc;
                        dsm.UpdateStructuredDataStructure(structuredDataStructure);
                    }
                }
                return doc;
            }
        }
        public static List<Variable> getOrderedVariables(StructuredDataStructure structuredDataStructure)
        {
            return structuredDataStructure.Variables.OrderBy(v => v.OrderNo).ToList();
            
        }
        public static StructuredDataStructure setVariableOrder(StructuredDataStructure dataStructure, List<long> orderList)
        {
            DataStructureManager dsm = null;
            try
            {
                dsm = new DataStructureManager();
                if(orderList != null && orderList.Count > 0)
                {
                    foreach (Variable v in dataStructure.Variables)
                    {
                        v.OrderNo = orderList.IndexOf(v.Id) + 1;
                        Debug.WriteLine(v.Id + "|" + v.OrderNo);
                    }
                }
                return dsm.UpdateStructuredDataStructure(dataStructure);
            }
            finally
            {
                dsm.Dispose();
            }          
        }

        public static void convertOrder(StructuredDataStructure structuredDataStructure)
        {
            XmlDocument doc = (XmlDocument)structuredDataStructure.Extra;
            XmlNode order;

            if (doc != null)
            {
                if (doc.GetElementsByTagName("order").Count > 0)
                {
                    order = doc.GetElementsByTagName("order")[0];
                    List<long> orderedVariableIDs = new List<long>();
                    if (structuredDataStructure.Variables.Count != 0)
                    {
                        foreach (XmlNode x in order)
                        {
                            foreach (Variable v in structuredDataStructure.Variables)
                            {
                                if (v.Id == Convert.ToInt64(x.InnerText))
                                    orderedVariableIDs.Add(v.Id);

                            }
                        }
                    }
                    setVariableOrder(structuredDataStructure, orderedVariableIDs);
                    order.ParentNode.RemoveChild(order);
                }
            }
        }

    }
}