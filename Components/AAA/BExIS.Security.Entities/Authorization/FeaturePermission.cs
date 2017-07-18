using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Entities.Authorization
{
    public class FeaturePermission : Permission
    {
        public virtual Feature Feature { get; set; }
    }
}