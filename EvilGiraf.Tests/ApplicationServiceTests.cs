using EvilGiraf.Dto;
using EvilGiraf.Model;
using EvilGiraf.Service;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EvilGiraf.Tests;

public class ApplicationServiceTests
{
    private readonly ApplicationService _applicationService;

    public ApplicationServiceTests()
    {
        var databaseService = new DatabaseService(new DbContextOptionsBuilder<DatabaseService>().UseInMemoryDatabase("TestDb").Options);
        _applicationService = new ApplicationService(databaseService);
    }

    [Fact]
    public async Task CreateApplication_ShouldCreateAndReturnNewApplication()
    {
        // Arrange
        var application = new Application(){
            Name = "TestApp",
            Type = ApplicationType.Docker,
            Link = "https://test.com",
            Version = "1.0.0"
        };

        // Act
        var result = await _applicationService.CreateApplication(application);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(application.Name);
        result.Type.Should().Be(application.Type);
        result.Link.Should().Be(application.Link);
        result.Version.Should().Be(application.Version);
    }

    [Fact]
    public async Task GetApplication_ShouldReturnApplication()
    {
        // Arrange
        var application = new Application(){
            Name = "TestApp",
            Type = ApplicationType.Docker,
            Link = "https://test.com",
            Version = "1.0.0"
        };
        var createdApplication = await _applicationService.CreateApplication(application);

        // Act
        var result = await _applicationService.GetApplication(createdApplication.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdApplication.Id);
        result.Name.Should().Be(application.Name);
        result.Type.Should().Be(application.Type);
        result.Link.Should().Be(application.Link);
        result.Version.Should().Be(application.Version);
    }

    [Fact]
    public async Task GetApplication_ShouldReturnNull()
    {
        // Act
        var result = await _applicationService.GetApplication(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteApplication_ShouldDeleteApplication()
    {
        // Arrange
        var application = new Application(){
            Name = "TestApp",
            Type = ApplicationType.Docker,
            Link = "https://test.com",
            Version = "1.0.0"
        };
        var createdApplication = await _applicationService.CreateApplication(application);

        // Act
        var result = await _applicationService.DeleteApplication(createdApplication.Id);

        // Assert
        result.Should().NotBeNull();

        var app = await _applicationService.GetApplication(createdApplication.Id);
        app.Should().BeNull();
    }

    [Fact]
    public async Task DeleteApplication_ShouldReturnNull()
    {
        // Act
        var result = await _applicationService.DeleteApplication(999);

        // Assert
        result.Should().BeNull();
    }
}