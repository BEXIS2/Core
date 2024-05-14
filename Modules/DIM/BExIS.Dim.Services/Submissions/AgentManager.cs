using BExIS.Dim.Entities.Submissions;
using BExIS.Dlm.Entities.Party;
using BExIS.Utils.NH.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;
using System.Linq.Dynamic.Core;

namespace BExIS.Dim.Services.Submissions
{
    public class AgentManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        ~AgentManager()
        {
            Dispose(true);
        }
        public IReadOnlyRepository<Agent> AgentRepository { get; }
        public IQueryable<Agent> Agents => AgentRepository.Query();
        public AgentManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            AgentRepository = _guow.GetReadOnlyRepository<Agent>();
        }

        public Agent Create(Agent agent)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Agent>();
                entityRequestRepository.Put(agent);
                uow.Commit();

                return agent;
            }

            return null;
        }

        public void Delete(Agent agent)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Agent>();
                entityRequestRepository.Delete(agent);
                uow.Commit();
            }
        }

        public Agent FindById(long id)
        {
            return AgentRepository.Get(id);
        }

        public List<Agent> Find(FilterExpression filter, OrderByExpression orderBy, int pageNumber, int pageSize, out int count)
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
                        var l = Agents.Where(whereClause);
                        var x = l.OrderBy(orderbyClause);
                        var y = x.Skip((pageNumber - 1) * pageSize);
                        var z = y.Take(pageSize);

                        count = l.Count();

                        return z.ToList();
                    }
                    else if (whereClause != null)
                    {
                        var filtered = Agents.Where(whereClause);
                        count = filtered.Count();

                        return filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    if (orderBy != null)
                    {
                        count = Agents.Count();
                        return Agents.OrderBy(orderbyClause).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    count = count = Agents.Count();

                    // without filter and order
                    return Agents.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not retrieve filtered groups."), ex);
            }
        }

        public void Update(Agent entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<Agent>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }
    }
}
