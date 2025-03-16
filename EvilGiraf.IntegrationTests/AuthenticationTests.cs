using System.Net;
using FluentAssertions;

namespace EvilGiraf.IntegrationTests;

public class AuthenticationTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Request_WithoutApiKey_ShouldReturn401()
    {
        // Arrange
        using var client = Factory.CreateClient(); // Create new client without auth headers

        // Act
        var response = await client.GetAsync("/application/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Request_WithWrongApiKey_ShouldReturn401()
    {
        // Arrange
        using var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", "wrong-api-key");

        // Act
        var response = await client.GetAsync("/application/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Request_WithEmptyApiKey_ShouldReturn401()
    {
        // Arrange
        using var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", ["", ""]); 

        // Act
        var response = await client.GetAsync("/application/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}