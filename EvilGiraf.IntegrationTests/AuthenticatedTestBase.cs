namespace EvilGiraf.IntegrationTests;

public abstract class AuthenticatedTestBase : IntegrationTestBase
{
    protected AuthenticatedTestBase(CustomWebApplicationFactory factory) : base(factory)
    {
        Client.DefaultRequestHeaders.Add("X-API-Key", "test-api-key");
    }
}