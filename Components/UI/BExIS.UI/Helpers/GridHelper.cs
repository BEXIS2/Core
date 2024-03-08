using BExIS.Dlm.Entities.DataStructure;
using BExIS.UI.Models;
using BExIS.Utils.NH.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Web.Mvc;
using DataType = BExIS.Utils.NH.Querying.DataType;

namespace BExIS.UI.Helpers
{
    public class DataTableHelper
    {
        #region filter

        // currently the primary data is loaded from the matView, for whatever reason the variables are
        // defined with id as column name and not with the direct name of the columns.
        // Therefore, there must be a transformation so that the query matches the columns.
        // e.g.ScientificName - var61, varDic[ScientificName] = var61
        public static FilterExpression ConvertTo(List<DataTableFilter> filters, Dictionary<string, string> varDic)
        {
            FilterExpression filter = null;
            FilterExpression tmpFilter = null;

            foreach (var f in filters)
            {
                FilterExpression fe = convert(f, varDic[f.Column]);
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

            return filter;
        }

        /// <summary>
        /// Convert a FilterDesriptor to a FilterExpression
        /// </summary>
        /// <param name="filterDescriptor"></param>
        /// <returns></returns>
        private static FilterExpression convert(DataTableFilter filterDescriptor, string label)
        {
            var fd = filterDescriptor;


            switch (fd.FilterBy)
            {
                case DataTableFilterType.c:
                    {
                        return new FilterStringItemExpression()
                        {
                            Field = new Field() { DataType = DataType.String, Name = label }
                           ,
                            Operator = StringOperator.Operation.Contains
                           ,
                            Value = fd.Value
                        };
                    }
                case DataTableFilterType.nc:
                    {
                        return null;
                    }

                case DataTableFilterType.ew:
                    {
                        return new FilterStringItemExpression()
                        {
                            Field = new Field() { DataType = DataType.String, Name = label }
                            ,
                            Operator = StringOperator.Operation.EndsWith
                            ,
                            Value = fd.Value
                        };
                    }
                case DataTableFilterType.sw:
                    {
                        return new FilterStringItemExpression()
                        {
                            Field = new Field() { DataType = DataType.String, Name = label }
                            ,
                            Operator = StringOperator.Operation.StartsWith
                            ,
                            Value = fd.Value
                        };
                    }
                case DataTableFilterType.ie: // can be a string or a number
                    {
                        int convertedInt;
                        double convertedDouble;
                        DateTime convertedDateTime;
                        if (int.TryParse(fd.Value,out convertedInt))
                        {
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = DataType.Ineteger, Name = label },
                                Operator = NumberOperator.Operation.Equals,
                                Value = fd.Value
                            };
                        }
                        else
                        if (double.TryParse(fd.Value, out convertedDouble))
                        {
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = DataType.Double, Name = label },
                                Operator = NumberOperator.Operation.Equals,
                                Value = fd.Value
                            };
                        }
                        else
                        if (DateTime.TryParse(fd.Value,out convertedDateTime))
                        {
                            return new FilterDateTimeItemExpression()
                            {
                                Field = new Field() { DataType = DataType.DateTime, Name = label },
                                Operator = DateTimeOperator.Operation.Equals,
                                Value = fd.Value
                            };
                        }

                        return new FilterStringItemExpression()
                        {
                            Field = new Field() { DataType = DataType.String, Name = label },
                            Operator = StringOperator.Operation.Equals,
                            Value = fd.Value

                        };
                    }
                case DataTableFilterType.ne:
                    {
                        return null;
                    }
                case DataTableFilterType.gt:
                    {
                        try
                        {
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = DataType.Ineteger, Name = label }
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

                case DataTableFilterType.gte:
                    {
                        try
                        {
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = DataType.Ineteger, Name = label }
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
                case DataTableFilterType.lt:
                    {
                        try
                        {
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = DataType.Ineteger, Name = label }
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

                case DataTableFilterType.lte:
                    {
                        try
                        {
                            return new FilterNumberItemExpression()
                            {
                                Field = new Field() { DataType = DataType.Ineteger, Name = label }
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
        #endregion

        #region orderby

        // currently the primary data is loaded from the matView, for whatever reason the variables are
        // defined with id as column name and not with the direct name of the columns.
        // Therefore, there must be a transformation so that the query matches the columns.
        // e.g.ScientificName - var61, varDic[ScientificName] = var61
        public static OrderByExpression ConvertTo(List<DataTableOrderBy> sortDescriptors, Dictionary<string, string> varDic)
        {
            if (sortDescriptors == null || sortDescriptors.Count == 0) return null;

            List<OrderItemExpression> oieList = new List<OrderItemExpression>();
            foreach (var sort in sortDescriptors)
            {
                string name = varDic[sort.Column];
                SortDirection sortDirection = SortDirection.Ascending;

                if (sort.Direction == DataTableOrderType.asc) sortDirection = SortDirection.Ascending;
                if (sort.Direction == DataTableOrderType.desc) sortDirection = SortDirection.Descending;

                oieList.Add(new OrderItemExpression(name, sortDirection));
            }

            return new OrderByExpression(oieList);
        }

        #endregion

        public static Dictionary<string, string> variablesAsKVP(ICollection<VariableInstance> variables)
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();

            foreach (var item in variables)
            {
                tmp.Add(item.Label, "var"+item.Id);
            }

            return tmp;
        }

    }
}
