using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.DCM.Transform.Validation.Exceptions;

namespace BExIS.DCM.Transform.Validation
{
    public interface IDataStructureValidation
    {
        DsType AppliedTo { get; }
        //Error Execute(int id);

    }

    public enum DsType
    {
        Dataset,
        Datastructure
    }
}
