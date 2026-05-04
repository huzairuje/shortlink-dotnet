namespace MyFirstApi.Api.Swagger;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

public class SnakeCaseSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties is null || !schema.Properties.Any())
            return;

        var properties = schema.Properties
            .ToDictionary(
                kvp => JsonNamingPolicy.SnakeCaseLower.ConvertName(kvp.Key),
                kvp => kvp.Value
            );

        schema.Properties.Clear();
        foreach (var prop in properties)
            schema.Properties.Add(prop.Key, prop.Value);
    }
}