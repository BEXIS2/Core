using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Helpers;
using Newtonsoft.Json.Linq;

//using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace BEXIS.JSON.Helpers
{
    public class MetadataStructureConverter
    {
        /// <summary>
        /// Convert a existing metadata structure into a jsonschema
        /// id of the metadata structure is required
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public JSchema ConvertToJsonSchema(long id)
        {
            if (id == 0) throw new ArgumentException("id");

            JSchema schema = new JSchema();

            // add schema version to schema
            schema = addSchemaVersion(schema);

            using (var metadataStructureManager = new MetadataStructureManager())
            {
                // load metadata structure
                var metadataStructure = metadataStructureManager.Repo.Get(id);

                // check if metadat structure exist
                if (metadataStructure == null) throw new ArgumentNullException("metadata structure with id " + id + " not exist");

                // set schema title
                schema.Title = metadataStructure.Name;
                // set schema description
                schema.Description = metadataStructure.Description;
                // set schema root element type
                schema.Type = JSchemaType.Object;

                // first child level are the MetadataPackageUsages
                // go to each MetadataPackageUsage and start recursive adding to schema
                foreach (var pu in metadataStructure.MetadataPackageUsages)
                {
                    // load usage from database
                    var packageUsage = metadataStructureManager.PackageUsageRepo.Get(pu.Id);

                    // required usages must be added to a array of the parrent
                    // addPackageUsage set required as a out property
                    bool required = false;
                    schema = addPackageUsage(packageUsage, schema, out required);

                    // if usage is required add to list
                    if (required)
                        schema.Required.Add(packageUsage.Label);
                }
            }

            return schema;
        }

        /// <summary>
        /// first level of a metadata structure are the package usages
        /// the function convert a PackageUsage into a JsonSchema and add it to the parent schema
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="schema"></param>
        /// <param name="required"></param>
        /// <returns>parent schema</returns>
        private JSchema addPackageUsage(MetadataPackageUsage usage, JSchema schema, out bool required)
        {
            var type = usage.MetadataPackage;

            JSchema current = new JSchema();

            // base information
            current.Title = usage.Label;
            current.Type = JSchemaType.Object;
            current.Description = usage.Description;

            // set required
            if (usage.MinCardinality > 0) required = true;
            else required = false;

            //children
            if (type != null && type.MetadataAttributeUsages.Any())
            {
                foreach (var metadataAttrUsage in type.MetadataAttributeUsages)
                {
                    bool _childRequired = false;
                    current = addMetadataAttrUsage(metadataAttrUsage, current, out _childRequired);

                    // if current is required add it to the parent
                    if (_childRequired) current.Required.Add(metadataAttrUsage.Label);
                    if (IsChoice(usage.Extra) && getMaxCardinality(usage) > 1) current.AnyOf.Add(current.Properties.Last().Value);
                    else if (IsChoice(usage.Extra)) current.OneOf.Add(current.Properties.Last().Value);
                }
            }

            // add system properties
            current = addAttributes(current);

            // add to parent
            // check if usage has cardinality >1 then create a array before
            if (GetJSchemaType(usage) == JSchemaType.Array)
            {
                JSchema array = new JSchema();
                array.Type = JSchemaType.Array;
                array.Items.Add(current);
                schema.Properties.Add(usage.Label, array);

                // add Range contraint
                array.MinimumItems = usage.MinCardinality;
                array.MaximumItems = usage.MaxCardinality;
            }
            else // add object to schema
            {
                schema.Properties.Add(usage.Label, current);
            }

         
            return schema;
        }

        /// <summary>
        /// convert a BaseUsage to a JsonSchema and add to incoming schema
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="schema"></param>
        /// <param name="required"></param>
        /// <returns>incoming schame with the added usage</returns>
        private JSchema addMetadataAttrUsage(BaseUsage usage, JSchema schema, out bool required)
        {
            MetadataAttribute type = null;

            if (usage is MetadataAttributeUsage)
            {
                type = ((MetadataAttributeUsage)usage).MetadataAttribute;
            }

            if (usage is MetadataNestedAttributeUsage)
            {
                type = ((MetadataNestedAttributeUsage)usage).Member;
            }

            JSchema current = new JSchema();

            current.Title = usage.Label;
            current.Description = usage.Description;

            // set required
            if (usage.MinCardinality > 0) required = true;
            else required = false;

            if (type.Self is MetadataSimpleAttribute)
            {
                //because of the extension of the values with references, the simple attribute must be defined as a jobject,
                //because json properties cannot have any further attributes. a simple attribute is defined by Jobect with the properties a
                //#text and @ref. #text receives all constraints, datatypes rules.
                // the jobject get the cardinalilties

                //set current to JObject
                current.Type = JSchemaType.Object;

                // add text
                JSchema currentText = new JSchema();
                // Datatype
                // set json schema type based on datatype input
                currentText.Type = convertToJSchemaType(type.DataType);

                //if Datatype is datetime, jsosn schema use type string
                // but need to set a format
                if (type.DataType.SystemType == "datetime")
                {
                    currentText.Format = "date";
                }

                //Contraints
                currentText = addConstraints(currentText, type.Constraints);

                //current.Properties.Add("@ref",currentRef);
                current.Properties.Add("#text", currentText);
            }

            if (type.Self is MetadataCompoundAttribute)
            {
                var mcu = (MetadataCompoundAttribute)type.Self;

                current.Type = JSchemaType.Object; // maybe also array?

                // properties
                // from usage and attrbiute
                if (mcu.MetadataNestedAttributeUsages.Any())
                {
                    foreach (var mnau in mcu.MetadataNestedAttributeUsages)
                    {
                        bool _childIsRequired = false;
                        current = addMetadataAttrUsage(mnau, current, out _childIsRequired);
                        // if current is required add it to the parent
                        if (_childIsRequired) current.Required.Add(mnau.Label);
                        if (IsChoice(usage.Extra) && getMaxCardinality(usage) > 1) current.AnyOf.Add(current.Properties.Last().Value);
                        else if (IsChoice(usage.Extra)) current.OneOf.Add(current.Properties.Last().Value);
                    }
                }
            }

            // add system props
            current = addAttributes(current);

            // check if usage has cardinality >1 then create a array before
            if (GetJSchemaType(usage) == JSchemaType.Array)
            {
                JSchema array = new JSchema();
                array.Type = JSchemaType.Array;
                array.Items.Add(current);
                schema.Properties.Add(usage.Label, array);

                // add Range contraint
                array.MinimumItems = usage.MinCardinality;
                array.MaximumItems = usage.MaxCardinality;
            }
            else // add object to schema
            {
                schema.Properties.Add(usage.Label, current);
            }

            return schema;
        }

        /// <summary>
        /// add schema version to the created schema and is the rules of this
        /// later the created schema will be validated againts this one
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        private JSchema addSchemaVersion(JSchema schema)
        {
            var value = @"http://json-schema.org/draft-07/schema";

            // add schema rules ref
            schema.SchemaVersion = new Uri(value);
            return schema;
        }

        /// <summary>
        /// Convert a DataType into a JSchemaType
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns>JSchemaType</returns>
        private JSchemaType convertToJSchemaType(DataType dataType)
        {
            switch (dataType.SystemType.ToLower())
            {
                case "string": return JSchemaType.String;
                case "integer": return JSchemaType.Integer;
                case "uint16": return JSchemaType.Integer;
                case "uint32": return JSchemaType.Integer;
                case "uint64": return JSchemaType.Integer;
                case "int16": return JSchemaType.Integer;
                case "int32": return JSchemaType.Integer;
                case "int64": return JSchemaType.Integer;
                case "boolean": return JSchemaType.Boolean;
                case "date": return JSchemaType.String;
                case "double": return JSchemaType.Number;
                case "float": return JSchemaType.Number;
                case "decimal": return JSchemaType.Number;
                default: return JSchemaType.String;
            }
        }

        /// <summary>
        /// add a set of attributes to each json object
        /// </summary>
        /// <param name="current"></param>
        /// <returns>schema with attributes</returns>
        private JSchema addAttributes(JSchema current)
        {
            // ref
            JSchema refValue = new JSchema();
            refValue.Type = JSchemaType.String;
            current.Properties.Add("@ref", refValue);

            return current;
        }

        /// <summary>
        /// add a list of incoming constraints to the current schema
        /// </summary>
        /// <param name="current"></param>
        /// <param name="constraints"></param>
        /// <returns>schema with contraints</returns>
        private JSchema addConstraints(JSchema current, ICollection<Constraint> constraints)
        {
            if (constraints.Any())
            {
                foreach (var constraint in constraints)
                {
                    // range ->
                    //  text ->minLenght & maxLenght
                    // e.g. integer -> minimum, maximum
                    if (constraint is RangeConstraint)
                    {
                        var r = (RangeConstraint)constraint;

                        // the range of a string need to def as a length in json schema
                        if (current.Type == JSchemaType.String)
                        {
                            long min = Int64.MinValue;
                            if (Int64.TryParse(r.Lowerbound.ToString(), out min))
                                current.MinimumLength = min; //may not exist
                            long max = Int64.MaxValue;
                            if (Int64.TryParse(r.Upperbound.ToString(), out max))
                                current.MaximumLength = max; //may not exist
                        }
                        else // numbers or other datatypes
                        {
                            current.Minimum = r.Lowerbound;
                            current.Maximum = r.Upperbound;
                        }
                    }
                    // enum filled by domain list
                    if (constraint is DomainConstraint)
                    {
                        var d = (DomainConstraint)constraint;
                        d.Items.ForEach(i => current.Enum.Add(new JValue(i.Value)));
                    }

                    //pattern -> regex
                    if (constraint is PatternConstraint)
                    {
                        var p = (PatternConstraint)constraint;
                        current.Pattern = p.MatchingPhrase;
                    }
                }
            }

            return current;
        }

        private int getMaxCardinality(BaseUsage usage)
        {
            if(usage.Extra==null) return usage.MaxCardinality;

            // check for choice
            var xmlnode = XmlUtility.GetXmlNodeByAttribute(usage.Extra, "type","name","choice");
            if (xmlnode != null)
            {
                var XmlAttribute = xmlnode.Attributes["max"];
                if (XmlAttribute != null)
                {
                    return int.Parse(XmlAttribute.Value);
                }
            }

            return 1; // default
        }

        private bool IsChoice(XmlNode xmlNode)
        {
            if (xmlNode != null)
            {
                XmlNode element = XmlUtility.GetXmlNodeByAttribute(xmlNode, "type", "name", "choice");
                if (element != null) return true;
            }
            return false;
        }

        private JSchemaType GetJSchemaType(BaseUsage usage)
        {
            // if choice and max cardinality >1 then array
            if (IsChoice(usage.Extra) && getMaxCardinality(usage) > 1)
                return JSchemaType.Array;
            else if (IsChoice(usage.Extra))
                return JSchemaType.Object;
            else if (getMaxCardinality(usage) > 1)
                return JSchemaType.Array;
            else
                return JSchemaType.Object;
        }
    }
}