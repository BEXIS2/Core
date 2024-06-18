using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Vaiona.Core.Serialization
{
    public class ObjGraphXmlDeserializer : ObjGraphXmlSerializationBase
    {
        private CultureInfo cult;
        private Dictionary<int, Type> deserializationTypeCache = null;
        private Dictionary<int, object> deserializationObjCache = new Dictionary<int, object>();
        private ITypeResolver typeResolver;

        public object Deserialize(XmlDocument xml, int maxSupportedVer, ITypeResolver typeResolver)
        {
            this.typeResolver = typeResolver;
            doc = xml;
            string version = doc.DocumentElement.GetAttribute("version");
            version = !string.IsNullOrWhiteSpace(version) ? version : "1";
            if (maxSupportedVer < Convert.ToInt32(version))
            {
                return null;
            }
            string culture = doc.DocumentElement.GetAttribute("culture");
            culture = !string.IsNullOrWhiteSpace(culture) ? culture : "en-US";
            cult = new CultureInfo(culture);
            return DeserializeCore(doc.DocumentElement);
        }

        private void DeserializeComplexType(object obj, Type objType, XmlNode firstChild)
        {
            // complex type
            // get the class's fields
            IDictionary<string, MemberInfo> dictMembers = GetTypeMembersInfo(objType);
            // set values for fields that are found
            for (XmlNode node = firstChild; node != null; node = node.NextSibling)
            {
                string memberName = node.Attributes["Name"].Value;
                MemberInfo member = null;
                if (dictMembers.TryGetValue(memberName, out member))
                {
                    // field is present, get value
                    object val = DeserializeCore((XmlElement)node);
                    // set value in object
                    if (member.MemberType == MemberTypes.Field)
                        ((FieldInfo)member).SetValue(obj, val);
                    if (member.MemberType == MemberTypes.Property)
                        ((PropertyInfo)member).SetValue(obj, val, null);
                }
            }
        }

        private void LoadTypeCache(XmlElement element)
        {
            XmlNodeList children = element.GetElementsByTagName("TypeInfo");
            deserializationTypeCache = new Dictionary<int, Type>(children.Count);
            foreach (XmlElement child in children)
            {
                int typeId = Convert.ToInt32(child.GetAttribute("typeid"));
                Type objType = InferTypeFromElement(child);
                deserializationTypeCache.Add(typeId, objType);
            }
        }

        private object DeserializeCore(XmlElement element)
        {
            // check if this is a reference to another object
            int objId;
            if (int.TryParse(element.GetAttribute("id"), out objId))
            {
                object objCached = GetObjFromCache(objId);
                if (objCached != null)
                {
                    return objCached;
                }
            }
            else
            {
                objId = -1;
            }

            // check for null
            string value = element.GetAttribute("value");
            if (value == NULL_VALUE)
            {
                return null;
            }

            int subItems = element.ChildNodes.Count;
            XmlNode firstChild = element.FirstChild;

            // load type cache if available
            if (element.GetAttribute("hasTypeCache") == "true")
            {
                LoadTypeCache((XmlElement)firstChild); // this should change so that gets the "Types" node by name!
                subItems--;
                firstChild = firstChild.NextSibling;
            }
            // get type
            Type objType;
            string typeId = element.GetAttribute("typeid");
            if (string.IsNullOrEmpty(typeId))
            {
                // no type id so type information must be present
                objType = InferTypeFromElement(element);
            }
            else
            {
                // there is a type id present
                objType = deserializationTypeCache[Convert.ToInt32(typeId)];
            }
            // process enum
            if (objType.IsEnum)
            {
                long val = Convert.ToInt64(value, cult);
                return Enum.ToObject(objType, val);
            }

            // process some simple types
            switch (Type.GetTypeCode(objType))
            {
                // there should be a set of checks/ decisions for empty and null values
                case TypeCode.Boolean: return string.IsNullOrEmpty(value) ? (Boolean?)null : Convert.ToBoolean(value, cult);
                case TypeCode.Byte: return string.IsNullOrEmpty(value) ? (Byte?)null : Convert.ToByte(value, cult);
                case TypeCode.Char: return string.IsNullOrEmpty(value) ? (Char?)null : Convert.ToChar(value, cult);
                case TypeCode.DBNull: return DBNull.Value;
                case TypeCode.DateTime: return string.IsNullOrEmpty(value) ? (DateTime?)null : Convert.ToDateTime(value, cult);
                case TypeCode.Decimal: return string.IsNullOrEmpty(value) ? (Decimal?)null : Convert.ToDecimal(value, cult);
                case TypeCode.Double: return string.IsNullOrEmpty(value) ? (Double?)null : Convert.ToDouble(value, cult);
                case TypeCode.Int16: return string.IsNullOrEmpty(value) ? (Int16?)null : Convert.ToInt16(value, cult);
                case TypeCode.Int32: return string.IsNullOrEmpty(value) ? (Int32?)null : Convert.ToInt32(value, cult);
                case TypeCode.Int64: return string.IsNullOrEmpty(value) ? (Int64?)null : Convert.ToInt64(value, cult);
                case TypeCode.SByte: return string.IsNullOrEmpty(value) ? (SByte?)null : Convert.ToSByte(value, cult);
                case TypeCode.Single: return string.IsNullOrEmpty(value) ? (Single?)null : Convert.ToSingle(value, cult);
                case TypeCode.String: return value;
                case TypeCode.UInt16: return string.IsNullOrEmpty(value) ? (UInt16?)null : Convert.ToUInt16(value, cult);
                case TypeCode.UInt32: return string.IsNullOrEmpty(value) ? (UInt32?)null : Convert.ToUInt32(value, cult);
                case TypeCode.UInt64: return string.IsNullOrEmpty(value) ? (UInt64?)null : Convert.ToUInt64(value, cult);
            }

            // our value
            object obj;

            if (objType.IsArray)
            {
                Type elementType = objType.GetElementType();
                MethodInfo setMethod = objType.GetMethod("Set", new Type[] { typeof(int), elementType });

                ConstructorInfo constructor = objType.GetConstructor(new Type[] { typeof(int) });
                obj = constructor.Invoke(new object[] { subItems });
                // add object to cache if necessary
                if (objId >= 0)
                {
                    deserializationObjCache.Add(objId, obj);
                }

                int i = 0;
                foreach (object val in ValuesFromNode(firstChild))
                {
                    setMethod.Invoke(obj, new object[] { i, val });
                    i++;
                }
                return obj;
            }

            // process XmlDoc
            if (objType.IsSubclassOf(typeof(XmlNode)))
            {
                if (objType == typeof(XmlElement))
                {
                    XmlDocument doc = new System.Xml.XmlDocument();
                    XmlElement nn = (XmlElement)doc.ImportNode(element.FirstChild, true);
                    nn.SetAttribute("Name", element.GetAttribute("Name"));
                    // set attribute name to Property
                    //nn. = "Property";
                    return (nn);
                }
                if (objType == typeof(XmlDocument))
                {
                    obj = (new XmlDocument()).CreateNode(objType.Name.Replace("Xml", "").ToLower(), element.GetAttribute("Name"), null);
                    if (string.IsNullOrWhiteSpace(value))
                        ((XmlNode)obj).InnerXml = element.InnerXml;
                    else
                        ((XmlNode)obj).InnerXml = value;
                    return (obj);
                }
            }

            // create a new instance of the object
            obj = Activator.CreateInstance(objType, true);
            // add object to cache if necessary
            if (objId >= 0)
            {
                deserializationObjCache.Add(objId, obj);
            }

            IXmlSerializable xmlSer = obj as IXmlSerializable;
            if (xmlSer == null)
            {
                IList lst = obj as IList;
                if (lst == null)
                {
                    IDictionary dict = obj as IDictionary;
                    if (dict == null)
                    {
                        if (objType == typeof(DictionaryEntry) ||
                            (objType.IsGenericType &&
                             objType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)))
                        {
                            // load all field contents in a dictionary
                            Dictionary<string, object> properties = new Dictionary<string, object>(element.ChildNodes.Count);
                            for (XmlNode node = firstChild; node != null; node = node.NextSibling)
                            {
                                object val = DeserializeCore((XmlElement)node);
                                properties.Add(node.Name, val);
                            }
                            // return the dictionary
                            return properties;
                        }
                        // complex type
                        DeserializeComplexType(obj, objType, firstChild);
                    }
                    else
                    {
                        // it's a dictionary
                        foreach (object val in ValuesFromNode(firstChild))
                        {
                            // should be a Dictionary
                            Dictionary<string, object> dictVal = (Dictionary<string, object>)val;
                            if (dictVal.ContainsKey("key"))
                            {
                                // should be a KeyValuePair
                                dict.Add(dictVal["key"], dictVal["value"]);
                            }
                            else
                            {
                                // should be a DictionaryEntry
                                dict.Add(dictVal["_key"], dictVal["_value"]);
                            }
                        }
                    }
                }
                else
                {
                    // it's a list
                    foreach (object val in ValuesFromNode(firstChild))
                    {
                        lst.Add(val);
                    }
                }
            }
            else
            {
                // the object can deserialize itself
                StringReader sr = new StringReader(element.InnerXml);
                XmlReader rd = XmlReader.Create(sr);
                xmlSer.ReadXml(rd);
                rd.Close();
                sr.Close();
            }
            return obj;
        }

        private IEnumerable ValuesFromNode(XmlNode firstChild)
        {
            for (XmlNode node = firstChild; node != null; node = node.NextSibling)
            {
                yield return DeserializeCore((XmlElement)node);
            }
        }

        private object GetObjFromCache(int objId)
        {
            object obj;
            if (deserializationObjCache.TryGetValue(objId, out obj))
            {
                return obj;
            }
            return null;
        }

        private Type InferTypeFromElement(XmlElement element)
        {
            Type objType;
            string typeFullName = element.GetAttribute("type");
            string assemblyFullName = element.GetAttribute("assembly");

            if (typeResolver != null)
            {
                typeResolver.ResolveType(ref assemblyFullName, ref typeFullName);
            }
            try
            {
                if (string.IsNullOrEmpty(assemblyFullName))
                {
                    // type is directly loadable
                    objType = Type.GetType(typeFullName, true);
                }
                else
                {
                    Assembly asm = Assembly.Load(assemblyFullName);
                    objType = asm.GetType(typeFullName, true);
                }
            }
            catch
            {
                objType = typeof(string);
            }
            return objType;
        }
    }
}