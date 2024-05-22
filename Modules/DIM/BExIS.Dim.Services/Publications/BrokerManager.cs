using BExIS.Dim.Entities.Publications;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Services.Publications
{
    public class BrokerManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public BrokerManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            BrokerRepository = _guow.GetReadOnlyRepository<Broker>();
        }

        ~BrokerManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Broker> BrokerRepository { get; }
        public IQueryable<Broker> Requests => BrokerRepository.Query();

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