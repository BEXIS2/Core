using System;


namespace BExIS.Dlm.Entities.Data
{
    public class Tag
    {
        public virtual long Id { get; set; }
        public virtual double TagNr { get; set; }
        public virtual DateTime ReleaseDate { get; set; }
        public virtual DatasetVersion Version { get; set; }
        public virtual bool Final { get; set; }
        public virtual bool Show { get; set; }

        public Tag()
        {
            this.ReleaseDate = DateTime.Now;
            this.Final = false;
            this.Show = false;
            this.TagNr = 0.0;
        }
    }
}
