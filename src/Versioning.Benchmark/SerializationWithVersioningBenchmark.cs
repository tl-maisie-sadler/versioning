using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Versioning;

public static class Versions
{
    public const string FutureVersion = "2023-09-10";
    public const string VersionNow = "2023-09-01";
}

public record Response
{
    public ResponseInner? NestedVisible { get; set; }
    public ResponseInner[]? NestedCollectionVisible { get; set; }

    [FromVersion(Versions.FutureVersion)]
    public ResponseInner? NestedSkipped { get; set; }

    [FromVersion(Versions.FutureVersion)]
    public ResponseInner[]? NestedCollectionSkipped { get; set; }
}

public record ResponseInner
{
    public string? Shown { get; set; }

    [FromVersion(Versions.FutureVersion)]
    public string? SkipMe { get; set; }
}

[JsonExporterAttribute.Full()]
[CsvMeasurementsExporter()]
[CsvExporter(CsvSeparator.Comma)]
[HtmlExporter()]
[MarkdownExporterAttribute.GitHub()]
public class SerializationWithVersioningBenchmark
{
    private readonly Response _response;
    private readonly JsonOptions _jsonOptions;

    public SerializationWithVersioningBenchmark()
    {
        _response = new Response
        {
            NestedVisible = new ResponseInner { Shown = "value", SkipMe = "***" },
            NestedSkipped = new ResponseInner { Shown = "value", SkipMe = "***" },
            NestedCollectionVisible = Enumerable.Repeat(new ResponseInner { Shown = "value", SkipMe = "***" }, 10).ToArray(),
            NestedCollectionSkipped = Enumerable.Repeat(new ResponseInner { Shown = "value", SkipMe = "***" }, 10).ToArray(),
        };

        var services = new ServiceCollection();
        services.AddVersioning();
        var sp = services.BuildServiceProvider();

        _jsonOptions = sp.GetRequiredService<IOptions<JsonOptions>>().Value;
    }

    [Benchmark]
    public string WithoutVersioning()
    {
        var serialized = JsonSerializer.Serialize(_response);
        return serialized;
    }

    [Benchmark(Baseline = true)]
    public string WithVersioning()
    {
        var serialized = JsonSerializer.Serialize(_response, _jsonOptions.SerializerOptions);
        return serialized;
    }
}
