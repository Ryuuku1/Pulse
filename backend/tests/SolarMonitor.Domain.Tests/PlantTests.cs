using SolarMonitor.Domain.Entities;
using Xunit;

namespace SolarMonitor.Domain.Tests;

public class PlantTests
{
    [Fact]
    public void Plant_CanBeCreatedWithRequiredProperties()
    {
        // Arrange & Act
        var plant = new Plant
        {
            Id = "TEST-001",
            Name = "Test Solar Plant",
            Status = PlantStatus.Connected
        };

        // Assert
        Assert.NotNull(plant);
        Assert.Equal("TEST-001", plant.Id);
        Assert.Equal("Test Solar Plant", plant.Name);
        Assert.Equal(PlantStatus.Connected, plant.Status);
    }

    [Fact]
    public void Plant_CanHaveOptionalProperties()
    {
        // Arrange & Act
        var plant = new Plant
        {
            Id = "TEST-002",
            Name = "Test Plant with Details",
            Status = PlantStatus.Connected,
            Address = "123 Solar Street",
            InstalledCapacityKw = 10.5m,
            Latitude = 52.52m,
            Longitude = 13.405m,
            InstallationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Assert
        Assert.Equal("123 Solar Street", plant.Address);
        Assert.Equal(10.5m, plant.InstalledCapacityKw);
        Assert.Equal(52.52m, plant.Latitude);
        Assert.Equal(13.405m, plant.Longitude);
        Assert.Equal(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), plant.InstallationDate);
    }

    [Theory]
    [InlineData(PlantStatus.Unknown)]
    [InlineData(PlantStatus.Connected)]
    [InlineData(PlantStatus.Disconnected)]
    [InlineData(PlantStatus.Fault)]
    [InlineData(PlantStatus.Offline)]
    public void Plant_CanHaveAllStatuses(PlantStatus status)
    {
        // Arrange & Act
        var plant = new Plant
        {
            Id = "TEST-003",
            Name = "Test Plant",
            Status = status
        };

        // Assert
        Assert.Equal(status, plant.Status);
    }
}
