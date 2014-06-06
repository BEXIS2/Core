using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.Data;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DDM.Helpers
{
    public static class GridHelper
    {
        /*
         * Grid filter options
         * 
         * 1. IsEqualTo
         * 2. IsGreaterThan
         * 3. IsGreaterThanOrEqualTo
         * 4. IsLessThan
         * 5. IsLessThanOrEqualTo
         * 6. IsNotEqualTo
         * 
         * */
        public enum GridFilterOption
        {
            IsEqualTo,
            IsNotEqualTo,
            IsGreaterThan,
            IsGreaterThanOrEqualTo,
            IsLessThan,
            IsLessThanOrEqualTo
        }

        /*
         * Grid Sort Options
         * 
         * 1. Ascending
         * 2. Descending
         * 
         * */
        public enum GridOrderOption
        {
            Ascending,
            Descending
        }

        public static bool ValueComparion(VariableValue val, FilterOperator filterOperator, object value)
        {

            switch (filterOperator)
            {
                case FilterOperator.Contains:
                    {
                        if (val.Value.ToString().Contains(value.ToString()))
                            return true;
                        else
                            return false;
                    }

                case FilterOperator.IsContainedIn:
                    {
                        if (value.ToString().Contains(val.Value.ToString()))
                            return true;
                        else
                            return false;
                    }

                case FilterOperator.DoesNotContain:
                    {
                        if (!val.Value.ToString().Contains(value.ToString()))
                            return true;
                        else
                            return false;
                    }

                case FilterOperator.EndsWith:
                    {
                        if (val.Value.ToString().EndsWith(value.ToString()))
                            return true;
                        else
                            return false;
                    }
                case FilterOperator.StartsWith:
                    {
                        if (val.Value.ToString().StartsWith(value.ToString()))
                            return true;
                        else
                            return false;
                    }
                case FilterOperator.IsEqualTo:
                    {
                        if (val.Value.ToString().Equals(value.ToString()))
                            return true;
                        else
                            return false;
                    }
                case FilterOperator.IsNotEqualTo:
                    {
                        if (!val.Value.ToString().Equals(value.ToString()))
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

                            if (valueAsNumber > compareValueAsNumber)
                                return true;
                            else
                                return false;
                        }
                        catch(Exception e)
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

                            if (valueAsNumber >= compareValueAsNumber)
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

                            if (valueAsNumber < compareValueAsNumber)
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

                            if (valueAsNumber <= compareValueAsNumber)
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


    }
}