using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Versioning.Test;

public class VersionOrderTests
{
    public record TestClass
    {
        public string? AlwaysThereString { get; set; }

        [FromVersion(_propertyAvailableFromVersion)]
        public string? SkipMeString { get; set; }
    }

    private static JsonOptions GetJsonOptions()
    {
        var services = new ServiceCollection();
        services.AddVersioning();
        var sp = services.BuildServiceProvider();

        return sp.GetRequiredService<IOptions<JsonOptions>>().Value;
    }

    private const string _date3 = "2023-08-19";
    private const string _propertyAvailableFromVersion = "2023-08-10";
    private const string _date1 = "2023-08-01";

    private readonly ActivitySource _activitySource;

    public VersionOrderTests()
    {
        _activitySource = new ActivitySource("Testing");
        var activityListener = new ActivityListener
        {
            ShouldListenTo = s => true,
            SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
            Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(activityListener);
    }

    [Fact]
    public void VersionsSetPerRegistration()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _date1);

        var testClass = new TestClass
        {
            AlwaysThereString = "property-value",
            SkipMeString = "hidden-property-value",
        };

        var jsonOptions1 = GetJsonOptions();
        var jsonOptions2 = GetJsonOptions();

        // Act
        var serialized1 = JsonSerializer.Serialize(testClass, jsonOptions1.SerializerOptions);
        var serialized2 = JsonSerializer.Serialize(testClass, jsonOptions2.SerializerOptions);

        // Assert
        Assert.Equal("{\"alwaysThereString\":\"property-value\"}", serialized1);
        Assert.Equal("{\"alwaysThereString\":\"property-value\"}", serialized2);
    }
}
