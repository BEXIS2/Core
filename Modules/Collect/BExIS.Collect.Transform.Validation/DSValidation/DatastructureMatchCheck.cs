using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.DCM.Transform.Validation.Exceptions;


namespace BExIS.DCM.Transform.Validation.DSValidation
{
    public class DatastructureMatchCheck : IDataStructureValidation
    {
        private DsType _appliedTo = new DsType();

        public DsType AppliedTo
        {
            get { return _appliedTo; }
        }

        public List<Error> Execute(List<VariableIdentifier> checkList, List<VariableIdentifier> sourceList, string sourceListName)
        {
            List<Error> errors = new List<Error>();

            // check length
            if (checkList.Count.Equals(sourceList.Count))
            {
                foreach (VariableIdentifier o in checkList)
                {
                    var test = sourceList.Select(p => p.Equals(o)).Contains(true);
                    if (!test) errors.Add(new Error(ErrorType.Datastructure, "Variable from file not exist in datastructure", o.name));

                }
            }
            else
            {
                errors.Add(new Error(ErrorType.Datastructure, "Different number of variables", sourceListName));
                return errors;
            }

            if (errors.Count > 0)
            {
                return errors;
            }
            return null;
        }

        public DatastructureMatchCheck()
        {
            _appliedTo = DsType.Datastructure;
        }
    }
}
