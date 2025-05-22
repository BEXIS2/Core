using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.IO.DataType
{
    public class DataTypeHelper
    {
      

        public static TypeCode GetMaxTypeCode(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Int16: return TypeCode.Int64;
                case TypeCode.Int32: return TypeCode.Int64;
                case TypeCode.Int64: return TypeCode.Int64;
                case TypeCode.UInt16: return TypeCode.Int64;
                case TypeCode.UInt32: return TypeCode.Int64;
                case TypeCode.UInt64: return TypeCode.Int64;
                case TypeCode.Double: return TypeCode.Double;
                case TypeCode.Decimal: return TypeCode.Double;
                default:
                    return code;
            }
        }

        public static string GetLabel(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Int16: return "Integer";
                case TypeCode.Int32: return "Integer";
                case TypeCode.Int64: return "Integer";
                case TypeCode.UInt16: return "Integer";
                case TypeCode.UInt32: return "Integer";
                case TypeCode.UInt64: return "Integer";
                case TypeCode.Double: return "Floating Point Number";
                case TypeCode.Decimal: return "Floating Point Number";
                case TypeCode.DateTime: return "Date and Time";
                case TypeCode.Boolean: return "Boolean";
                case TypeCode.String: return "Text";
                default:
                    return "Text";
            }
        }

    }
}
