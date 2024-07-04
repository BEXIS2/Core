using System.Collections.Generic;

/// <summary>
///
/// </summary>
namespace BExIS.Utils.Models
{
    /// <summary>
    ///
    /// </summary>
    public enum Direction
    {
        none,
        increase,
        decrease
    }

    /// <summary>
    ///
    /// </summary>
    public enum SearchComponentBaseType
    {
        //Base,
        Category,

        Facet,
        Property,
        General
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class Category : SearchComponentBase
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public Category()
        {
            this.Type = SearchComponentBaseType.Category;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public bool HasFacet { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string Value { get; set; }
    }

    public class General : SearchComponentBase
    {
        public General()
        {
            this.Type = SearchComponentBaseType.General;
            this.IsVisible = false;
        }

        public string Value { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class Facet : SearchComponentBase //, IHierarchyData. maybe it is good to implement this interface in order to make UI data binding easier!
    {
        /// <summary>
        ///
        /// </summary>
        public Facet()
        {
            this.Type = SearchComponentBaseType.Facet;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public List<Facet> Childrens { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public int Count { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public Facet Parent { get; set; }

        /// <summary>
        /// Displayed Name
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string Text { get; set; }

        /// <summary>
        /// to work with.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string Value { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class Property : SearchComponentBase
    {
        public const string AGGREGATION_TYPE = "aggregationType";
        public const string AT_DISTINCT = "distinct";
        public const string AT_UNION = "union";
        public const string DATA_SOURCE_KEY = "dataSourceKey";
        public const string DATATYPE_DATE = "date";
        public const string DATATYPE_STRING = "string";
        public const string DATE_FORMAT = "format";
        public const string DATE_TYPE = "dataType";
        public const string DEFAULT_ALL = "all";
        public const string DEFAULT_VALUE_NODE = "defaultValue";
        public const string DISPLAY_TITLE = "displayTitle";
        public const string ID = "id";
        public const string UI_COMPONENT = "uiComponent";
        public const string UI_ITEM = "item";
        public const string UI_LIST = "list";
        public const string UI_RANGE = "range";

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public Property()
        {
            this.Type = SearchComponentBaseType.Property;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string AggregationType { get; set; }

        /// <summary>
        /// Key for searching
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string DataSourceKey { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string DataType { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public Direction Direction { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string DisplayTitle { get; set; }

        /// <summary>
        /// If the Propertie based on a date then we need the dateformat
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public IEnumerable<string> Formats { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string UIComponent { get; set; }

        /// <summary>
        /// List of Values for the Property
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public IEnumerable<string> Values { get; set; }

        public string SelectedValue { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class SearchComponentBase
    {
        public SearchComponentBase()
        {
            this.IsVisible = true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string DefaultValue { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string DisplayName { get; set; }

        public bool IsVisible { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public SearchComponentBaseType Type { get; set; }
    }

    // no needed -  change textvalues to string list

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class TextValue : SearchComponentBase
    {
        public string Value { get; set; }
    }
}