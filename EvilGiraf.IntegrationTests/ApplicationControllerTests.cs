using System.Net;
using System.Net.Http.Json;
using EvilGiraf.Dto;
using EvilGiraf.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EvilGiraf.IntegrationTests;

public class ApplicationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApplicationControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenNameIsMissing()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            Link = "hello-world"
        };

        var response = await client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            Name = string.Empty,
            Link = "hello-world"
        };

        var response = await client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenLinkIsMissing()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            Name = "my-app"
        };

        var response = await client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenLinkIsEmpty()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            Name = "my-app",
            Link = string.Empty
        };

        var response = await client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployDocker_ReturnsCreated_WhenRequestIsValid()
    {
        var client = _factory.CreateClient();
        var request = new
        {
            Name = "my-app",
            Link = "hello-world"
        };

        var response = await client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<DeployResponse>();
        content!.Status.Should().Be(ApplicationStatus.Running);
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenNameTypeIsNotString()
    {
        var client = _factory.CreateClient();
        var request = new
        {
            Name = 12345,
            Link = "hello-world"
        };

        var response = await client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenNameIsMissing()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            Link = "https://github.com/pontatot/EvilGiraf/"
        };

        var response = await client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            Name = string.Empty,
            Link = "https://github.com/pontatot/EvilGiraf/"
        };

        var response = await client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenLinkIsMissing()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            Name = "my-app"
        };

        var response = await client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenLinkIsEmpty()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            Name = "my-app",
            Link = string.Empty
        };

        var response = await client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsCreated_WhenRequestIsValid()
    {
        var client = _factory.CreateClient();
        var request = new
        {
            Name = "my-app",
            Link = "https://github.com/pontatot/EvilGiraf/"
        };

        var response = await client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<DeployResponse>();
        content!.Status.Should().Be(ApplicationStatus.Running);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenNameTypeIsNotString()
    {
        var client = _factory.CreateClient();
        var request = new
        {
            Name = 12345,
            Link = "https://github.com/pontatot/EvilGiraf/"
        };

        var response = await client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
