namespace BExIS.Dim.Entities.Mapping
{
    public enum Key
    {
        Title = 1,
        Description = 2
    }

    public enum LinkElementComplexity
    {
        None,
        Simple,
        Complex
    }

    public enum LinkElementType
    {
        MetadataStructure,
        XSD,
        System,
        SimpleMetadataAttribute,
        ComplexMetadataAttribute,
        MetadataAttributeUsage,
        MetadataNestedAttributeUsage,
        MetadataPackage,
        MetadataPackageUsage,
        PartyType,
        PartyCustomType,
        Key,
        Role
    }



    public class LinkElementMetadataStructure : LinkElement { }
    public class LinkElementXsd : LinkElement { }
    public class LinkElementSystem : LinkElement { }
    public class LinkElementSimpleMetadataAttribute : LinkElement { }
    public class LinkElementComplexMetadatAttribute : LinkElement { }
    public class LinkElementMetadatAttributeUsage : LinkElement { }
    public class LinkElementMetadataNestedAttributeUsage : LinkElement { }
    public class LinkElementMetadataPackage : LinkElement { }
    public class LinkElementMetadataPackageUsage : LinkElement { }
    public class LinkElementPartyType : LinkElement { }
    public class LinkElementParty : LinkElement { }
    public class LinkElementKey : LinkElement { }
    public class LinkElementRole : LinkElement { }



}
