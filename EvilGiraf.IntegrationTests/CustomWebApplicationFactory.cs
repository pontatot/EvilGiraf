using EvilGiraf.Service;
using k8s;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EvilGiraf.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private const string TestApiKey = "test-api-key";

    public CustomWebApplicationFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("evilgiraf_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ApiKey"] = TestApiKey
            }!);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IKubernetes));
            services.AddSingleton(Substitute.For<IKubernetes>());
            
            services.RemoveAll(typeof(DatabaseService));
            services.AddDbContext<DatabaseService>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseService>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}