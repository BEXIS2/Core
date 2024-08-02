namespace Vaiona.Entities.Common
{
    public interface IStatefullEntity
    {
        EntityStateInfo StateInfo { get; set; }
    }
}