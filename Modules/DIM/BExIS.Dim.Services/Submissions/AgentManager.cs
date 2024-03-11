using BExIS.Dim.Entities.Submissions;
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

        public IReadOnlyRepository<Agent> AgentRepository { get; }

        public IQueryable<Agent> Agents => AgentRepository.Query();

        public AgentManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            AgentRepository = _guow.GetReadOnlyRepository<Agent>();
        }

        ~AgentManager()
        {
            Dispose(true);
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
