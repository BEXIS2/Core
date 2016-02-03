using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Telerik.Web.Mvc.UI;

namespace BExIS.Web.Shell.Areas.MSM.Models
{
    public class TestModel
    {

       

        public String Name { get; set; }
        public TreePartialModel Left { get; set; }
        public TreePartialModel Right { get; set; }
        public List<AddTextFieldModel> TextFields { get; set; }

        //public TreeViewItemModel xmlTree { get; set; }

      

        public TestModel() {

            TextFields = new List<AddTextFieldModel>();
            TextFields.Add(new AddTextFieldModel("hallo", "123"));
            //TextFields.Add(new AddTextFieldModel("hallo", "123"));
            this.Name = "Metadata Mapping Module";
            string xsd1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  
  <xs:element name=""Address"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""Recipient"" type=""xs:string"" />
        <xs:element name=""House"" type=""xs:string"" minOccurs=""0""/>
        <xs:element name=""Street"" type=""xs:string"" minOccurs=""0""/>
        <xs:element name=""Town"" type=""xs:string"" minOccurs=""0""/>
        <xs:element name=""PostCode"" type=""xs:string"" minOccurs=""0""/>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>
";

            string xsd2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  
  <xs:element name=""Address"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""Recipient"" type=""xs:string"" />
        <xs:element name=""House"" type=""xs:string"" minOccurs=""0""/>
        <xs:element name=""Street"" type=""xs:string"" minOccurs=""0""/>
        <xs:element name=""City"" type=""xs:string"" minOccurs=""0""/>
        <xs:element name=""PostCode"" type=""xs:string"" minOccurs=""0""/>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>
";

            Left = new TreePartialModel(1,"Metadata Schema 1",xsd1);
            Right = new TreePartialModel(2, "Metadata Schema 2", xsd2);
        }

    }

   
}