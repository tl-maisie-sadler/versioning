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
                Modifiers = { DetectIgnoreDataMemberAttribute }
            };
        });

        return services;
    }
    public static WebApplication UseVersioning(this WebApplication app)
    {
        app.UseMiddleware<VersionHeaderMiddleware>();
        return app;
    }

    private static void DetectIgnoreDataMemberAttribute(JsonTypeInfo typeInfo)
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
                if (fromVersion == null) continue;
                if (fromVersion is not FromVersionAttribute fromVersionAttribute) continue;

                propertyInfo.ShouldSerialize = (obj, value) =>
                {
                    // obj = class
                    // value = property
                    var comparisonResult = Activity.Current.CompareToTlVersion(fromVersionAttribute.MinimumVersion);
                    return comparisonResult >= 0;
                };
            }
        }
    }
}
