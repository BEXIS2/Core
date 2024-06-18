using System;

namespace Vaiona.Core.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class XmlIgnoreBaseTypeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ObjGraphXmlSerializationOptionsAttribute : Attribute
    {
        public ObjGraphXmlSerializer.SerializationOptions SerializationOptions = new ObjGraphXmlSerializer.SerializationOptions();

        public ObjGraphXmlSerializationOptionsAttribute(bool useTypeCache, bool useGraphSerialization)
        {
            SerializationOptions.UseTypeCache = useTypeCache;
            SerializationOptions.UseGraphSerialization = useGraphSerialization;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class XmlSerializeAsCustomTypeAttribute : Attribute
    {
    }
}