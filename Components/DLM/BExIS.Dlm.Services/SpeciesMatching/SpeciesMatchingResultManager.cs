using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.SpeciesMatching;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.SpeciesMatching
{
    public class SpeciesMatchingResultManager : IDisposable
    {

        private IUnitOfWork guow = null;

        public SpeciesMatchingResultManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.Repo = guow.GetReadOnlyRepository<SpeciesMatchingResult>();
        }

        private bool isDisposed = false;

        ~SpeciesMatchingResultManager()
        {
            Dispose(true);
        }

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

        public IReadOnlyRepository<SpeciesMatchingResult> Repo { get; private set; }

        public SpeciesMatchingResult Create(SpeciesMatchingResult matchingResult)
        {
            if (matchingResult == null) throw new ArgumentNullException("Species matching result must not be null.");
            if (matchingResult.Creator == null) throw new ArgumentNullException("Creator type must not be null.");
            if (matchingResult.Dataset == null) throw new ArgumentNullException("Dataset must not be null.");
            if (matchingResult.OriginalName == null) throw new ArgumentNullException("Dataset must not be null.");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                try
                {
                    IRepository<SpeciesMatchingResult> repo = uow.GetRepository<SpeciesMatchingResult>();
                    repo.Put(matchingResult);
                    uow.Commit();

                    return (matchingResult);
                }
                catch (Exception ex)
                {
                    throw new Exception("SpeciesMatchingResult creation failed.", ex);
                }
            }
        }

        public SpeciesMatchingResult Update(SpeciesMatchingResult matchingResult)
        {
            if (matchingResult == null) throw new ArgumentNullException("Species matching result must not be null.");

            Contract.Ensures(Contract.Result<SpeciesMatchingResult>() != null && Contract.Result<SpeciesMatchingResult>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                try
                {
                    IRepository<SpeciesMatchingResult> repo = uow.GetRepository<SpeciesMatchingResult>();
                    repo.Merge(matchingResult);
                    var merged = repo.Get(matchingResult.Id);
                    repo.Put(merged);
                    uow.Commit();

                    return (merged);
                }
                catch (Exception ex)
                {
                    throw new Exception("SpeciesMatchingResult creation failed.", ex);
                }
            }
        }
        public bool Delete(long id)
        {
            if (id == 0) throw new ArgumentException("Species matching result must not be null.");

            Contract.Ensures(Contract.Result<SpeciesMatchingResult>() != null && Contract.Result<SpeciesMatchingResult>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<SpeciesMatchingResult> repo = uow.GetRepository<SpeciesMatchingResult>();

                var e = repo.Get(id);

                if (e != null)
                {
                    repo.Delete(e);
                    uow.Commit();

                    return true;
                }
                else
                {
                    throw new ArgumentException(string.Format("the species matching result with the id {0} does not exist", id));
                }
            }
        }

        public bool Delete(SpeciesMatchingResult matchingResult)
        {
            if (matchingResult == null) throw new ArgumentNullException("Entity template must not be null.");

            Contract.Ensures(Contract.Result<SpeciesMatchingResult>() != null && Contract.Result<SpeciesMatchingResult>().Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<SpeciesMatchingResult> repo = uow.GetRepository<SpeciesMatchingResult>();

                repo.Delete(matchingResult);
                uow.Commit();

                return true;
            }
        }

    }
}
