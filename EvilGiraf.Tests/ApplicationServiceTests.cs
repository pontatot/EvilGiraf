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
}