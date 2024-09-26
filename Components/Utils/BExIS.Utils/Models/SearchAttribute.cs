using System;

/// <summary>
///
/// </summary>
namespace BExIS.Utils.Models
{
    /// <summary>
    ///
    /// </summary>
    public enum AggregationType
    {
        None, Union, Distinct
    }

    /// <summary>
    ///
    /// </summary>
    public enum UiComponent
    {
        None, List, Item, Range
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class SearchAttribute
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string dateFormat = "bgc:format";

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public SearchAttribute()
        {
            displayName = "";
            sourceName = "";
            metadataName = "";

            searchType = SearchComponentBaseType.General;
            dataType = TypeCode.String;

            store = true;
            multiValue = false;
            analysed = true;
            norm = true;
            boost = 0.5;

            headerItem = false;
            defaultHeaderItem = false;

            direction = Direction.increase;
            uiComponent = UiComponent.None;
            aggregationType = AggregationType.None;

            placeholder = "";
        }

        /// <summary>
        /// Palceholder for card view
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string placeholder { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public AggregationType aggregationType { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public bool analysed { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public double boost { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public TypeCode dataType { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public bool defaultHeaderItem { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public Direction direction { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public String displayName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public bool headerItem { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public int id { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public String metadataName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public bool multiValue { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public bool norm { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public SearchComponentBaseType searchType { get; set; }

        //names
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public String sourceName { get; set; }

        //Type
        // parameter for index

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public bool store { get; set; }

        // ResultView
        // properties
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public UiComponent uiComponent { get; set; }

        #region reader helper

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AggregationType GetAggregationType(string value)
        {
            switch (value.ToLower())
            {
                case "distinct": return AggregationType.Distinct;
                case "union": return AggregationType.Union;
                default: return AggregationType.Distinct;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetBoolean(string value)
        {
            switch (value.ToLower())
            {
                case "yes": return true;
                case "no": return false; ;
                default: return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TypeCode GetDataType(String value)
        {
            switch (value.ToLower())
            {
                case "string": return TypeCode.String;
                case "double": return TypeCode.Double;
                case "datetime": return TypeCode.DateTime;
                case "date": return TypeCode.DateTime;
                case "integer": return TypeCode.Int32;
                default: return TypeCode.String;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Direction GetDirection(string value)
        {
            switch (value.ToLower())
            {
                case "increase": return Direction.increase;
                case "decrease": return Direction.decrease; ;
                default: return Direction.increase;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SearchComponentBaseType GetSearchType(String value)
        {
            switch (value.ToLower())
            {
                case "category_field": return SearchComponentBaseType.Category;
                case "category": return SearchComponentBaseType.Category;

                case "facet_field": return SearchComponentBaseType.Facet;
                case "facet": return SearchComponentBaseType.Facet;

                case "property_field": return SearchComponentBaseType.Property;
                case "property": return SearchComponentBaseType.Property;

                case "general_field": return SearchComponentBaseType.General;
                case "general": return SearchComponentBaseType.General;

                default: return SearchComponentBaseType.General;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UiComponent GetUIComponent(string value)
        {
            switch (value.ToLower())
            {
                case "item": return UiComponent.Item;
                case "list": return UiComponent.List;
                case "range": return UiComponent.Range;
                default: return UiComponent.Item;
            }
        }

        #endregion reader helper

        #region writer helper

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetAggregationTypeAsString(AggregationType value)
        {
            switch (value)
            {
                case AggregationType.Distinct: return "distinct";
                case AggregationType.Union: return "union";
                default: return "distinct";
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetBooleanAsString(bool value)
        {
            switch (value)
            {
                case true: return "yes";
                case false: return "no";
                default: return "no";
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDataTypeAsDisplayString(TypeCode value)
        {
            switch (value)
            {
                case TypeCode.String: return "String";
                case TypeCode.Double: return "Double";
                case TypeCode.DateTime: return "DateTime";
                case TypeCode.Int32: return "Integer";
                default: return "String";
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDataTypeAsString(TypeCode value)
        {
            switch (value)
            {
                case TypeCode.String: return "string";
                case TypeCode.Double: return "double";
                case TypeCode.DateTime: return "date";
                case TypeCode.Int32: return "integer";
                default: return "string";
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDirectionAsString(Direction value)
        {
            switch (value)
            {
                case Direction.increase: return "increase";
                case Direction.decrease: return "decrease";
                default: return "increase";
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetSearchTypeAsDisplayString(SearchComponentBaseType value)
        {
            switch (value)
            {
                case SearchComponentBaseType.Category: return "Category";
                case SearchComponentBaseType.Facet: return "Facet";
                case SearchComponentBaseType.Property: return "Property";
                case SearchComponentBaseType.General: return "General";
                default: return "General";
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetSearchTypeAsString(SearchComponentBaseType value)
        {
            switch (value)
            {
                case SearchComponentBaseType.Category: return "category_field";
                case SearchComponentBaseType.Facet: return "facet_field";
                case SearchComponentBaseType.Property: return "property_field";
                case SearchComponentBaseType.General: return "general_field";
                default: return "general_field";
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetUIComponentAsString(UiComponent value)
        {
            switch (value)
            {
                case UiComponent.Item: return "item";
                case UiComponent.List: return "list";
                case UiComponent.Range: return "range";
                default: return "item";
            }
        }

        #endregion writer helper
    }
}