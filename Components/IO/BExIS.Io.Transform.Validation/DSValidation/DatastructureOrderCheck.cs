using System.Collections.Generic;
using System.Linq;
using BExIS.IO.Transform.Validation.Exceptions;

/// <summary>
///
/// </summary>        
namespace BExIS.IO.Transform.Validation.DSValidation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class DatastructureOrderCheck : IDataStructureValidation
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

            // check length
            if (checkList.Count.Equals(sourceList.Count))
            {
                for(int i = 0; i < checkList.Count; i++)
                {
                    var incoming = checkList.ElementAt(i);
                    var source = sourceList.ElementAt(i);

                    var test = false;
                    if(incoming.id==0)
                        test = incoming.name.Equals(source.name);
                    else
                        test = incoming.id.Equals(source.id);

                    if (!test) 
                        errors.Add(new Error(ErrorType.Datastructure, "Data structure does not have the same order. the variable from the incoming structure does not match the structure in the system", incoming.name));

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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public DatastructureOrderCheck()
        {
            appliedTo = DataEntityType.Datastructure;
        }
    }
}
