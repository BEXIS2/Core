namespace BExIS.Modules.Rpm.UI.Models.DataTypes
{
    public class DataTypeListItem
    {
        /// <summary>
        /// Name of the Entity Template
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name of the Entity Template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the Entity Template
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// SystemType
        /// </summary>
        public string SystemType { get; set; }

        public bool InUse { get; set; }

        public DataTypeListItem()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            SystemType = string.Empty;
            InUse = false;
        }
    }
}