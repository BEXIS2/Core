using System.Collections.Generic;
using System.Linq;
using BExIS.Io.Transform.Validation.Exceptions;


namespace BExIS.Io.Transform.Validation.DSValidation
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
                    var test = false;
                    if(o.id==0)
                        test = sourceList.Select(p => p.name.Equals(o.name)).Contains(true);
                    else
                        test = sourceList.Select(p => p.id.Equals(o.id)).Contains(true);

                    if (!test) errors.Add(new Error(ErrorType.Datastructure, "A variable of the data file does not exist in the data structure", o.name));

                }
            }
            else
            {
                errors.Add(new Error(ErrorType.Datastructure, "Datastructure does not fit. The number of variables in file do not match", sourceListName));
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
