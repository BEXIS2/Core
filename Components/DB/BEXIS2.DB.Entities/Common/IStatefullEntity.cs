namespace BEXIS2.DB.Entities.Common
{
    public interface IStatefullEntity
    {
        EntityStateInfo StateInfo { get; set; }
    }
}