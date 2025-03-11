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
        var applicationDto = new ApplicationDto(
            "TestApp", ApplicationType.Docker, "https://test.com", "1.0.0");

        // Act
        var result = await _applicationService.CreateApplication(applicationDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(applicationDto.Name);
        result.Type.Should().Be(applicationDto.Type);
        result.Link.Should().Be(applicationDto.Link);
        result.Version.Should().Be(applicationDto.Version);
    }

    [Fact]
    public async Task GetApplication_ShouldReturnApplication()
    {
        // Arrange
        var applicationDto = new ApplicationDto(
            "TestApp", ApplicationType.Docker, "https://test.com", "1.0.0");
        var createdApplication = await _applicationService.CreateApplication(applicationDto);

        // Act
        var result = await _applicationService.GetApplication(createdApplication.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createdApplication.Id);
        result.Name.Should().Be(applicationDto.Name);
        result.Type.Should().Be(applicationDto.Type);
        result.Link.Should().Be(applicationDto.Link);
        result.Version.Should().Be(applicationDto.Version);
    }

    [Fact]
    public async Task GetApplication_ShouldThrowKeyNotFoundException()
    {
        // Act
        Func<Task> act = async () => await _applicationService.GetApplication(999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Application with id 999 not found");
    }

    [Fact]
    public async Task DeleteApplication_ShouldDeleteApplication()
    {
        // Arrange
        var applicationDto = new ApplicationDto(
            "TestApp", ApplicationType.Docker, "https://test.com", "1.0.0");
        var createdApplication = await _applicationService.CreateApplication(applicationDto);

        // Act
        var result = await _applicationService.DeleteApplication(createdApplication.Id);

        Func<Task> act = async () => await _applicationService.GetApplication(createdApplication.Id);
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Application with id " + createdApplication.Id + " not found");
    }

    [Fact]
    public async Task DeleteApplication_ShouldThrowKeyNotFoundException()
    {
        // Act
        Func<Task> act = async () => await _applicationService.DeleteApplication(999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Application with id 999 not found");
    }
}