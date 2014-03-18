using BExIS.Io.Transform.Validation.Exceptions;

namespace BExIS.Io.Transform.Validation
{
    public interface IValueValidation
    {
        ValueType AppliedTo  { get;}
        string Name { get; }
        string DataType { get; }
        Error Execute(object value, int row);
             
    }

    
    public enum ValueType
    { 
        Number,
        Date,
        String,
        All,
    }
}
