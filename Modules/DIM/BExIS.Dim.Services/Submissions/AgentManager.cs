using BExIS.Dim.Entities.Submissions;
using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

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

        public void Create(Agent agent)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Agent>();
                entityRequestRepository.Put(agent);
                uow.Commit();
            }
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
