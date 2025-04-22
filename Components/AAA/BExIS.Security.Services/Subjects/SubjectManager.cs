using BExIS.Security.Entities.Subjects;
using BExIS.Utils.NH.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class SubjectManager : IDisposable
    {
        private readonly IUnitOfWork _guow = null;
        private bool isDisposed = false;

        public SubjectManager()
        {
            _guow = this.GetIsolatedUnitOfWork();

            SubjectRepository = _guow.GetReadOnlyRepository<Subject>();
        }

        ~SubjectManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Subject> SubjectRepository { get; }
        public IQueryable<Subject> Subjects => SubjectRepository.Query();

        /// <summary>
        /// returns subset of subjects based on the parameters
        /// and also count of filtered list
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<Subject> GetSubjects(FilterExpression filter, OrderByExpression orderBy, int pageNumber, int pageSize, out int count)
        {
            var orderbyClause = orderBy?.ToLINQ();
            var whereClause = filter?.ToLINQ();
            count = 0;
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    if (whereClause != null && orderBy != null)
                    {
                        var l = SubjectRepository.Query(whereClause);
                        var x = l.OrderBy(orderbyClause);
                        var y = x.Skip((pageNumber - 1) * pageSize);
                        var z = y.Take(pageSize);

                        count = l.Count();

                        return z.ToList();
                    }
                    else if (whereClause != null)
                    {
                        var filtered = Subjects.Where(whereClause);
                        count = filtered.Count();

                        return filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }
                    if (orderBy != null)
                    {
                        count = Subjects.Count();
                        return Subjects.OrderBy(orderbyClause).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    return Subjects.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not retrieve filtered subjects."), ex);
            }
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
                    if (_guow != null)
                        _guow.Dispose();
                    isDisposed = true;
                }
            }
        }
    }
}