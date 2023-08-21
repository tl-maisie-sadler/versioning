using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        [JsonConverter(typeof(CustomObjectToInferredTypesConverter))]
        public TestValues? Result { get; set; }
    }

    private const string _dateBefore = "2023-08-01";
    private const string _propertyAvailableFromVersion = "2023-08-10";

    private readonly ActivitySource _activitySource;

    public abstract class StringEnumValueConverter<TEnum> : JsonConverter<TEnum>
        where TEnum : struct, Enum
    {
        public override TEnum Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => throw new NotImplementedException();

        public override void Write(
            Utf8JsonWriter writer,
            TEnum objectToWrite,
            JsonSerializerOptions options)
        {
            objectToWrite = ConvertValue(objectToWrite);

            var enumString = TypeDescriptor.GetConverter(objectToWrite).ConvertTo(objectToWrite, typeof(string))?.ToString();
            writer.WriteStringValue(enumString);
        }

        protected abstract TEnum ConvertValue(TEnum value);
    }

    public class CustomObjectToInferredTypesConverter : StringEnumValueConverter<TestValues>
    {
        protected override TestValues ConvertValue(TestValues value)
        {
            if (value == TestValues.Fail_Specific)
                return TestValues.Fail;

            return value;
        }
    }

    private static JsonOptions GetJsonOptions()
    {
        var services = new ServiceCollection();
        services.AddVersioning();
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
}
