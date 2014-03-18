
namespace BExIS.Io.Transform.Validation
{
    public interface IValueCheck
    {
        ValueType AppliedTo { get; }
        string Name { get; }
        string DataType { get; }
        object Execute(string value, int row);
    }

}
