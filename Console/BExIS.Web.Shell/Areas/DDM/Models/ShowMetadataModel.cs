using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;

/// <summary>
///
/// </summary>
namespace BExIS.Modules.Ddm.UI.Models
{
    /// <summary>
    /// model of showing Metadata
    /// </summary>
    /// <remarks></remarks>
    public class ShowMetadataModel
    {
        public List<PackageUsageModel> PU { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Show metadata model required
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="BExIS.Web.Shell\Areas\ddm\Views\Data\ShowMetaData.cshtml"/>
        /// <param>NA</param>
        public ShowMetadataModel()
        {
            PU = new List<PackageUsageModel>();
            Title = "";
            Description = "";
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class PackageUsageModel : BaseModelElement
    {
        /// <summary>
        /// List of Packages
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public List<BaseModelElement> Attributes { get; set; }

        /// <summary>
        /// PackageUsage includes the name and a list of Packages
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public PackageUsageModel()
        {
            Name = "";
            DisplayName = "";
            Attributes = new List<BaseModelElement>();
            Type = XmlNodeType.MetadataPackageUsage;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class PackageModel : BaseModelElement
    {
        /// <summary>
        /// AttribueUsages is a dictionary
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public List<BaseModelElement> Attributes;

        /// <summary>
        /// Create a new dictionary called AttributeUsages
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public PackageModel()
        {
            Name = "";
            DisplayName = "";
            Attributes = new List<BaseModelElement>();
        }
    }

    public class SimpleAttributeModel : BaseModelElement
    {
        public string Value { get; set; }

        public SimpleAttributeModel()
        {
            Name = "";
            DisplayName = "";
            Value = "";
        }
    }

    public class CompundAttributeModel : BaseModelElement
    {
        public List<BaseModelElement> Childrens { get; set; }

        public CompundAttributeModel()
        {
            Name = "";
            DisplayName = "";
            Childrens = new List<BaseModelElement>();
        }
    }

    public abstract class BaseModelElement
    {
        public String Name { get; set; }
        public String DisplayName { get; set; }
        public XmlNodeType Type { get; set; }
    }
}