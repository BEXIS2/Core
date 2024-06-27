using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Vaiona.Core.Serialization
{
    /// <summary>
    /// In case of assembly name/ version change or type change, programmer can use this interface to implement proper
    /// type reolver.
    /// The main use case of this approach in to sopprt code change over time
    /// </summary>
    public interface ITypeResolver
    {
        void ResolveType(ref string assemblyFullName, ref string typeFullName);
    }

    public interface IObjectTransfromer
    {
        object ExportTo<S>(object source, string instanceName, string rootElementName = "Root", int version = 1, bool resetExportInfo = false);

        object ExportTo(Type sourceType, object source, string instanceName, string rootElementName = "Root", int version = 1, bool resetExportInfo = false);

        T ImportFrom<T>(object source, ITypeResolver typeResolver = null, int version = 1, bool resetImportInfo = false);

        object ImportFrom(Type returnType, object source, ITypeResolver typeResolver = null, int version = 1, bool resetImportInfo = false);
    }

    public abstract class ObjGraphXmlSerializationBase
    {
        /// <summary>
        /// This value is the representation of the NULL value in XML.
        /// It is used for de/serialization. ANy other applicatio/logic thta accesses the raw XML should handle this case.
        /// </summary>
        public static readonly string NULL_VALUE = "_null_null";

        protected static Dictionary<Type, IDictionary<string, MemberInfo>> memberInfoCache = new Dictionary<Type, IDictionary<string, MemberInfo>>();

        protected XmlDocument doc = new XmlDocument();

        protected IDictionary<string, MemberInfo> GetTypeMembersInfo(Type objType)
        {
            //string typeName = objType.FullName;
            IDictionary<string, MemberInfo> effectiveMambers;
            if (!memberInfoCache.TryGetValue(objType, out effectiveMambers))
            {
                // fetch fields
                MemberInfo[] members = objType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(p => p.MemberType == MemberTypes.Property || p.MemberType == MemberTypes.Field).ToArray();

                //objType.GetProperties
                Dictionary<string, MemberInfo> dict = new Dictionary<string, MemberInfo>(members.Length);
                foreach (MemberInfo member in members)
                {
                    if (!member.GetType().IsSubclassOf(typeof(MulticastDelegate)))
                    {
                        object[] attribs = member.GetCustomAttributes(typeof(XmlIgnoreAttribute), false);
                        if (attribs.Length == 0)
                        {
                            dict.Add(member.Name, member);
                        }
                    }
                }

                // check base class as well
                Type baseType = objType.BaseType;
                if (baseType != null && baseType != typeof(object))
                {
                    // should we include this base class?
                    object[] attribs = baseType.GetCustomAttributes(typeof(XmlIgnoreBaseTypeAttribute), false);
                    if (attribs.Length == 0)
                    {
                        IDictionary<string, MemberInfo> baseMembers = GetTypeMembersInfo(baseType);
                        // add fields
                        foreach (KeyValuePair<string, MemberInfo> member in baseMembers)
                        {
                            string memberName = member.Value.Name;
                            if (dict.ContainsKey(memberName))
                            {
                                // make field name unique
                                memberName = "base." + memberName;
                            }
                            dict.Add(memberName, member.Value);
                        }
                    }
                }

                effectiveMambers = dict;
                memberInfoCache.Add(objType, effectiveMambers);
            }
            return effectiveMambers;
        }

        protected class TypeInfo
        {
            internal int TypeId;
            internal XmlElement OnlyElement;

            internal void WriteTypeId(XmlElement element)
            {
                element.SetAttribute("typeid", TypeId.ToString());
            }
        }
    }
}