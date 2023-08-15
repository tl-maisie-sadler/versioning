using System.Net;
using Xunit.Abstractions;

namespace Versioning.Example.Api.Test;

public class UnitTest1
{
    private readonly ITestOutputHelper _outputHelper;

    public UnitTest1(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async void Test1()
    {
        // Arrange
        using var fixture = new TestServerFixture();
        fixture.OutputHelper = _outputHelper;
        using var httpClient = fixture.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(2);

        // Act
        var response = await httpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
