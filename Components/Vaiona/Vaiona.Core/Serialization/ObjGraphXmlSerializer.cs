using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Vaiona.Core.Serialization
{
    public class ObjGraphXmlSerializer : ObjGraphXmlSerializationBase
    {
        private Dictionary<Type, TypeInfo> typeCache = new Dictionary<Type, TypeInfo>();
        private Dictionary<Type, IDictionary<ObjKeyForCache, ObjInfo>> objCache = new Dictionary<Type, IDictionary<ObjKeyForCache, ObjInfo>>();
        private int objCacheNextId = 0;
        private SerializationOptions options;
        public Type ObjectType { get; protected set; }

        public ObjGraphXmlSerializer(object obj, Type objectType)
        {
            this.ObjectType = objectType ?? obj.GetType();
            setUp(obj);
        }

        private void setUp(object obj)
        {
            // determine serialization options
            SerializationOptions serOptions = new SerializationOptions();
            if (obj != null)
            {
                object[] attribs = ObjectType.GetCustomAttributes(typeof(ObjGraphXmlSerializationOptionsAttribute), false);
                if (attribs.Length > 0)
                {
                    serOptions = ((ObjGraphXmlSerializationOptionsAttribute)attribs[0]).SerializationOptions;
                }
            }
            options = serOptions;
        }

        private void SetTypeInfo(Type objType, XmlElement element)
        {
            if (!options.UseTypeCache)
            {
                // add detailed type information
                WriteTypeToNode(element, objType);
                return;
            }
            TypeInfo typeInfo;
            if (!typeCache.ContainsKey(objType))
            {
                // add type to cache
                typeInfo = new TypeInfo();
                typeInfo.TypeId = typeCache.Count;
                typeInfo.OnlyElement = element;
                typeCache.Add(objType, typeInfo);
            }
            if (typeCache.TryGetValue(objType, out typeInfo))
            {
                XmlElement onlyElement = typeInfo.OnlyElement;
                if (onlyElement != null)
                {
                    // set the type of the element to be a reference to the type ID
                    // since the element is no longer the only one of this type
                    typeInfo.WriteTypeId(onlyElement);
                    onlyElement.RemoveAttribute("type");
                    onlyElement.RemoveAttribute("assembly");
                    typeInfo.OnlyElement = null;
                }
                typeInfo.WriteTypeId(element);
            }
            else
            {
                throw new Exception("Type is not added to type cache!!!");
                //// add type to cache
                //typeInfo = new TypeInfo();
                //typeInfo.TypeId = typeCache.Count;
                //typeInfo.OnlyElement = element;
                //typeCache.Add(objType, typeInfo);
                //// add detailed type information
                //WriteTypeToNode(element, objType);
            }
        }

        private void WriteTypeToNode(XmlElement element, Type objType)
        {
            element.SetAttribute("type", objType.FullName);
            element.SetAttribute("assembly", objType.Assembly.FullName);
        }

        private XmlElement GetTypeInfoNode()
        {
            XmlElement element = doc.CreateElement("Types");
            foreach (KeyValuePair<Type, TypeInfo> kv in typeCache)
            {
                if (kv.Value.OnlyElement == null)
                {
                    // there is more than one element having this type
                    XmlElement e = doc.CreateElement("TypeInfo");
                    kv.Value.WriteTypeId(e);
                    WriteTypeToNode(e, kv.Key);
                    element.AppendChild(e);
                }
            }
            return element.HasChildNodes ? element : null;
        }

        public XmlDocument Serialize(object obj, int ver, string instanceName, string rootElementName)
        {
            XmlElement element = SerializeCore(instanceName, obj, ObjectType, rootElementName);
            element.SetAttribute("version", ver.ToString());
            element.SetAttribute("culture", Thread.CurrentThread.CurrentCulture.ToString());
            // add typeinfo
            XmlElement typeInfo = GetTypeInfoNode();
            if (typeInfo != null)
            {
                element.PrependChild(typeInfo);
                element.SetAttribute("hasTypeCache", "true");
            }
            // add serialized data
            doc.AppendChild(element);
            return doc;
        }

        private bool AddObjToCache(Type objType, object obj, XmlElement element)
        {
            ObjKeyForCache kfc = new ObjKeyForCache(obj);
            IDictionary<ObjKeyForCache, ObjInfo> entry;
            if (objCache.TryGetValue(objType, out entry))
            {
                // look for this particular object
                ObjInfo objInfoFound;
                if (entry.TryGetValue(kfc, out objInfoFound))
                {
                    // the object has already been added
                    if (objInfoFound.OnlyElement != null)
                    {
                        objInfoFound.WriteObjId(objInfoFound.OnlyElement);
                        objInfoFound.OnlyElement = null;
                    }
                    // write id to element
                    objInfoFound.WriteObjId(element);
                    return false;
                }
            }
            else
            {
                // brand new type in the cache
                entry = new Dictionary<ObjKeyForCache, ObjInfo>(1);
                objCache.Add(objType, entry);
            }
            // object not found, add it
            ObjInfo objInfo = new ObjInfo();
            objInfo.Id = objCacheNextId;
            objInfo.OnlyElement = element;
            entry.Add(kfc, objInfo);
            objCacheNextId++;
            return true;
        }

        private bool CheckForcedSerialization(Type objType)
        {
            object[] attribs = objType.GetCustomAttributes(typeof(XmlSerializeAsCustomTypeAttribute), false);
            return attribs.Length > 0;
        }

        private XmlElement SerializeCore(string name, object obj, Type objType, string elementType)
        {
            XmlElement element = doc.CreateElement(elementType);
            element.SetAttribute("Name", name);
            if (obj == null)
            {
                element.SetAttribute("value", NULL_VALUE);
                return element;
            }

            if (objType.IsClass && objType != typeof(string))
            {
                // check if we have already serialized this object
                if (options.UseGraphSerialization && !AddObjToCache(objType, obj, element))
                {
                    return element;
                }
                // the object has just been added
                SetTypeInfo(objType, element);

                if (CheckForcedSerialization(objType))
                {
                    // serialize as complex type
                    SerializeComplexType(obj, objType, element);
                    return element;
                }

                IXmlSerializable xmlSer = obj as IXmlSerializable;
                if (xmlSer == null)
                {
                    // does not know about automatic serialization
                    if (objType.IsSubclassOf(typeof(XmlNode)))
                    {
                        SerializeXmlType(obj, objType, element);
                    }
                    else
                    {
                        IEnumerable arr = obj as IEnumerable;
                        if (arr == null)
                        {
                            SerializeComplexType(obj, objType, element);
                        }
                        else
                        {
                            SerializeIEnumerable(name, arr, element);
                        }
                    }
                }
                else
                {
                    // can perform the serialization itself
                    StringBuilder sb = new StringBuilder();
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                    settings.Encoding = Encoding.UTF8;
                    settings.OmitXmlDeclaration = true;
                    XmlWriter wr = XmlWriter.Create(sb, settings);
                    wr.WriteStartElement("value");
                    xmlSer.WriteXml(wr);
                    wr.WriteEndElement();
                    wr.Close();

                    element.InnerXml = sb.ToString();
                }
            }
            else
            {
                // the object has just been added
                SetTypeInfo(objType, element);

                if (CheckForcedSerialization(objType))
                {
                    // serialize as complex type
                    SerializeComplexType(obj, objType, element);
                    return element;
                }

                if (objType.IsEnum)
                {
                    object val = Enum.Format(objType, obj, "d");
                    element.SetAttribute("value", val.ToString());
                }
                else
                {
                    if (objType.IsPrimitive || objType == typeof(string) ||
                        objType == typeof(DateTime) || objType == typeof(decimal))
                    {
                        element.SetAttribute("value", obj.ToString());
                    }
                    else
                    {
                        // this is most probably a struct
                        SerializeComplexType(obj, objType, element);
                    }
                }
            }

            return element;
        }

        private void SerializeXmlType(object obj, Type objType, XmlElement element)
        {
            try
            {
                element.InnerXml = ((XmlDocument)obj).InnerXml; // obj is an XmlDoc
            }
            catch
            {
                XmlNode node = doc.ImportNode((XmlNode)obj, true); // obj is an XmlElement
                element.AppendChild(node);
            }
            //element.AppendChild((XmlNode)obj);
            //element. SetAttribute("value", ((XmlNode)obj).InnerXml); // it should be changed to somethig else to put the Xmldocuments inside the body of parent elements
        }

        private void SerializeIEnumerable(string name, IEnumerable arr, XmlElement element)
        {
            foreach (object arrObj in arr)
            {
                XmlElement e = SerializeCore(arrObj.GetType().Name, arrObj, arrObj.GetType(), "Item");
                element.AppendChild(e);
            }
        }

        private void SerializeComplexType(object obj, Type objType, XmlElement element)
        {
            //Type objType = obj.GetType();
            // get all instance fields
            IDictionary<string, MemberInfo> members = GetTypeMembersInfo(objType);
            foreach (KeyValuePair<string, MemberInfo> member in members)
            {
                // serialize field
                XmlElement e = null;
                if (member.Value.MemberType == MemberTypes.Field)
                {
                    FieldInfo f = ((FieldInfo)member.Value);
                    object value = f.GetValue(obj);
                    Type effectiveType = (value.GetType() != typeof(object)) ? value.GetType() : f.FieldType;
                    e = SerializeCore(member.Key, value, effectiveType, "Field");
                }
                else if (member.Value.MemberType == MemberTypes.Property)
                {
                    PropertyInfo p = ((PropertyInfo)member.Value);
                    object value = p.GetValue(obj, null);
                    Type effectiveType = (value != null && value.GetType() != typeof(object)) ? value.GetType() : p.PropertyType;
                    e = SerializeCore(member.Key, value, effectiveType, "Property");
                }
                element.AppendChild(e);
            }
        }

        private class ObjInfo
        {
            internal int Id;
            internal XmlElement OnlyElement;

            internal void WriteObjId(XmlElement element)
            {
                element.SetAttribute("id", Id.ToString());
            }
        }

        private struct ObjKeyForCache : IEquatable<ObjKeyForCache>
        {
            private object m_obj;

            public ObjKeyForCache(object obj)
            {
                m_obj = obj;
            }

            public bool Equals(ObjKeyForCache other)
            {
                return object.ReferenceEquals(m_obj, other.m_obj);
            }
        }

        public class SerializationOptions
        {
            public bool UseTypeCache = true;
            public bool UseGraphSerialization = true;
        }
    }
}