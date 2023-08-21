using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Versioning.Test;

public class FromToVersionTests
{
    public record TestClass
    {
        public string? AlwaysThere { get; set; }

        [FromVersion(_propertyAvailableFromVersion)]
        public string? AvailableFromDate { get; set; }

        [ToVersion(_propertyAvailableToVersion)]
        public string? AvailableToDate { get; set; }

        [FromVersion(_propertyAvailableFromVersion)]
        [ToVersion(_propertyAvailableToVersion)]
        public string? AvailableBetweenDates { get; set; }
    }

    private static JsonOptions GetJsonOptions()
    {
        var services = new ServiceCollection();
        services.AddVersioning();
        var sp = services.BuildServiceProvider();

        return sp.GetRequiredService<IOptions<JsonOptions>>().Value;
    }

    private const string _dateBefore = "2023-08-01";
    private const string _propertyAvailableFromVersion = "2023-08-10";
    private const string _dateBetween = "2023-08-15";
    private const string _propertyAvailableToVersion = "2023-08-19";
    private const string _dateAfter = "2023-08-25";

        private static TestClass CreateTestClass()
    {
        return new TestClass
        {
            AlwaysThere = "property-value",
            AvailableFromDate = "available-after",
            AvailableToDate = "available-before",
            AvailableBetweenDates = "available-between",
        };
    }

    private readonly ActivitySource _activitySource;
    private readonly JsonOptions _jsonOptions;

    public FromToVersionTests()
    {
        _activitySource = new ActivitySource("Testing");
        var activityListener = new ActivityListener
        {
            ShouldListenTo = s => true,
            SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
            Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(activityListener);
        _jsonOptions = GetJsonOptions();
    }

    [Fact]
    public void DateAfter_ShowPropertyAfter()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _dateAfter);

        // Act
        var serialized = JsonSerializer.Serialize(CreateTestClass(), _jsonOptions.SerializerOptions);

        // Assert
        Assert.Equal("{\"alwaysThere\":\"property-value\",\"availableFromDate\":\"available-after\"}", serialized);
    }

    [Fact]
    public void DateBetween_ShowAll()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _dateBetween);

        // Act
        var serialized = JsonSerializer.Serialize(CreateTestClass(), _jsonOptions.SerializerOptions);

        // Assert
        Assert.Equal("{\"alwaysThere\":\"property-value\",\"availableFromDate\":\"available-after\",\"availableToDate\":\"available-before\",\"availableBetweenDates\":\"available-between\"}", serialized);
    }

    [Fact]
    public void DateBefore_ShowPropertyBefore()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _dateBefore);

        // Act
        var serialized = JsonSerializer.Serialize(CreateTestClass(), _jsonOptions.SerializerOptions);

        // Assert
        Assert.Equal("{\"alwaysThere\":\"property-value\",\"availableToDate\":\"available-before\"}", serialized);
    }
}
