using BExIS.Dlm.Entities.Party;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Party
{
    public class PartyRelationshipManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public IReadOnlyRepository<PartyRelationship> PartyRelationshipRepository { get; }
        public IQueryable<PartyRelationship> PartyRelationships => PartyRelationshipRepository.Query();

        public PartyRelationshipManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            PartyRelationshipRepository = _guow.GetReadOnlyRepository<PartyRelationship>();
        }

        ~PartyRelationshipManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
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

        public PartyRelationship FindById(long partyRelationshipId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var PartyRelationshipRepository = uow.GetRepository<PartyRelationship>();
                return PartyRelationshipRepository.Get(partyRelationshipId);
            }
        }
    }
}