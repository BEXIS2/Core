using BExIS.Dim.Entities.Mapping;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dim.UI.Models.Mapping
{
    public class MappingMainModel
    {
        public LinkElementRootModel Source { get; set; }
        public LinkElementRootModel Target { get; set; }
        public List<ComplexMappingModel> ParentMappings { get; set; }
        public List<LinkElementRootListItem> SelectionList { get; set; }

        public MappingMainModel()
        {
            ParentMappings = new List<ComplexMappingModel>();
            SelectionList = new List<LinkElementRootListItem>();
        }

    }

    public class LinkElementRootListItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public LinkElementRootListItem(long elementId, string name, LinkElementType type)
        {
            Id = elementId + "_" + type;
            Name = name + " (" + type + ")";

        }

    }

    public class LinkElementRootModel
    {
        public long Id { get; set; }
        public long ElementId { get; set; }
        public string Name { get; set; }
        public LinkElementType Type { get; set; }
        public List<LinkElementContainerModel> LinkElementContainers { get; set; }
        public List<LinkElementModel> LinkElements { get; set; }
        public LinkElementPostion Position { get; set; }

        public LinkElementRootModel(LinkElementType type, long elementId, string name, LinkElementPostion position)
        {
            Id = ElementId;
            ElementId = elementId;
            Type = type;
            Name = name;
            LinkElementContainers = new List<LinkElementContainerModel>();
            LinkElements = new List<LinkElementModel>();
            Position = position;
        }
    }

    public class LinkElementContainerModel
    {
        public LinkElementType Type { get; set; }
        public LinkElementComplexity Complexity { get; set; }
        public List<LinkElementModel> LinkElements { get; set; }
        public LinkElementPostion Position { get; set; }

        public LinkElementContainerModel(LinkElementComplexity complexity, LinkElementPostion position)
        {
            //Type = type;
            Complexity = complexity;
            LinkElements = new List<LinkElementModel>();
            Position = position;
        }
    }


    public class LinkElementModel
    {
        public long Id { get; set; }
        public long ElementId { get; set; }
        public LinkElementType Type { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String XPath { get; set; }
        public LinkElementPostion Position { get; set; }
        public LinkElementComplexity Complexity { get; set; }
        public string Mask { get; set; }

        public List<LinkElementModel> Children { get; set; }
        public LinkElementModel Parent { get; set; }

        public LinkElementModel()
        {
            Id = -1;
            ElementId = -1;
            Name = "";
            XPath = "";
            Description = "";
            Children = new List<LinkElementModel>();
            Parent = null;
            Complexity = LinkElementComplexity.None;
            Mask = "";
            Type = LinkElementType.SimpleMetadataAttribute;
        }

        public LinkElementModel(
            long id,
            long elementid,
            LinkElementType type,
            string name,
            string xpath,
            LinkElementPostion position,
            LinkElementComplexity complexity,
            string mask,
            string description = ""
            )
        {
            Id = id;
            ElementId = elementid;
            Type = type;
            Name = name;
            XPath = xpath;
            Description = description;
            Position = position;
            Children = new List<LinkElementModel>();
            Complexity = complexity;
            Mask = mask;
        }
    }

    public class TransformationRuleModel
    {
        public long Id { get; set; }
        public string RegEx { get; set; }

        public TransformationRuleModel()
        {
            Id = 0;
            RegEx = "";
        }
    }


    public enum LinkElementPostion
    {
        Source,
        Target
    }

}