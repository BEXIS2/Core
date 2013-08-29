using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace BExIS.Search.Model
{

    public enum SearchComponentBaseType
    {
        //Base,
        Category, 
        Facet,       
        Property,
        General
    }

    public class SearchComponentBase 
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string DefaultValue { get; set; }
        public SearchComponentBaseType Type { get; set; }
    }

    public class Facet : SearchComponentBase //, IHierarchyData. maybe it is good to implement this interface in order to make UI data binding easier!
    {
        public Facet() 
        {

            this.Type = SearchComponentBaseType.Facet;
        }

        /// <summary>
        /// Displayed Name
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// to work with.
        /// </summary>
        public string Value { get; set; }

        public int Count { get; set; }

        public Facet Parent { get; set; }

        public List<Facet> Childrens { get; set; }

       
    }

    public class Category : SearchComponentBase
    {
        public Category()
        {

            this.Type = SearchComponentBaseType.Category;
        }

        public string Value { get; set; }
        public bool HasFacet { get; set; }
    }

    public class Property : SearchComponentBase
    {
        public const string ID = "id";
        public const string DISPLAY_TITLE = "displayTitle";
        public const string DATA_SOURCE_KEY = "dataSourceKey";
        public const string UI_COMPONENT = "uiComponent";
        public const string DEFAULT_VALUE_NODE = "defaultValue";
        public const string AGGREGATION_TYPE = "aggregationType";
        public const string DATE_FORMAT = "format";
        public const string DATE_TYPE = "dataType";

        public const string AT_UNION = "union";
        public const string AT_DISTINCT = "distinct";

        public const string UI_LIST = "list";
        public const string UI_ITEM = "item";
        public const string UI_RANGE = "range";

        public const string DATATYPE_DATE = "date";
        public const string DATATYPE_STRING = "string";

        public const string DEFAULT_ALL = "all";

        public Property() 
        {

            this.Type = SearchComponentBaseType.Property;
        }

        public string DisplayTitle { get; set; }
        /// <summary>
        /// Key for searching
        /// </summary>
        public string DataSourceKey { get; set; }

        public string UIComponent { get; set; }

        public string AggregationType { get; set; }
        
        public string DataType { get; set; }

        public Direction Direction { get; set; }

        /// <summary>
        /// If the Propertie based on a date then we need the dateformat
        /// </summary>
        public IEnumerable<string> Formats { get; set; }

        /// <summary>
        /// List of Values for the Property
        /// </summary>
        public IEnumerable<string> Values { get; set; }

    
       
    }

    // no needed -  change textvalues to string list
    public class TextValue : SearchComponentBase
    {
        public string Value { get; set; }
    }


    public enum Direction { 
    
        none,
        increase,
        decrease
    }

}
