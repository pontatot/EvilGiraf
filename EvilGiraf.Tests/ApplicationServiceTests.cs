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
        var application = new ApplicationCreateDto("TestApp", ApplicationType.Docker, "https://test.com", "1.0.0");

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
        var application = new ApplicationCreateDto("TestApp", ApplicationType.Docker, "https://test.com", "1.0.0");
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
        var application = new ApplicationCreateDto("TestApp", ApplicationType.Docker, "https://test.com", "1.0.0");
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
    
    [Fact]
    public async Task UpdateApplication_ShouldUpdateAndReturnApplication()
    {
        // Arrange
        var applicationDto = new ApplicationCreateDto(
            "TestApp", ApplicationType.Docker, "https://test.com", "1.0.0");
        var createdApplication = await _applicationService.CreateApplication(applicationDto);

        var updatedApplicationDto = new ApplicationUpdateDto(
            "UpdatedTestApp", ApplicationType.Docker, "https://updatedtest.com", "2.0.0");

        // Act
        var result = await _applicationService.UpdateApplication(createdApplication.Id, updatedApplicationDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(updatedApplicationDto.Name);
        result.Type.Should().Be(updatedApplicationDto.Type);
        result.Link.Should().Be(updatedApplicationDto.Link);
        result.Version.Should().Be(updatedApplicationDto.Version);
    }
    
    [Fact]
    public async Task UpdateApplication_ShouldReturnNotUpdatedApplication()
    {
        // Arrange
        var applicationDto = new ApplicationCreateDto(
            "TestApp", ApplicationType.Docker, "https://test.com", "1.0.0");
        var createdApplication = await _applicationService.CreateApplication(applicationDto);

        var updatedApplicationDto = new ApplicationUpdateDto(
            null, null, null, null);

        // Act
        var result = await _applicationService.UpdateApplication(createdApplication.Id, updatedApplicationDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(applicationDto.Name);
        result.Type.Should().Be(applicationDto.Type);
        result.Link.Should().Be(applicationDto.Link);
        result.Version.Should().Be(applicationDto.Version);
    }
    
        [Fact]
        public async Task UpdateApplication_ShouldReturnNull_WhenApplicationDoesNotExist()
        {
            // Arrange
            var updatedApplicationDto = new ApplicationUpdateDto(
                "UpdatedTestApp", ApplicationType.Docker, "https://updatedtest.com", "2.0.0");
    
            // Act
            var result = await _applicationService.UpdateApplication(999, updatedApplicationDto);
    
            // Assert
            result.Should().BeNull();
        }
}