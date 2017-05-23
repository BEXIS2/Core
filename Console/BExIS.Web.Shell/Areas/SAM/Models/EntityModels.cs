namespace BExIS.Modules.Sam.UI.Models
{
    public class CreateEntityModel
    {
        public string AssemblyPath { get; set; }
        public string ClassPath { get; set; }
        public string Name { get; set; }
    }

    public class ReadEntityModel
    {
    }

    public class UpdateEntityModel
    {
    }

    public class DeleteEntityModel
    {
    }

    public class EntityGridRowModel
    {
        public string AssemblyPath { get; set; }
        public string ClassPath { get; set; }
        public string Name { get; set; }
    }
}