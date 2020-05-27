﻿using System;
using System.Collections.Generic;
using System.Globalization;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.IO.Transform.Validation.ValueCheck;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Validation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class ValueValidationManager
    {
        private string _name = "";
        private string _dataType = "";
        private bool _optional = false;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string Value { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public string DateFormat { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public Object ConvertedValue { get; set; }

        //public List<IValueCheck> CheckList { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public List<IValueValidation> ValidationList { get; set; }

        public OptionalCheck NullOrOptionalCheck;
        public DataTypeCheck DataTypeCheck;
        public MissingValueCheck MissingValueCheck;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public object CheckValue(string value, int row, ref List<Error> errors)
        {
            object temp = value;

            if (this.NullOrOptionalCheck != null)
            {
                temp = this.NullOrOptionalCheck.Execute(value, row);

                if (temp is Error)
                {
                    errors.Add((Error)temp);
                }
                else
                {
                    if (this.DataTypeCheck != null)
                    {
                        temp = this.DataTypeCheck.Execute(value, row);
                    }

                    if (temp is Error) errors.Add((Error)temp);
                    else ConvertedValue = temp;
                }
            }

            return temp;
        }

        public bool ValueIsMissingValue(string value, int row)
        {
            if (this.MissingValueCheck != null)
            {
                var tmp = MissingValueCheck.Execute(value, row);
                if (tmp == null) return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the value is a missing value and out the placeholder
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool ValueIsMissingValueGetPlaceHolder(string value, int row, out string placeholder)
        {
            placeholder = value;

            if (this.MissingValueCheck != null)
            {
                var tmp = MissingValueCheck.Execute(value, row);
                if (tmp == null) return false;

                placeholder = tmp.ToString();
            }

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public List<Error> ValidateValue(object value, int row)
        {
            List<Error> errors = new List<Error>();
            foreach (IValueValidation vv in ValidationList)
            {
                Error e = vv.Execute(value, row);
                if (e != null) errors.Add(e);
            }

            return errors;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="optional"></param>
        /// <param name="decimalCharacter"></param>
        /// <param name="pattern"></param>
        public ValueValidationManager(string name, string dataType, bool optional, DecimalCharacter decimalCharacter, string pattern = "", IEnumerable<MissingValue> missingValues = null, CultureInfo cultureInfo = null)
        {
            _name = name;
            _dataType = dataType;
            _optional = optional;
            ValidationList = new List<IValueValidation>();
            NullOrOptionalCheck = new OptionalCheck(name, dataType, optional);
            DataTypeCheck = new DataTypeCheck(name, dataType, decimalCharacter, pattern, cultureInfo);
            MissingValueCheck = new MissingValueCheck(name, dataType, missingValues);
        }
    }
}