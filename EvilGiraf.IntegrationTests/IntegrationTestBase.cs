using EvilGiraf.Service;
using k8s;
using Microsoft.Extensions.DependencyInjection;

namespace EvilGiraf.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
    }

    protected DatabaseService GetDatabaseService()
    {
        return Scope.ServiceProvider.GetRequiredService<DatabaseService>();
    }

    public IKubernetes GetKubernetesClient() => Factory.GetKubernetesClient();
}