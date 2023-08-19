using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Versioning.Test;

public class ObjectStructureTests
{
    public record TestClassCollection
    {
        public TestClass[]? Collection { get; set; }

        [FromVersion(_propertyAvailableFromVersion)]
        public TestClass[]? HiddenCollection { get; set; }
    }

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

    private const string _expectedSerializedTestClass = "{\"alwaysThereString\":\"property-value\","
                + "\"alwaysThereInt\":23,"
                + "\"alwaysThereBool\":true}";
    private static TestClass CreateTestClass()
    {
        return new TestClass
        {
            AlwaysThereString = "property-value",
            AlwaysThereInt = 23,
            AlwaysThereBool = true,
            SkipMeString = "hidden-property-value",
            SkipMeInt = 33,
            SkipMeBool = false,
        };
    }

    private const string _propertyAvailableFromVersion = "2023-08-19";
    private const string _requestVersion = "2023-08-01";

    private readonly ActivitySource _activitySource;
    private readonly JsonOptions _jsonOptions;

    public ObjectStructureTests()
    {
        KnownTlVersions.Instance.Register(_requestVersion);
        KnownTlVersions.Instance.Register(_propertyAvailableFromVersion);

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
    public void SimpleObject_SkipProperty()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _requestVersion);

        // Act
        var serialized = JsonSerializer.Serialize(CreateTestClass(), _jsonOptions.SerializerOptions);

        // Assert
        Assert.Equal(_expectedSerializedTestClass, serialized);
    }

    [Fact]
    public void NestedCollection_SkipProperty()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _requestVersion);

        var collection = new TestClassCollection()
        {
            Collection = new[] { CreateTestClass(), CreateTestClass() },
            HiddenCollection = new[] { CreateTestClass(), CreateTestClass() },
        };

        // Act
        var serialized = JsonSerializer.Serialize(collection, _jsonOptions.SerializerOptions);

        // Assert
        var expected = "{\"collection\":" + $"[{_expectedSerializedTestClass},{_expectedSerializedTestClass}]" + "}";
        Assert.Equal(expected, serialized);
    }

    [Fact]
    public void Collection_SkipProperty()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _requestVersion);

        // Act
        var serialized = JsonSerializer.Serialize(new[] { CreateTestClass(), CreateTestClass() }, _jsonOptions.SerializerOptions);

        // Assert
        var expected = $"[{_expectedSerializedTestClass},{_expectedSerializedTestClass}]";
        Assert.Equal(expected, serialized);
    }
}
