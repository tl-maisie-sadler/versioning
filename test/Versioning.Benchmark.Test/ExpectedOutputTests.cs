using System.Diagnostics;

namespace Versioning.Benchmark.Test;

public class ExpectedOutputTests
{
    private readonly ActivitySource _activitySource;

    public ExpectedOutputTests()
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
    public void WithoutVersioningOutputIsAsExpected()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", Versions.VersionNow);

        var x = new SerializationWithVersioningBenchmark();

        // Act
        var output = x.WithoutVersioning();

        // Assert
        var expectedOutput = "{"
            + "\"NestedVisible\":{\"Shown\":\"value\",\"SkipMe\":\"***\"},"
            + "\"NestedCollectionVisible\":[{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"}],"
            + "\"NestedSkipped\":{\"Shown\":\"value\",\"SkipMe\":\"***\"},"
            + "\"NestedCollectionSkipped\":[{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"}]"
            + "}";
        Assert.Equal(expectedOutput, output);
    }

    [Fact]
    public void WithVersioningOutputIsAsExpected()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", Versions.VersionNow);

        var x = new SerializationWithVersioningBenchmark();

        // Act
        var output = x.WithVersioning();

        // Assert
        var expectedOutput = "{"
            + "\"nestedVisible\":{\"shown\":\"value\"},"
            + "\"nestedCollectionVisible\":[{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"}]"
            + "}";
        Assert.Equal(expectedOutput, output);
    }
}
