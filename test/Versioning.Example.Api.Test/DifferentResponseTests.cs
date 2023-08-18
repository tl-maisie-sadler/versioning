using System.Net;
using System.Text.Json.Nodes;
using Xunit.Abstractions;

namespace Versioning.Example.Api.Test;

public class DifferentResponseTests
{
    private readonly ITestOutputHelper _outputHelper;

    public DifferentResponseTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async void Version_2023_02_31()
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
        var responseJson = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();

        Assert.Equal(3, responseJson.Count());
        Assert.True(responseJson.TryGetPropertyValue("parameter", out var parameterVal1));
        Assert.True(responseJson.TryGetPropertyValue("another_parameter", out var parameterVal2));
        Assert.True(responseJson.TryGetPropertyValue("a_number", out var parameterVal3));
        Assert.Equal("value 1", parameterVal1!.ToString());
        Assert.Equal("value 2", parameterVal2!.ToString());
        Assert.Equal("23", parameterVal3!.ToString());
    }

    [Fact]
    public async void Version_2023_06_30_RemovesParam()
    {
        // Arrange
        using var fixture = new TestServerFixture();
        fixture.OutputHelper = _outputHelper;
        using var httpClient = fixture.CreateClient();

        httpClient.DefaultRequestHeaders.Add("Tl-Version", "2023-06-30");

        // Act
        var response = await httpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseJson = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();

        Assert.Equal(1, responseJson.Count());
        Assert.True(responseJson.TryGetPropertyValue("parameter", out var parameterVal1));
        Assert.Equal("value 1", parameterVal1!.ToString());
    }
}
