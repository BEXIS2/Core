
using BEXIS2.DB.Persistence.NH;
using BEXIS2.DB.Persistence.Api;
using BEXIS2.Web.Shell.DB;
using BEXIS2.Web.Shell.Extensions;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using DbSession = NHibernate.ISession; // Alias definieren

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddIoCConfiguration(builder.Configuration);

var app = builder.Build();

// 1. ISessionFactory als Singleton registrieren (teuer in der Erstellung, wird einmalig initialisiert)
builder.Services.AddSingleton<ISessionFactory>(provider =>
{
    var configuration = new Configuration();

    configuration.DataBaseIntegration(db =>
    {
        db.Dialect<PostgreSQLDialect>();
        db.Driver<NpgsqlDriver>();
        db.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        //db.KeywordsImportAutoImport = Hbm2DDlKeyWords.AutoQuote;
        db.LogFormattedSql = true;
        db.LogSqlInConsole = true;
    });

    // Mappings hinzufügen (hier per Assembly, sucht nach hbm.xml oder Fluent Mappings)
    configuration.AddAssembly(typeof(Program).Assembly);

    // add mappingfiles
    DirectoryInfo mappingDirectory = new DirectoryInfo("..\\Workspace\\Components");
    configuration.AddDirectory(mappingDirectory);

    return configuration.BuildSessionFactory();
});



// Konfiguriere Persistence Manager
//var persistenceManager = app.Services.GetRequiredService<IPersistenceManager>() as NHibernatePersistenceManager;
//if (persistenceManager != null)
//{
//    persistenceManager.Configure(
//        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
//        databaseDilect: "PostgreSQLDialect",
//        //fallbackFolder: "Default",
//        showQueries: !app.Environment.IsProduction(),
//        configureModules: true
//    );
//}

// 2. ISession (Unit of Work) als Scoped-Service registrieren
// Pro HTTP-Request wird exakt eine Session erstellt und am Ende automatisch verworfen.
builder.Services.AddScoped<DbSession>(provider =>
{
    var sessionFactory = provider.GetRequiredService<ISessionFactory>();
    return sessionFactory.OpenSession();
});

builder.Services.AddScoped<DatasetManager>();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
