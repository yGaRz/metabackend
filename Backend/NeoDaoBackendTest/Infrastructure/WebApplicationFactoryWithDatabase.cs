using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NeoDaoBackend.Models;
using NeoDaoBackendTest.Extensions;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace NeoDaoBackendTest.Infrastructure;

public class WebApplicationFactoryWithDatabase : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ContainerName = "container-neo-dao";
    private const string DatabaseName = "neo-dao";
    private const string Username = "postgres";
    private const string Password = "postgres";
    private const ushort PostgrePort = 5678;

    public NeoDaoDbContext DbContext { get; private set; } = null!;

    private DbConnection? _connection = null;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithName($"{ContainerName}-{Guid.NewGuid()}")
        .WithDatabase(DatabaseName)
        .WithUsername(Username)
        .WithPassword(Password)
        .WithPortBinding(PostgrePort, 5432)
        .WithEnvironment("PGDATA", "/pgdata")
        .WithTmpfsMount("/pgdata")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Initialize database
            services.RemoveDbContext<NeoDaoDbContext>();

            services.AddDbContext<NeoDaoDbContext>(options =>
            {
                options.UseNpgsql(_container.GetConnectionString());
            });
            //services.CreateDatabase<NeoDaoDbContext>(); 
        });
        _container.StartAsync().Wait();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        DbContext = Services.CreateScope().ServiceProvider.GetRequiredService<NeoDaoDbContext>();
        _connection = DbContext.Database.GetDbConnection();
        await _connection.OpenAsync();
    }

    public async new Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
        }
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    //public async Task<bool> ResetDatabase()
    //{
    //    if( _connection != null )
    //    {
    //        Respawner respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
    //        {
    //            DbAdapter = DbAdapter.Postgres,
    //            SchemasToInclude = new[] { "public" }
    //        });
    //        await respawner.ResetAsync(_connection);
    //        return true;
    //    }
    //   return false;
    //}  
}