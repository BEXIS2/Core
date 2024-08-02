using System.Collections.Generic;

namespace BExIS.Dlm.Services.TypeSystem
{
    /// <summary>
    /// The class holds information about all supported data types specific to the application domain, as well as any given data type.
    /// </summary>
    public class DataTypeInfo
    {
        private static List<DataTypeInfo> typeInformation = new List<DataTypeInfo>()
        {
            new DataTypeInfo() {Code = DataTypeCode.Boolean,    DisplayName = "Boolean",                    Description = "A true or false value."},
            new DataTypeInfo() {Code = DataTypeCode.Char,       DisplayName = "Character",                  Description = "A single unicode charachter like \"a\", \"-\", or a character representation of a digit i.e., \"2\"."},
            new DataTypeInfo() {Code = DataTypeCode.String,     DisplayName = "String",                     Description = "Any combination of letters, digits and punctuation."},
            new DataTypeInfo() {Code = DataTypeCode.Int16,      DisplayName = "16 bits Integer",            Description = "Any integer number between -32,768 and 32,767."},
            new DataTypeInfo() {Code = DataTypeCode.Int32,      DisplayName = "32 bits Integer",            Description = "Any integer number between -2,147,483,648 and 2,147,483,647."},
            new DataTypeInfo() {Code = DataTypeCode.Int64,      DisplayName = "64 bits Integer",            Description = "Any integer number between -9,223,372,036,854,775,808 and 9,223,372,036,854,775,807."},
            new DataTypeInfo() {Code = DataTypeCode.UInt16,     DisplayName = "16 bits Unsigned Integer",   Description = "Any (positive) integer number between 0 and 65535."},
            new DataTypeInfo() {Code = DataTypeCode.UInt32,     DisplayName = "32 bits Unsigned Integer",   Description = "Any (positive) integer number between  0 and 4,294,967,295."},
            new DataTypeInfo() {Code = DataTypeCode.UInt64,     DisplayName = "64 bits Unsigned Integer",   Description = "Any (positive) integer number between 0 and 18,446,744,073,709,551,615."},
            new DataTypeInfo() {Code = DataTypeCode.Double,     DisplayName = "Double",                     Description = "Any double-precision floating point number between -1.79769313486232E308 and 1.79769313486232E308."},
            new DataTypeInfo() {Code = DataTypeCode.Decimal,    DisplayName = "Decimal",                    Description = "Any precise fractional or integral value that can represent decimal numbers with 29 significant digits from ±1.0 × 10E−28 to ±7.9 × 10E28."},
            new DataTypeInfo() {Code = DataTypeCode.DateTime,   DisplayName = "DateTime",                   Description = "An instant in time, typically expressed as a date and time of day e.g., 28/01/2015 15:53 PM."},
        };

        public DataTypeCode Code { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// use this property in the form of DataTypeInfo.Types to access all the types and filter them using LINQ if required
        /// </summary>
        public static List<DataTypeInfo> Types
        { get { return typeInformation; } }
    }
}