using BExIS.Dim.Entities.Publications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public IQueryable<Broker> Brokers => BrokerRepository.Query();

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

        public Broker Create(Broker broker)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var entityRequestRepository = uow.GetRepository<Broker>();
                    entityRequestRepository.Put(broker);
                    uow.Commit();
                }

                return broker;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool Delete(Broker broker)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var brokerRepository = uow.GetRepository<Broker>();
                    brokerRepository.Delete(broker);
                    uow.Commit();
                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }

        }

        public bool DeleteById(long brokerId)
        {
            return Delete(BrokerRepository.Get(brokerId));
        }

        public Task<Broker> FindByIdAsync(long brokerId)
        {
            return Task.FromResult(BrokerRepository.Get(brokerId));

        }

        public Broker FindById(long brokerId)
        {
            return BrokerRepository.Get(brokerId);
        }

        public List<Broker> FindByName(string name)
        {
            return BrokerRepository.Query(b => string.Equals(b.Name, name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public bool Update(Broker broker)
        {
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    var repo = uow.GetRepository<Broker>();
                    repo.Merge(broker);
                    var merged = repo.Get(broker.Id);
                    repo.Put(merged);
                    uow.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}