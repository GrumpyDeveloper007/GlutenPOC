using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Gluten.Data.Access.Service;

// Retrieve the connection string for use with the application. 
string connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_CONNECTIONSTRING");

if (connectionString == null)
{
    Console.WriteLine("Azure BLOB connection string not found.");
}


var sqlConnection = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");

var dbEndpoint = Environment.GetEnvironmentVariable("DbEndpointUri");
if (dbEndpoint == null)
{
    Console.WriteLine("dbEndpoint string not found.");
}
var dbPrimaryKey = Environment.GetEnvironmentVariable("DbPrimaryKey");
if (dbPrimaryKey == null)
{
    Console.WriteLine("dbPrimaryKey string not found.");
}

if (sqlConnection == null)
{
    Console.WriteLine("Azure SQL connection string not found.");
}

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddSingleton<DataStore>(new DataStore(dbEndpoint, dbPrimaryKey));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        //services.AddDbContext<ShoppingContext>(options => options.UseSqlServer(sqlConnection, b => b.MigrationsAssembly("TheShire")));
    })
    .Build();

host.Run();
