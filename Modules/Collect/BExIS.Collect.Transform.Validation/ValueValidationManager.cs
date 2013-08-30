using System;
using System.Collections.Generic;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.DCM.Transform.Validation.ValueCheck;

namespace BExIS.DCM.Transform.Validation
{
    public class ValueValidationManager
    {

        private string _name = "";
        private string _dataType = "";
        private bool _optional = false;
        
        public string Value { get; set; }
        public string DateFormat { get; set; }
        public Object ConvertedValue { get; set; }
        //public List<IValueCheck> CheckList { get; set; }
        public List<IValueValidation> ValidationList { get; set; }

        public OptionalCheck NullOrOptionalCheck;
        public DataTypeCheck DataTypeCheck;
       

        public List<Error> CheckValue(string value, int row)
        {
            List<Error> errors = new List<Error>();

            if (this.NullOrOptionalCheck != null)
            {
                object temp = this.NullOrOptionalCheck.Execute(value, row);

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

            return errors;
        }

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

        public ValueValidationManager(string name, string dataType, bool optional)
        {
            _name = name;
            _dataType = dataType;
            _optional = optional;
            ValidationList = new List<IValueValidation>();
            this.NullOrOptionalCheck = new OptionalCheck(name, dataType, optional);
            this.DataTypeCheck = new DataTypeCheck(name, dataType);
        }
    }
}
