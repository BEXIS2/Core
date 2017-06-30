namespace BExIS.Security.Entities.Objects
{
    public class Operation
    {
        public virtual int Position { get; set; }
        public virtual string Module { get; set; }
        public virtual string Controller { get; set; }
        public virtual string Action { get; set; }
    }
}