using System.Net;
using Xunit.Abstractions;

namespace Versioning.Example.Api.Test;

public class ApiVersionUsedTests
{
    private readonly ITestOutputHelper _outputHelper;

    public ApiVersionUsedTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async void OverrideHeader_UsedVersionIsOverride()
    {
        // Arrange
        using var fixture = new TestServerFixture();
        fixture.OutputHelper = _outputHelper;
        using var httpClient = fixture.CreateClient();

        httpClient.DefaultRequestHeaders.Add("Tl-Version", "2023-02-31");

        // Act
        var response = await httpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Tl-Version", out var version));
    }
}
