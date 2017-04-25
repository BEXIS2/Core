using BExIS.Security.Entities.Subjects;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class SubjectManager
    {
        public SubjectManager()
        {
            var uow = this.GetUnitOfWork();

            SubjectRepository = uow.GetReadOnlyRepository<Subject>();
        }

        public IReadOnlyRepository<Subject> SubjectRepository { get; }
        public IQueryable<Subject> Subjects => SubjectRepository.Query();
    }
}