using System.Net;
using System.Net.Http.Json;
using EvilGiraf.Dto;
using EvilGiraf.Model;
using EvilGiraf.Service;
using EvilGiraf.Controller;
using FluentAssertions;
using k8s;
using k8s.Autorest;
using k8s.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvilGiraf.IntegrationTests;

public class ApplicationControllerTests : IntegrationTestBase
{
    private readonly DatabaseService _dbContext;
    private readonly IKubernetes _kubernetes;

    public ApplicationControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _dbContext = GetDatabaseService();
        _kubernetes = GetKubernetesClient();
    }
        
    [Fact]
    public async Task Create_ShouldCreateApplication()
    {
        // Arrange
        var createRequest = new ApplicationCreateDto("test-application", ApplicationType.Docker, "docker.io/test-application:latest", "1.0.0");

        // Act
        var response = await Client.PostAsJsonAsync("/application", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var application = await response.Content.ReadFromJsonAsync<Application>();
        
        application.Should().NotBeNull();
        application!.Name.Should().Be(createRequest.Name);
        application.Type.Should().Be(createRequest.Type);
        application.Link.Should().Be(createRequest.Link);
        application.Version.Should().Be(createRequest.Version);
        application.Id.Should().NotBe(default);

        // Verify the application was saved to the database
        var savedApplication = await _dbContext.Applications.FindAsync(application.Id);
        savedApplication.Should().NotBeNull();
        savedApplication!.Name.Should().Be(createRequest.Name);
        savedApplication.Type.Should().Be(createRequest.Type);
        savedApplication.Link.Should().Be(createRequest.Link);
        savedApplication.Version.Should().Be(createRequest.Version);
    }

    [Fact]
    public async Task CreateWithWrongBody_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new ApplicationCreateDto(null!, ApplicationType.Docker, "docker.io/test-application:latest", "1.0.0");

        // Act
        var response = await Client.PostAsJsonAsync("/application", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}