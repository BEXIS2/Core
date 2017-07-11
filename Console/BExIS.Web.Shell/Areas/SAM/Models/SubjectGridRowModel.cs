using BExIS.Security.Entities.Subjects;

namespace BExIS.Modules.Sam.UI.Models
{
    public class SubjectGridRowModel
    {
        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public static SubjectGridRowModel Convert(Subject subject)
        {
            return new SubjectGridRowModel()
            {
                SubjectName = subject.Name,
                SubjectId = subject.Id,
                SubjectType = subject.GetType().ToString()
            };
        }
    }
}