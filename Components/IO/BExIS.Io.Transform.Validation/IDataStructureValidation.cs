
/// <summary>
///
/// </summary>        
namespace BExIS.Io.Transform.Validation
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public interface IDataStructureValidation
    {
        DsType AppliedTo { get; }
        //Error Execute(int id);

    }

    /// <summary>
    /// 
    /// </summary>
    public enum DsType
    {
        Dataset,
        Datastructure
    }
}
