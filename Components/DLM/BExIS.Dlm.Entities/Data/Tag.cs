using System;


namespace BExIS.Dlm.Entities.Data
{
    public enum TagType
    { 
        None = 0,
        Major = 1,
        Minor = 2
    }
    public class Tag
    {
        public virtual long Id { get; set; }
        public virtual double Nr { get; set; }
        public virtual DateTime ReleaseDate { get; set; }

        public virtual bool Final { get; set; }
        public virtual bool Show { get; set; }
        public virtual TagType Type { get; set; }

        public Tag()
        {
            this.Final = false;
            this.Show = false;
            this.Nr = 0.0;
            this.Type = TagType.None;
        }
    }
}
