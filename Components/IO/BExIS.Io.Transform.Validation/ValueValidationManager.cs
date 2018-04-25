using System;
using System.Collections.Generic;
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
                if(e != null)errors.Add(e); 
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
        public ValueValidationManager(string name, string dataType, bool optional, DecimalCharacter decimalCharacter, string pattern="")
        {
            _name = name;
            _dataType = dataType;
            _optional = optional;
            ValidationList = new List<IValueValidation>();
            this.NullOrOptionalCheck = new OptionalCheck(name, dataType, optional);
            this.DataTypeCheck = new DataTypeCheck(name, dataType, decimalCharacter,pattern);
        }
    }
}
