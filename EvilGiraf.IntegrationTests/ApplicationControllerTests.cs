using System.Net;
using System.Net.Http.Json;
using EvilGiraf.Dto;
using EvilGiraf.Model;
using EvilGiraf.Service;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EvilGiraf.IntegrationTests;

public class ApplicationControllerTests : AuthenticatedTestBase
{
    private readonly DatabaseService _dbContext;

    public ApplicationControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _dbContext = GetDatabaseService();
    }
        
    [Fact]
    public async Task Create_ShouldCreateApplication()
    {
        // Arrange
        var createRequest = new ApplicationCreateDto("test-application", ApplicationType.Docker, "docker.io/test-application:latest", "1.0.0", [22]);

        // Act
        var response = await Client.PostAsJsonAsync("/api/application", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var application = await response.Content.ReadFromJsonAsync<Application>();
        
        application.Should().NotBeNull();
        application!.Name.Should().Be(createRequest.Name);
        application.Type.Should().Be(createRequest.Type);
        application.Link.Should().Be(createRequest.Link);
        application.Version.Should().Be(createRequest.Version);
        application.Id.Should().NotBe(0);
        application.Ports.Should().BeEquivalentTo(createRequest.Ports);

        // Verify the application was saved to the database
        var savedApplication = await _dbContext.Applications.FindAsync(application.Id);
        savedApplication.Should().NotBeNull();
        savedApplication!.Name.Should().Be(createRequest.Name);
        savedApplication.Type.Should().Be(createRequest.Type);
        savedApplication.Link.Should().Be(createRequest.Link);
        savedApplication.Version.Should().Be(createRequest.Version);
        savedApplication.Ports.Should().BeEquivalentTo(createRequest.Ports);
    }

    [Fact]
    public async Task CreateWithWrongBody_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new ApplicationCreateDto(null!, ApplicationType.Docker, "docker.io/test-application:latest", "1.0.0", [22]);

        // Act
        var response = await Client.PostAsJsonAsync("/api/application", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_ShouldReturnApplication()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-application",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-application:latest",
            Version = "1.0.0",
            Ports = [22]
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/application/{application.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationDto = await response.Content.ReadFromJsonAsync<Application>();
        
        applicationDto.Should().NotBeNull();
        applicationDto!.Name.Should().Be(application.Name);
        applicationDto.Type.Should().Be(application.Type);
        applicationDto.Link.Should().Be(application.Link);
        applicationDto.Version.Should().Be(application.Version);
        applicationDto.Id.Should().Be(application.Id);
        applicationDto.Ports.Should().BeEquivalentTo(application.Ports);
    }

    [Fact]
    public async Task GetWithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-application",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-application:latest",
            Version = "1.0.0",
            Ports = [22]
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/application/{application.Id + 1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldDeleteApplication()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-application",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-application:latest",
            Version = "1.0.0",
            Ports = [22]
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/application/{application.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var checkresponse = await Client.GetAsync($"/api/application/{application.Id}");
        checkresponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteWithNonExistingId_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"/api/application/{999}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldUpdateApplication()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-application",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-application:latest",
            Version = "1.0.0",
            Ports = [22]
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new ApplicationUpdateDto("updated-application", ApplicationType.Git, "k8s.io/updated-application:latest", "2.0.0", [23]);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/application/{application.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedApplication = await response.Content.ReadFromJsonAsync<Application>();
        
        updatedApplication.Should().NotBeNull();

        updatedApplication!.Name.Should().Be(updateRequest.Name);
        updatedApplication.Type.Should().Be(updateRequest.Type);
        updatedApplication.Link.Should().Be(updateRequest.Link);
        updatedApplication.Version.Should().Be(updateRequest.Version);
        updatedApplication.Id.Should().Be(application.Id);
        updatedApplication.Ports.Should().BeEquivalentTo(updateRequest.Ports);

        // Verify the application was updated in the database
        response = await Client.GetAsync($"/api/application/{application.Id}");
        var savedApplication = await response.Content.ReadFromJsonAsync<Application>();

        savedApplication.Should().NotBeNull();

        savedApplication!.Name.Should().Be(updateRequest.Name);
        savedApplication.Type.Should().Be(updateRequest.Type);
        savedApplication.Link.Should().Be(updateRequest.Link);
        savedApplication.Version.Should().Be(updateRequest.Version);
        savedApplication.Ports.Should().BeEquivalentTo(updateRequest.Ports);
    }

    [Fact]
    public async Task UpdateWithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var updateRequest = new ApplicationUpdateDto("updated-application", ApplicationType.Git, "k8s.io/updated-application:latest", "2.0.0", [22]);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/application/{999}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWithNullBody_ShouldReturnApplication()
    {
        // Arrange
        var application = new Application
        {
            Name = "test-application",
            Type = ApplicationType.Docker,
            Link = "docker.io/test-application:latest",
            Version = "1.0.0",
            Ports = [22]
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new ApplicationUpdateDto(null, null, null, null, null);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/application/{application.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedApplication = await response.Content.ReadFromJsonAsync<Application>();

        updatedApplication.Should().NotBeNull();
        updatedApplication!.Name.Should().Be(application.Name);
        updatedApplication.Type.Should().Be(application.Type);
        updatedApplication.Link.Should().Be(application.Link);
        updatedApplication.Version.Should().Be(application.Version);
        updatedApplication.Id.Should().Be(application.Id);
        updatedApplication.Ports.Should().BeEquivalentTo(application.Ports);

        // Verify the application was updated in the database
        response = await Client.GetAsync($"/api/application/{application.Id}");
        var savedApplication = await response.Content.ReadFromJsonAsync<Application>();

        savedApplication.Should().NotBeNull();

        savedApplication!.Name.Should().Be(application.Name);
        savedApplication.Type.Should().Be(application.Type);
        savedApplication.Link.Should().Be(application.Link);
        savedApplication.Version.Should().Be(application.Version);
        savedApplication.Ports.Should().BeEquivalentTo(application.Ports);
    }

    [Fact]
    public async Task List_ShouldReturnListOfApplications()
    {
        var applications = new List<Application>
        {
            new()
            {
                Name = "test-application-1",
                Type = ApplicationType.Docker,
                Link = "docker.io/test-application-1:latest",
                Version = "1.0.0",
                Ports = [22]
            },
            new()
            {
                Name = "test-application-2",
                Type = ApplicationType.Git,
                Link = "k8s.io/test-application-2:latest",
                Version = "2.0.0",
                Ports = [22]
            }
        };

        _dbContext.Applications.AddRange(applications);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/application");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var applicationDtos = await response.Content.ReadFromJsonAsync<List<Application>>();

        applicationDtos.Should().NotBeNull();
        applicationDtos.Should().HaveCountGreaterOrEqualTo(2);

        applicationDtos.Should().ContainEquivalentOf(applications[0]);
        applicationDtos.Should().ContainEquivalentOf(applications[1]);
    }

    [Fact]
    public async Task CreateWithNoPorts_ShouldReturnApplication()
    {
        // Arrange
        var createRequest = new ApplicationCreateDto("test-application", ApplicationType.Docker, "docker.io/test-application:latest", "1.0.0", null);

        // Act
        var response = await Client.PostAsJsonAsync("/api/application", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var application = await response.Content.ReadFromJsonAsync<Application>();
        
        application.Should().NotBeNull();
        application!.Name.Should().Be(createRequest.Name);
        application.Type.Should().Be(createRequest.Type);
        application.Link.Should().Be(createRequest.Link);
        application.Version.Should().Be(createRequest.Version);
        application.Id.Should().NotBe(0);
        application.Ports.Should().BeEmpty();

        // Verify the application was saved to the database
        var savedApplication = await _dbContext.Applications.FindAsync(application.Id);
        savedApplication.Should().NotBeNull();
        savedApplication!.Name.Should().Be(createRequest.Name);
        savedApplication.Type.Should().Be(createRequest.Type);
        savedApplication.Link.Should().Be(createRequest.Link);
        savedApplication.Version.Should().Be(createRequest.Version);
        savedApplication.Ports.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateWithMultiplePorts_ShouldReturnApplication()
    {
        // Arrange
        var createRequest = new ApplicationCreateDto("test-application", ApplicationType.Docker, "docker.io/test-application:latest", "1.0.0", [22, 80]);

        // Act
        var response = await Client.PostAsJsonAsync("/api/application", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var application = await response.Content.ReadFromJsonAsync<Application>();
        
        application.Should().NotBeNull();
        application!.Name.Should().Be(createRequest.Name);
        application.Type.Should().Be(createRequest.Type);
        application.Link.Should().Be(createRequest.Link);
        application.Version.Should().Be(createRequest.Version);
        application.Id.Should().NotBe(0);
        application.Ports.Should().BeEquivalentTo(createRequest.Ports);

        // Verify the application was saved to the database
        var savedApplication = await _dbContext.Applications.FindAsync(application.Id);
        savedApplication.Should().NotBeNull();
        savedApplication!.Name.Should().Be(createRequest.Name);
        savedApplication.Type.Should().Be(createRequest.Type);
        savedApplication.Link.Should().Be(createRequest.Link);
        savedApplication.Version.Should().Be(createRequest.Version);
        savedApplication.Ports.Should().BeEquivalentTo(createRequest.Ports);
    }
}