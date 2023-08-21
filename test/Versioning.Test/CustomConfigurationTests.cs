// using System.Diagnostics;
// using System.Text.Json;
// using System.Text.Json.Serialization;
// using System.Text.Json.Serialization.Metadata;
// using Microsoft.AspNetCore.Http.Json;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Options;

// namespace Versioning.Test;

// public class CustomConfigurationTests
// {
//     public record TestClass
//     {
//         public string? AlwaysThere { get; set; }
//         public string? NullProp { get; set; }

//         [FromVersion(_propertyAvailableFromVersion)]
//         public string? AvailableFromDate { get; set; }
//     }

//     private const string _dateBefore = "2023-08-01";
//     private const string _propertyAvailableFromVersion = "2023-08-10";

//     private static TestClass CreateTestClass()
//     {
//         return new TestClass
//         {
//             AlwaysThere = "property-value",
//             AvailableFromDate = "not-available",
//             NullProp = null,
//         };
//     }

//     private readonly ActivitySource _activitySource;

//     public CustomConfigurationTests()
//     {
//         _activitySource = new ActivitySource("Testing");
//         var activityListener = new ActivityListener
//         {
//             ShouldListenTo = s => true,
//             SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
//             Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
//         };
//         ActivitySource.AddActivityListener(activityListener);
//     }

//     // private static void ShouldIgnoreNull(JsonTypeInfo typeInfo)
//     // {
//     //     return default(T) is null ? value is not null : !EqualityComparer<T>.Default.Equals(default, value);
//     // }

//     [Fact]
//     public void DateBefore_ShowPropertyBefore()
//     {
//         // Arrange
//         using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
//         activity?.SetTag("version", _dateBefore);

//         var services = new ServiceCollection();
//         services.AddVersioning();
//         services.Configure<JsonOptions>(options =>
//         {
//             options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
//             {
//                 // Modifiers = { ShouldIgnoreNull },
//             };
//         });
//         var sp = services.BuildServiceProvider();

//         var jsonOptions = sp.GetRequiredService<IOptions<JsonOptions>>().Value;

//         // Act
//         var serialized = JsonSerializer.Serialize(CreateTestClass(), jsonOptions.SerializerOptions);

//         // Assert
//         Assert.Equal("{\"alwaysThere\":\"property-value\"}", serialized);
//     }
// }
