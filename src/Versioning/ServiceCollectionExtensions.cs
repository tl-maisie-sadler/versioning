using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Versioning;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { DetectVersionAttribute },
            };
        });

        return services;
    }

    public static WebApplication UseVersioning(this WebApplication app)
    {
        app.UseMiddleware<VersionHeaderMiddleware>();
        return app;
    }

    private static void DetectVersionAttribute(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
        {
            if (propertyInfo.AttributeProvider is ICustomAttributeProvider provider)
            {
                var fromVersion = provider
                    .GetCustomAttributes(typeof(FromVersionAttribute), inherit: true)
                    .FirstOrDefault();

                var toVersion = provider
                    .GetCustomAttributes(typeof(ToVersionAttribute), inherit: true)
                    .FirstOrDefault();

                propertyInfo.ShouldSerialize = (obj, value) =>
                {
                    // obj = class
                    // value = property
                    var tlVersion = Activity.Current.GetTlVersion();
                    if (fromVersion is FromVersionAttribute fromVersionAttribute)
                    {
                        var fromComparisonResult = VersionExtensions.CompareVersions(tlVersion, fromVersionAttribute.MinimumVersion);
                        if (fromComparisonResult < 0) return false;
                    }

                    if (toVersion is ToVersionAttribute toVersionAttribute)
                    {
                        var toComparisonResult = VersionExtensions.CompareVersions(toVersionAttribute.MaximumVersion, tlVersion);
                        if (toComparisonResult < 0) return false;
                    }

                    return true;
                };
            }
        }
    }
}
