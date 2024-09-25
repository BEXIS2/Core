using BExIS.IO.Transform.Validation.Exceptions;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Validation.DSValidation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class DatastructureMatchCheck : IDataStructureValidation
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        private DataEntityType appliedTo = new DataEntityType();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public DataEntityType AppliedTo
        {
            get { return appliedTo; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="checkList"></param>
        /// <param name="sourceList"></param>
        /// <param name="sourceListName"></param>
        /// <returns></returns>
        public List<Error> Execute(List<VariableIdentifier> checkList, List<VariableIdentifier> sourceList, string sourceListName)
        {
            List<Error> errors = new List<Error>();
            List<string> errorVariables = new List<string>();

            // check length
            if (checkList.Count.Equals(sourceList.Count))
            {
                foreach (VariableIdentifier o in checkList)
                {
                    var test = false;
                    if (o.id == 0)
                        test = sourceList.Select(p => p.name.Equals(o.name)).Contains(true);
                    else
                        test = sourceList.Select(p => p.id.Equals(o.id)).Contains(true);

                    if (!test)
                        errorVariables.Add(o.name);
                }
                if (errorVariables.Count > 0)
                {
                    errors.Add(new Error(ErrorType.Datastructure, "A variable of the data file does not exist in the data structure. Check your variable names in your file and data structure. Names need to be identical.", string.Join(",", errorVariables)));
                }
            }
            else
            {
                errors.Add(new Error(ErrorType.Datastructure, "Data structure does not fit. The number of variables in the file ( "+sourceList.Count +" ) does not match with the defined number in the data structure ( " + checkList.Count + " ).", sourceListName));
                return errors;
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public DatastructureMatchCheck()
        {
            appliedTo = DataEntityType.Datastructure;
        }
    }
}