using System.Diagnostics;
using BenchmarkDotNet.Running;

var activitySource = new ActivitySource("Testing");
var activityListener = new ActivityListener
{
    ShouldListenTo = s => true,
    SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
    Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
};
ActivitySource.AddActivityListener(activityListener);

using var activity = activitySource.StartActivity("test", ActivityKind.Internal);
activity?.SetTag("version", Versions.VersionNow);

var summary = BenchmarkRunner.Run<SerializationWithVersioningBenchmark>();
