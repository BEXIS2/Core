using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DDM.Helpers
{
    public static class GridHelper
    {

        public static bool ValueComparion(VariableValue val, FilterOperator filterOperator, object value)
        {

            switch (filterOperator)
            {
                case FilterOperator.Contains:
                    {
                        if (val.Value.ToString().ToLower().Contains(value.ToString().ToLower()))
                            return true;
                        else
                            return false;
                    }

                case FilterOperator.IsContainedIn:
                    {
                        if (value.ToString().ToLower().Contains(val.Value.ToString().ToLower()))
                            return true;
                        else
                            return false;
                    }

                case FilterOperator.DoesNotContain:
                    {
                        if (!val.Value.ToString().ToLower().Contains(value.ToString().ToLower()))
                            return true;
                        else
                            return false;
                    }

                case FilterOperator.EndsWith:
                    {
                        if (val.Value.ToString().ToLower().EndsWith(value.ToString().ToLower()))
                            return true;
                        else
                            return false;
                    }
                case FilterOperator.StartsWith:
                    {
                        if (val.Value.ToString().ToLower().StartsWith(value.ToString().ToLower()))
                            return true;
                        else
                            return false;
                    }
                case FilterOperator.IsEqualTo:
                    {
                        if (val.Value.ToString().ToLower().Equals(value.ToString().ToLower()))
                            return true;
                        else
                            return false;
                    }
                case FilterOperator.IsNotEqualTo:
                    {
                        if (!val.Value.ToString().ToLower().Equals(value.ToString().ToLower()))
                            return true;
                        else
                            return false;
                    }
                case FilterOperator.IsGreaterThan:
                    {
                        try
                        {
                            double valueAsNumber = Convert.ToDouble(value);
                            double compareValueAsNumber = Convert.ToDouble(val.Value.ToString());

                            if (compareValueAsNumber > valueAsNumber)
                                return true;
                            else
                                return false;
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Value is not a number");
                        }


                    }

                case FilterOperator.IsGreaterThanOrEqualTo:
                    {
                        try
                        {
                            double valueAsNumber = Convert.ToDouble(value);
                            double compareValueAsNumber = Convert.ToDouble(val.Value.ToString());

                            if (compareValueAsNumber >= valueAsNumber)
                                return true;
                            else
                                return false;
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Value is not a number");
                        }
                    }
                case FilterOperator.IsLessThan:
                    {
                        try
                        {
                            double valueAsNumber = Convert.ToDouble(value);
                            double compareValueAsNumber = Convert.ToDouble(val.Value.ToString());

                            if (compareValueAsNumber < valueAsNumber)
                                return true;
                            else
                                return false;
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Value is not a number");
                        }
                    }

                case FilterOperator.IsLessThanOrEqualTo:
                    {
                        try
                        {
                            double valueAsNumber = Convert.ToDouble(value);
                            double compareValueAsNumber = Convert.ToDouble(val.Value.ToString());

                            if (compareValueAsNumber <= valueAsNumber)
                                return true;
                            else
                                return false;
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Value is not a number");
                        }
                    }


            }

            return false;
        }

        public static GridCommand ConvertToGridCommand(string filters, string orders)
        {

            GridCommand command = new GridCommand();

            if (!string.IsNullOrEmpty(filters))
            {
                filters = filters.Replace("~and", "");

                // add filters
                string[] filterSplit = filters.Split('~');

                FilterDescriptor filterDescriptor = new FilterDescriptor();

                for (int i = 0; i < filterSplit.Length; i += 3)
                {
                    filterDescriptor = new FilterDescriptor();
                    filterDescriptor.Member = filterSplit[i];
                    filterDescriptor.Operator = GetFilterOperator(filterSplit[i + 1]);
                    filterDescriptor.Value = filterSplit[i + 2].Replace("'","");

                    command.FilterDescriptors.Add(filterDescriptor);
                }
            }

            // add order
            if (!string.IsNullOrEmpty(orders))
            {
                orders = orders.Replace("~and", "");

                string[] orderSplit = orders.Split('~');
                SortDescriptor sortDescriptor = new SortDescriptor();


                for (int i = 0; i < orderSplit.Length; i += 2)
                {
                    sortDescriptor = new SortDescriptor();
                    sortDescriptor.Member = orderSplit[i];
                    sortDescriptor.SortDirection = GetSortDirection(orderSplit[i]);
                    command.SortDescriptors.Add(sortDescriptor);
                }
            }

            return command;
        }

        private static FilterOperator GetFilterOperator(string filter)
        {
            switch (filter)
            {
                case "eq": return FilterOperator.IsEqualTo;
                case "ne": return FilterOperator.IsNotEqualTo;
                case "gt": return FilterOperator.IsGreaterThan;
                case "ge": return FilterOperator.IsGreaterThanOrEqualTo;
                case "lt": return FilterOperator.IsLessThan;
                case "le": return FilterOperator.IsLessThanOrEqualTo;
                case "endswith": return FilterOperator.EndsWith;
                case "startswith": return FilterOperator.StartsWith;
                case "substringof": return FilterOperator.Contains;
                case "notsubstringof": return FilterOperator.Contains;

                default:    return FilterOperator.IsEqualTo;
            }
        }

        private static ListSortDirection GetSortDirection(string sortDirectionAbbr)
        {

            switch (sortDirectionAbbr)
            {
                case "asc": return ListSortDirection.Ascending;
                case "desc": return ListSortDirection.Descending;
                default: return ListSortDirection.Ascending;
            }

        }

        /// <summary>
        /// Cast a value to a systemtype
        /// </summary>
        /// <param name="value"></param>
        /// <param name="systemType"></param>
        /// <returns></returns>
        public static object CastVariableValue(object value, string systemType)
        {

            if (!String.IsNullOrEmpty(value.ToString()))
            {
                switch (systemType)
                {
                    case "Int16": return Convert.ToInt16(value);
                    case "Int32": return Convert.ToInt32(value);
                    case "Int64": return Convert.ToInt64(value);
                    case "Long": return Convert.ToInt64(value);
                    case "Double": return Convert.ToDouble(value);
                    case "DateTime": return DateTime.Parse(value.ToString(), new CultureInfo("en-US", false));
                    default: return value;
                }
            }
            else
            {
                switch (systemType)
                {
                    case "Int16": return Int16.MaxValue;
                    case "Int32": return Int32.MaxValue;
                    case "Int64": return Int64.MaxValue;
                    case "UInt16": return UInt16.MaxValue;
                    case "UInt32": return UInt32.MaxValue;
                    case "UInt64": return UInt64.MaxValue;
                    case "Long": return Int64.MaxValue;
                    case "Double": return Double.MaxValue;
                    case "DateTime": return DateTime.MaxValue;
                    default: return value;
                }
            }
        }

    }
}