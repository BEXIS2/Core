/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Validation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public interface IDataStructureValidation
    {
        DataEntityType AppliedTo { get; }
    }

    /// <summary>
    ///
    /// </summary>
    public enum DataEntityType
    {
        Dataset,
        Datastructure
    }
}