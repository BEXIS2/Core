using System;

namespace BExIS.Dlm.Services.TypeSystem
{
    public class DataTypeUtility
    {
        public static bool IsTypeOf(object data, string systemType)
        {
            DataTypeCode colType = (DataTypeCode)Enum.Parse(typeof(DataTypeCode), systemType);

            Type type = Type.GetType("System." + colType);

            try
            {
                Convert.ChangeType(data, type);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static DataTypeCode GetTypeCode(string type)
        {
            return (DataTypeCode)Enum.Parse(typeof(DataTypeCode), type);
        }
    }
}