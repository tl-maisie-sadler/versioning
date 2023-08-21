using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Versioning.Test;

public class NewEnumValuesTests
{
    public enum TestValues
    {
        Pass, Fail, Fail_Specific,
    }
    public record TestClass
    {
        public TestValues? Result { get; set; }
    }

    private const string _dateBefore = "2023-08-01";
    private const string _propertyAvailableFromVersion = "2023-08-10";
    private const string _dateAfter = "2023-08-15";

    private readonly ActivitySource _activitySource;

    public class CustomObjectToInferredTypesConverter : StringEnumValueConverter<TestValues>
    {
        public CustomObjectToInferredTypesConverter()
        {
            MapValueBeforeVersion(_propertyAvailableFromVersion, TestValues.Fail_Specific, TestValues.Fail);
        }
    }

    private static JsonOptions GetJsonOptions()
    {
        var services = new ServiceCollection();
        services.AddVersioning();
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new CustomObjectToInferredTypesConverter());
        });
        var sp = services.BuildServiceProvider();

        return sp.GetRequiredService<IOptions<JsonOptions>>().Value;
    }

    public NewEnumValuesTests()
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
    public void BeforeDateAvailable_PropertyShouldMapToOldValue()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _dateBefore);
        var jsonOptions = GetJsonOptions();

        var testClass = new TestClass()
        {
            Result = TestValues.Fail_Specific,
        };

        // Act
        var serialized = JsonSerializer.Serialize(testClass, jsonOptions.SerializerOptions);

        // Assert
        Assert.Equal("{\"result\":\"Fail\"}", serialized);
    }

    [Fact]
    public void OnAvailable_PropertyShouldShowNewValue()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _propertyAvailableFromVersion);
        var jsonOptions = GetJsonOptions();

        var testClass = new TestClass()
        {
            Result = TestValues.Fail_Specific,
        };

        // Act
        var serialized = JsonSerializer.Serialize(testClass, jsonOptions.SerializerOptions);

        // Assert
        Assert.Equal("{\"result\":\"Fail_Specific\"}", serialized);
    }

    [Fact]
    public void AfterDateAvailable_PropertyShouldShowNewValue()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", _dateAfter);
        var jsonOptions = GetJsonOptions();

        var testClass = new TestClass()
        {
            Result = TestValues.Fail_Specific,
        };

        // Act
        var serialized = JsonSerializer.Serialize(testClass, jsonOptions.SerializerOptions);

        // Assert
        Assert.Equal("{\"result\":\"Fail_Specific\"}", serialized);
    }
}
