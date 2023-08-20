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

    private readonly string _expectedOutputWithoutVersioning = "{"
        + "\"NestedVisible\":{\"Shown\":\"value\",\"SkipMe\":\"***\"},"
        + "\"NestedCollectionVisible\":[{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"}],"
        + "\"NestedSkipped\":{\"Shown\":\"value\",\"SkipMe\":\"***\"},"
        + "\"NestedCollectionSkipped\":[{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"},{\"Shown\":\"value\",\"SkipMe\":\"***\"}]"
        + "}";

    private readonly string _expectedOutputWithVersioning = "{"
        + "\"nestedVisible\":{\"shown\":\"value\"},"
        + "\"nestedCollectionVisible\":[{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"},{\"shown\":\"value\"}]"
        + "}";

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
        Assert.Equal(_expectedOutputWithoutVersioning, output);
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
        Assert.Equal(_expectedOutputWithVersioning, output);
    }

    [Fact]
    public void CanBeCalledMultipleTimes()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", Versions.VersionNow);

        var x = new SerializationWithVersioningBenchmark();

        // Act & Assert
        for (var i = 0; i < 100; i++)
        {
            Assert.Equal(_expectedOutputWithVersioning, x.WithVersioning());
            Assert.Equal(_expectedOutputWithoutVersioning, x.WithoutVersioning());
        }
    }

    [Fact]
    public async void CanBeCalledInParallel()
    {
        // Arrange
        using var activity = _activitySource.StartActivity("test", ActivityKind.Internal);
        activity?.SetTag("version", Versions.VersionNow);

        var x = new SerializationWithVersioningBenchmark();

        // Act & Assert
        void RunTest()
        {
            for (var i = 0; i < 100; i++)
            {
                Assert.Equal(_expectedOutputWithVersioning, x!.WithVersioning());
                Assert.Equal(_expectedOutputWithoutVersioning, x.WithoutVersioning());
            }
        }

        var t1 = Task.Run(() => RunTest());
        var t2 = Task.Run(() => RunTest());

        await Task.WhenAll(t1, t2);
    }
}
