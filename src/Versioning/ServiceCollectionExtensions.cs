using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
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
            options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { DetectVersionAttribute },
            };
            // options.SerializerOptions.Converters
            //     .Add<>();
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
                var versionAttributes = provider
                    .GetCustomAttributes(typeof(TrueLayerVersionAttribute), inherit: true);

                propertyInfo.ShouldSerialize = (obj, value) =>
                {
                    // obj = class
                    // value = property
                    var tlVersion = Activity.Current.GetTlVersion();

                    foreach (var versionAttribute in versionAttributes)
                    {
                        if (versionAttribute is TrueLayerVersionAttribute trueLayerVersionAttribute)
                        {
                            if (trueLayerVersionAttribute.ShouldSkipForVersion(tlVersion))
                                return false;
                        }
                    }

                    return true;
                };
            }
        }
    }
}
