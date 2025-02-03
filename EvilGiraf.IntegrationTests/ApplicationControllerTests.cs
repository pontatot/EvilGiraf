using System.Net;
using System.Net.Http.Json;
using EvilGiraf.Dto;
using EvilGiraf.Model;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace EvilGiraf.IntegrationTests;

public class ApplicationControllerTests : IClassFixture<SubstituteWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _services;

    public ApplicationControllerTests(SubstituteWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _services = factory.Services;
        
        var kubeClient = _services.GetRequiredService<IKubernetes>();
        var appv1OperationsSubstitute = Substitute.For<IAppsV1Operations>();

        var deploymentResponse = new HttpOperationResponse<V1Deployment>();
        deploymentResponse.Body = new V1Deployment();
        appv1OperationsSubstitute.CreateNamespacedDeploymentWithHttpMessagesAsync(Arg.Any<V1Deployment>(), Arg.Any<string>())
            .Returns(deploymentResponse);
        
        kubeClient.AppsV1.Returns(appv1OperationsSubstitute);
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenNameIsMissing()
    {
        var request = new
        {
            Link = "hello-world"
        };

        var response = await _client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var request = new
        {
            Name = string.Empty,
            Link = "hello-world"
        };

        var response = await _client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenLinkIsMissing()
    {
        var request = new
        {
            Name = "my-app"
        };

        var response = await _client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenLinkIsEmpty()
    {
        var request = new
        {
            Name = "my-app",
            Link = string.Empty
        };

        var response = await _client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployDocker_ReturnsCreated_WhenRequestIsValid()
    {
        var request = new
        {
            Name = "my-app",
            Link = "hello-world"
        };

        var response = await _client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<DeployResponse>();
        content!.Status.Should().Be(ApplicationStatus.Running);
    }

    [Fact]
    public async Task DeployDocker_ReturnsBadRequest_WhenNameTypeIsNotString()
    {
        var request = new
        {
            Name = 12345,
            Link = "hello-world"
        };

        var response = await _client.PostAsJsonAsync("/deploy/docker", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenNameIsMissing()
    {
        var request = new
        {
            Link = "https://github.com/pontatot/EvilGiraf/"
        };

        var response = await _client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var request = new
        {
            Name = string.Empty,
            Link = "https://github.com/pontatot/EvilGiraf/"
        };

        var response = await _client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenLinkIsMissing()
    {
        var request = new
        {
            Name = "my-app"
        };

        var response = await _client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenLinkIsEmpty()
    {
        var request = new
        {
            Name = "my-app",
            Link = string.Empty
        };

        var response = await _client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeployGithub_ReturnsCreated_WhenRequestIsValid()
    {
        var request = new
        {
            Name = "my-app",
            Link = "https://github.com/pontatot/EvilGiraf/"
        };

        var response = await _client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<DeployResponse>();
        content!.Status.Should().Be(ApplicationStatus.Running);
    }

    [Fact]
    public async Task DeployGithub_ReturnsBadRequest_WhenNameTypeIsNotString()
    {
        var request = new
        {
            Name = 12345,
            Link = "https://github.com/pontatot/EvilGiraf/"
        };

        var response = await _client.PostAsJsonAsync("/deploy/github", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
