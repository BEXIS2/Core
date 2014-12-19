using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Web.Shell.Areas.DDM.Models;

/// <summary>
/// 
/// </summary>        
namespace BExIS.Web.Shell.Areas.DDM.Models
{
    /// <summary>
    /// model of showing Metadata
    /// </summary>
    /// <remarks></remarks>        
    public class ShowMetadataModel
    {
        public List<string> PackageUsages { get; set; }
        public List<string> Packages { get; set; }
        public List<string> AttributesUsages { get; set; }
        public List<string> Attributes { get; set; }
        public List<PackageUsage> PU { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }


        /// <summary>
        /// Show metadata model required
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="BExIS.Web.Shell\Areas\DDM\Views\Data\ShowMetaData.cshtml"/>
        /// <param>NA</param>       
        public ShowMetadataModel()
        {
            Packages = new List<string>();
            PackageUsages = new List<string>();
            AttributesUsages = new List<string>();
            Attributes = new List<string>();
            PU = new List<PackageUsage>();
            Title = "";
            Description = "";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>        
    public class PackageUsage
    {
        /// <summary>
        /// Name of a Package
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>             
        public string Name { get; set; }

        /// <summary>
        /// List of Packages
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public List<Package> Packages { get; set; }

        /// <summary>
        /// PackageUsage includes the name and a list of Packages
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>   
        public PackageUsage()
        {
            Name = "";
            Packages = new List<Package>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>        
    public class Package
    {
        /// <summary>
        /// AttribueUsages is a dictionary
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public Dictionary<string, string> AttributeUsages;

        /// <summary>
        /// Create a new dictionary called AttributeUsages
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public Package()
        {
            AttributeUsages = new Dictionary<string, string>();
        }
    }

}
