using BExIS.Security.Entities.Subjects;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class SubjectManager : IDisposable
    {
        private IUnitOfWork guow = null;
        private bool isDisposed = false;

        public SubjectManager()
        {
            var guow = this.GetIsolatedUnitOfWork();

            SubjectRepository = guow.GetReadOnlyRepository<Subject>();
        }

        ~SubjectManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Subject> SubjectRepository { get; }
        public IQueryable<Subject> Subjects => SubjectRepository.Query();

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (guow != null)
                        guow.Dispose();
                    isDisposed = true;
                }
            }
        }
    }
}