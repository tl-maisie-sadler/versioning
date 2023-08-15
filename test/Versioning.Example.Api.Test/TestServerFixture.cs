using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Versioning.Example.Api.Test
{
    public class TestServerFixture : WebApplicationFactory<Program>, IDisposable
    {
        internal ITestOutputHelper? OutputHelper { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder
                .ConfigureAppConfiguration((ctx, services) =>
                {
                    var testDir = Path.GetDirectoryName(GetType().Assembly.Location);
                    var configLocation = Path.Combine(testDir!, "testsettings.json");

                    services.Sources.Clear();
                    services.AddJsonFile(configLocation);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(new CapturingLoggerProvider(() => OutputHelper));
                });
        }
    }
}
