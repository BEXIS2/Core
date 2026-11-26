using BEXIS2.Dlm.Entities.Data;
using NHibernate;
using NHibernate.Linq;
using DbSession = NHibernate.ISession;

namespace BEXIS2.Web.Shell.DB
{
    public class DatasetManager
    {
        private readonly DbSession _session;

        public DatasetManager(DbSession session)
        {
            _session = session;
        }

        public async Task<List<Dataset>> GetAllAsync()
        {
            return await _session.Query<Dataset>().ToListAsync();
        }

    }
}
