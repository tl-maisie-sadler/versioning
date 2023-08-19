using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Versioning.Test;

public class ObjectStructureTests
{
    private const string _propertyAvailableFromVersion = "2023-08-19";
    private const string _requestVersion = "2023-08-01";

    private readonly ActivitySource _activitySource;

    public record TestClass
    {
        public string? AlwaysThereString { get; set; }
        public int? AlwaysThereInt { get; set; }
        public bool? AlwaysThereBool { get; set; }

        [FromVersion(_propertyAvailableFromVersion)]
        public string? SkipMeString { get; set; }

        [FromVersion(_propertyAvailableFromVersion)]
        public int? SkipMeInt { get; set; }

        [FromVersion(_propertyAvailableFromVersion)]
        public bool? SkipMeBool { get; set; }
    }

    private static JsonOptions GetJsonOptions()
    {
        var services = new ServiceCollection();
        services.AddVersioning();
        var sp = services.BuildServiceProvider();

        return sp.GetRequiredService<IOptions<JsonOptions>>().Value;
    }

    public ObjectStructureTests()
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
    public void SimpleObject_SkipProperty()
    {
        // Arrange
        KnownTlVersions.Instance.Register(_requestVersion);
        KnownTlVersions.Instance.Register(_propertyAvailableFromVersion);
        var jsonOptions = GetJsonOptions();

        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _requestVersion);

        // Act
        var serialized = JsonSerializer.Serialize(new TestClass
        {
            AlwaysThereString = "property-value",
            AlwaysThereInt = 23,
            AlwaysThereBool = true,
            SkipMeString = "hidden-property-value",
            SkipMeInt = 33,
            SkipMeBool = false,
        }, jsonOptions.SerializerOptions);

        // Assert
        Assert.Equal(
            "{\"alwaysThereString\":\"property-value\","
            + "\"alwaysThereInt\":23,"
            + "\"alwaysThereBool\":true}",
            serialized);
    }
}
