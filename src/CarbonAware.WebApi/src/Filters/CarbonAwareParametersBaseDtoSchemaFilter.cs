using CarbonAware.Aggregators.CarbonAware;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarbonAware.WebApi.Filters;

public class CarbonAwareParametersBaseDtoSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Only for CarbonAwareParametersBaseDTO child classes
        if (context.Type.BaseType == typeof(CarbonAwareParametersBaseDTO))
        {
            // Find non-overridden inherited properties.
            var nonOverriddenInheritedProperties = context.Type.GetProperties()
                .Where(p => p.DeclaringType != context.Type)
                .ToList();

            // Remove those properties from generated schema
            foreach (var propertyToHide in nonOverriddenInheritedProperties)
            {
                var schemaProperty = schema.Properties.FirstOrDefault(
                    p => string.Equals(p.Key, propertyToHide.Name, System.StringComparison.InvariantCultureIgnoreCase)
                );
                
                schema.Properties.Remove(schemaProperty);
            }
        }
    }
}