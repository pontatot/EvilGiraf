using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EvilGiraf.IntegrationTests;

public class WeatherForecastControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WeatherForecastControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetWeatherForecast_Returns200AndForecasts()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/WeatherForecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var forecasts = await response.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>();
        forecasts.Should().NotBeNull()
            .And.HaveCount(5)
            .And.OnlyContain(f =>
                f.Date > DateOnly.FromDateTime(DateTime.Now) &&
                f.TemperatureC >= -20 && f.TemperatureC <= 55 &&
                !string.IsNullOrEmpty(f.Summary));
    }

    [Fact]
    public async Task GetWeatherForecast_InvalidRoute_Returns404()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/InvalidRoute");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}