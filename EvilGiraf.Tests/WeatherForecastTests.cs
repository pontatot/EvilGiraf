using FluentAssertions;

namespace EvilGiraf.Tests;

public class WeatherForecastTests
{
    [Fact]
    public void WeatherForecast_Should_InitializeCorrectly()
    {
        // Arrange
        var date = new DateOnly(2023, 5, 15);
        var temperatureC = 25;
        var summary = "Warm";

        // Act
        var forecast = new WeatherForecast
        {
            Date = date,
            TemperatureC = temperatureC,
            Summary = summary
        };

        // Assert
        forecast.Date.Should().Be(date);
        forecast.TemperatureC.Should().Be(temperatureC);
        forecast.Summary.Should().Be(summary);
        forecast.TemperatureF.Should().Be(76);
    }

    [Theory]
    [InlineData(-20, -3)]
    [InlineData(0, 32)]
    [InlineData(25, 76)]
    [InlineData(55, 130)]
    public void WeatherForecast_Should_Calculate_TemperatureF_Correctly(int temperatureC, int expectedTemperatureF)
    {
        // Arrange
        var forecast = new WeatherForecast
        {
            TemperatureC = temperatureC
        };

        // Act & Assert
        forecast.TemperatureF.Should().Be(expectedTemperatureF);
    }
}