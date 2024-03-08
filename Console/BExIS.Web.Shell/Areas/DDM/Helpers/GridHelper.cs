using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Utils.NH.Querying;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Infrastructure.Implementation;

namespace BExIS.Modules.Ddm.UI.Helpers
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
                            double valueAsNumber = System.Convert.ToDouble(value);
                            double compareValueAsNumber = System.Convert.ToDouble(val.Value.ToString());

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
                            double valueAsNumber = System.Convert.ToDouble(value);
                            double compareValueAsNumber = System.Convert.ToDouble(val.Value.ToString());

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
                            double valueAsNumber = System.Convert.ToDouble(value);
                            double compareValueAsNumber = System.Convert.ToDouble(val.Value.ToString());

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
                            double valueAsNumber = System.Convert.ToDouble(value);
                            double compareValueAsNumber = System.Convert.ToDouble(val.Value.ToString());

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
                    filterDescriptor.Value = filterSplit[i + 2].Replace("'", "");

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

                default: return FilterOperator.IsEqualTo;
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
                    case "Int16": return System.Convert.ToInt16(value);
                    case "Int32": return System.Convert.ToInt32(value);
                    case "Int64": return System.Convert.ToInt64(value);
                    case "Long": return System.Convert.ToInt64(value);
                    case "Double": return System.Convert.ToDouble(value);
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

        /// <summary>
        /// Convert a List of IFilterdesciptors to a FilterExpression
        /// </summary>
        /// <param name="filterDescriptors"></param>
        /// <returns></returns>
        public static FilterExpression Convert(List<IFilterDescriptor> filterDescriptors)
        {
            FilterExpression filter = null;
            FilterExpression tmpFilter = null;
            List<FilterExpression> tmpFilterExpressions = new List<FilterExpression>();

            foreach (var f in filterDescriptors)
            {
                if (f.GetType() == typeof(CompositeFilterDescriptor))
                {
                    CompositeFilterDescriptor compositeFilterDescriptor = (CompositeFilterDescriptor)f;
                    FilterExpression compositeFilter = null;

                    compositeFilter = Convert(compositeFilterDescriptor.FilterDescriptors);

                    if (compositeFilter != null)
                    {
                        if (filter != null)
                        {
                            tmpFilter = filter;
                            filter = BinaryFilterExpression.And(tmpFilter, compositeFilter);
                        }
                        else
                        {
                            filter = compositeFilter;
                        }
                    }
                }
                else
                if (f.GetType() == typeof(FilterDescriptor))
                {
                    FilterExpression fe = convert(f);
                    if (fe != null)
                    {
                        if (filter != null)
                        {
                            tmpFilter = filter;
                            filter = BinaryFilterExpression.And(tmpFilter, fe);
                        }
                        else
                        {
                            filter = fe;
                        }
                    }
                }
            }

            return filter;
        }

        /// <summary>
        /// Convert a FilterDescriptorCollection to FilterExpression
        /// </summary>
        /// <param name="filterDescriptors"></param>
        /// <returns></returns>
        public static FilterExpression Convert(FilterDescriptorCollection filterDescriptors)
        {
            FilterExpression filter = null;
            FilterExpression tmpFilter = null;
            List<FilterExpression> tmpFilterExpressions = new List<FilterExpression>();

            foreach (var f in filterDescriptors)
            {
                if (f.GetType() == typeof(CompositeFilterDescriptor))
                {
                    CompositeFilterDescriptor compositeFilterDescriptor = (CompositeFilterDescriptor)f;
                    FilterExpression compositeFilter = null;

                    compositeFilter = Convert(compositeFilterDescriptor.FilterDescriptors);

                    if (compositeFilter != null)
                    {
                        if (filter != null)
                        {
                            tmpFilter = filter;
                            filter = BinaryFilterExpression.And(tmpFilter, compositeFilter);
                        }
                        else
                        {
                            filter = compositeFilter;
                        }
                    }
                }
                else
                if (f.GetType() == typeof(FilterDescriptor))
                {
                    FilterExpression fe = convert(f);
                    if (fe != null)
                    {
                        if (filter != null)
                        {
                            tmpFilter = filter;
                            filter = BinaryFilterExpression.And(tmpFilter, fe);
                        }
                        else
                        {
                            filter = fe;
                        }
                    }
                }
            }

            return filter;
        }

        /// <summary>
        /// Convert a FilterDesriptor to a FilterExpression
        /// </summary>
        /// <param name="filterDescriptor"></param>
        /// <returns></returns>
        private static FilterExpression convert(IFilterDescriptor filterDescriptor)
        {
            FilterDescriptor fd = (FilterDescriptor)filterDescriptor;

            switch (fd.Operator)
            {
                case FilterOperator.Contains:
                    {
                        return new FilterStringItemExpression()
                        {
                            Field = new Field() { DataType = Utils.NH.Querying.DataType.String, Name = fd.Member }
                           ,
                            Operator = StringOperator.Operation.Contains
                           ,
                            Value = fd.Value
                        };
                    }

                case FilterOperator.IsContainedIn:
                    {
                        return new FilterStringItemExpression()
                        {
                            Field = new Field() { DataType = Utils.NH.Querying.DataType.String, Name = fd.Member }
                           ,
                            Operator = StringOperator.Operation.Contains
                           ,
                            Value = fd.Value
                        };
                    }

                case FilterOperator.DoesNotContain:
                    {
                        return null;
                    }

                case FilterOperator.EndsWith:
                    {
                        return new FilterStringItemExpression()
                        {
                            Field = new Field() { DataType = Utils.NH.Querying.DataType.String, Name = fd.Member }
                            ,
                            Operator = StringOperator.Operation.EndsWith
                            ,
                            Value = fd.Value
                        };
                    }
                case FilterOperator.StartsWith:
                    {
                        return new FilterStringItemExpression()
                        {
                            Field = new Field() { DataType = Utils.NH.Querying.DataType.String, Name = fd.Member }
                            ,
                            Operator = StringOperator.Operation.StartsWith
                            ,
                            Value = fd.Value
                        };
                    }
                case FilterOperator.IsEqualTo:
                    {
                        return new FilterStringItemExpression()
                        {
                            Field = new Field() { DataType = Utils.NH.Querying.DataType.String, Name = fd.Member }
                            ,
                            Operator = StringOperator.Operation.Equals
                            ,
                            Value = fd.Value
                        };
                    }
                case FilterOperator.IsNotEqualTo:
                    {
                        return null;
                    }
                case FilterOperator.IsGreaterThan:
                    {
                        try
                        {
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = Utils.NH.Querying.DataType.Ineteger, Name = fd.Member }
                                ,
                                Operator = NumberOperator.Operation.GreaterThan
                                ,
                                Value = fd.Value
                            };
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
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = Utils.NH.Querying.DataType.Ineteger, Name = fd.Member }
                                ,
                                Operator = NumberOperator.Operation.GreaterThanOrEqual
                                ,
                                Value = fd.Value
                            };
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
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = Utils.NH.Querying.DataType.Ineteger, Name = fd.Member }
                                ,
                                Operator = NumberOperator.Operation.LessThan
                                ,
                                Value = fd.Value
                            };
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
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = Utils.NH.Querying.DataType.Ineteger, Name = fd.Member }
                                ,
                                Operator = NumberOperator.Operation.LessThanOrEqual
                                ,
                                Value = fd.Value
                            };
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Value is not a number");
                        }
                    }
            }

            return null;
        }

        public static OrderByExpression Convert(List<SortDescriptor> sortDescriptors)
        {
            /*
             OrderByExpression orderByExpr = new OrderByExpression(
                                                    new List<OrderItemExpression>() {
                                                        new OrderItemExpression(var1Name),
                                                        new OrderItemExpression(var2Name, SortDirection.Descending)
                                                    });
             */

            List<OrderItemExpression> oieList = new List<OrderItemExpression>();
            if (sortDescriptors.Count > 0)
            {
                foreach (var sort in sortDescriptors)
                {
                    string name = sort.Member;
                    string direction = sort.SortDirection.ToString();
                    SortDirection sortDirection = SortDirection.Ascending;

                    if (direction.Equals("Ascending")) sortDirection = SortDirection.Ascending;
                    if (direction.Equals("Descending")) sortDirection = SortDirection.Descending;

                    oieList.Add(new OrderItemExpression(name, sortDirection));
                }
            }

            return new OrderByExpression(oieList);
        }

        /// <summary>
        /// Convert a list of column names to a Projection Expression
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static ProjectionExpression Convert(string[] columns)
        {
            List<ProjectionItemExpression> pieList = new List<ProjectionItemExpression>();

            foreach (var c in columns)
            {
                pieList.Add(new ProjectionItemExpression()
                {
                    FieldName = c
                });
            }

            return new ProjectionExpression(pieList);
        }


 
    }
}