using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.DCM.Transform.Validation.DSValidation
{
    public class VariableIdentifier
    {
        public string name = "";
        public long id = 0;

        public bool Equals(VariableIdentifier check)
        {
            if (check != null)
            {
                // name and id exist
                if (!string.IsNullOrEmpty(check.name) && (check.id != null && check.id != 0))
                {
                    if (check.name.Equals(name) && check.id.Equals(id)) return true;
                    else return false;
                }
                else
                { 
                    // name is empty
                    if (string.IsNullOrEmpty(check.name) && (check.id != null || check.id != 0))
                    {
                        if (check.id.Equals(id)) return true;
                        else return false;
                    }
                    else
                    {
                        //id is empty
                        if (!string.IsNullOrEmpty(check.name) && (check.id == null || check.id == 0))
                        {
                            if (check.name.Equals(name)) return true;
                            else return false;
                        }
                    }
                    return false;
                }
            }
            else return false;

        }
    }
}
