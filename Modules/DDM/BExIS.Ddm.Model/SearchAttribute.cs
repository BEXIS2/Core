using System;



namespace BExIS.Ddm.Model
{
    
    public class SearchAttribute
    {
        public int id { get; set; }

        //names
        public String displayName { get; set; }

        public String sourceName { get; set; }

        public String metadataName { get; set; }

        //Type
        public SearchComponentBaseType searchType { get; set; }

        public TypeCode dataType { get; set; }

        // parameter for index
        public bool store { get; set; }

        public bool multiValue { get; set; }

        public bool analysed { get; set; }

        public bool norm { get; set; }

        public double boost { get; set; }

        // ResultView
        public bool headerItem { get; set; }

        public bool defaultHeaderItem { get; set; }

        // properties
        public Direction direction { get; set; }

        public UiComponent uiComponent { get; set; }

        public AggregationType aggregationType { get; set; }

        public string dateFormat = "bgc:format";

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

        }

        #region reader helper
        public static SearchComponentBaseType GetSearchType(String value)
        {
            switch (value)
            {
                case "category_field": return SearchComponentBaseType.Category;
                case "Category": return SearchComponentBaseType.Category;

                case "facet_field": return SearchComponentBaseType.Facet;
                case "Facet": return SearchComponentBaseType.Facet;

                case "property_field": return SearchComponentBaseType.Property;
                case "Property": return SearchComponentBaseType.Property;

                case "general_field": return SearchComponentBaseType.General;
                case "General": return SearchComponentBaseType.General;

                default: return SearchComponentBaseType.General;
            }
        }

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

        public static Direction GetDirection(string value)
        {
            switch (value.ToLower())
            {
                case "increase": return Direction.increase;
                case "decrease": return Direction.decrease; ;
                default: return Direction.increase;
            }
        }

        public static bool GetBoolean(string value)
        {
            switch (value.ToLower())
            {
                case "yes": return true;
                case "no": return false; ;
                default: return false;
            }
        }

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

        public static AggregationType GetAggregationType(string value)
        {
            switch (value)
            {
                case "distinct": return AggregationType.Distinct;
                case "union": return AggregationType.Union;
                default: return AggregationType.Distinct;
            }
        }
        #endregion

        #region writer helper

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

        public static string GetBooleanAsString(bool value)
        {
            switch (value)
            {
                case true: return "yes";
                case false: return "no";
                default: return "no";
            }
        }

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

        public static string GetDirectionAsString(Direction value)
        {
            switch (value)
            {
                case Direction.increase: return "increase";
                case Direction.decrease: return "decrease";
                default: return "increase";
            }
        }

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

        public static string GetAggregationTypeAsString(AggregationType value)
        {
            switch (value)
            {
                case AggregationType.Distinct: return "distinct";
                case AggregationType.Union: return "union";
                default: return "distinct";
            }
        }

        #endregion
    }

    public enum UiComponent
    { 
        None, List,Item,Range
    }

    public enum AggregationType
    {
        None, Union, Distinct
    }

    

}
