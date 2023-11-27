using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace BExIS.UI.Helpers
{
    public static class BexisDataHelper
    {
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }

            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }

            if (table.Columns["Id"] != null)
            {
                table.Columns["Id"].SetOrdinal(0);
            }

            return table;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IList<T> data, List<string> columns)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (string column in columns)
            {
                PropertyDescriptor prop = props.Find(column, false);
                if (prop != null)
                    table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[table.Columns.Count];
            foreach (T item in data)
            {
                foreach (PropertyDescriptor prop in props)
                {
                    if (columns.Contains(prop.Name))
                    {
                        int columnIndex = table.Columns.IndexOf(table.Columns[prop.Name]);
                        values[columnIndex] = prop.GetValue(item);
                    }
                }

                table.Rows.Add(values);
            }
            return table;
        }
    }
}