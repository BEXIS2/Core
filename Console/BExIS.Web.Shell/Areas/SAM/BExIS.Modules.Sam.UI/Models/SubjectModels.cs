using BExIS.Security.Entities.Subjects;
using System.Collections.Generic;

namespace BExIS.Modules.Sam.UI.Models
{
    public class SelectableSubjectGridRowModel
    {
        public long Id { get; set; }
        public bool IsSelected { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public static SelectableSubjectGridRowModel Convert(Subject subject, List<long> subjectIds)
        {
            return new SelectableSubjectGridRowModel()
            {
                Id = subject.Id,
                Name = subject.Name,
                Type = subject.GetType().FullName,
                IsSelected = subjectIds.Contains(subject.Id)
            };
        }
    }
}