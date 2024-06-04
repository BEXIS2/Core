namespace Vaiona.Persistence.Api
{
    public interface IPersistenceManager
    {
        object Factory { get; }
        IUnitOfWorkFactory UnitOfWorkFactory { get; }

        void Configure(string connectionString = "", string databaseDilect = "DB2Dialect", string fallbackFoler = "Default", bool showQueries = false, bool configureModules = false);

        void ExportSchema(bool generateScript = false, bool executeAgainstTargetDB = true, bool justDrop = false);

        void UpdateSchema(bool generateScript = false, bool executeAgainstTargetDB = true);

        void Start();

        void Shutdown();

        string GetProperty(string propertyName);

        int PreferredPushSize { get; }
    }
}