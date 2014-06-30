using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class SubjectModel
    {
    }

    public class SubjectItemModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public static SubjectItemModel Convert(Subject subject)
        {
            return new SubjectItemModel()
            {
                Id = subject.Id,
                Name = subject is User ? subject.Name + " (User)" : subject.Name +  " (Role)"
            };
        }
    }
}