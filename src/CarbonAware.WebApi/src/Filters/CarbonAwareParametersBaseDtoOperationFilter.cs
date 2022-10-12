using CarbonAware.Aggregators.CarbonAware;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarbonAware.WebApi.Filters;

public class CarbonAwareParametersBaseDtoOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Find api parameters of non-overridden inherited properties on CarbonAwareParametersBaseDTO child classes.
        var parametersToHide = context.ApiDescription.ParameterDescriptions
            // For CarbonAwareParametersBaseDTO child classes;
            .Where(apiParam => apiParam.ModelMetadata?.ContainerType?.BaseType == typeof(CarbonAwareParametersBaseDTO))
            // Look at all the properties of the child class;
            .Where(apiParam => apiParam.ModelMetadata?.ContainerType?.GetProperties()
                            // Get the class property matching the API parameter name;
                            .FirstOrDefault(clsProp => string.Equals(clsProp.Name, apiParam.ModelMetadata?.Name))?
                            // Check if the child class declares the property (i.e. it's not overridden).
                            .DeclaringType != apiParam.ModelMetadata?.ContainerType)
            .ToList();

        // Remove those API parameters
        foreach (var parameterToHide in parametersToHide)
        {
            var apiParameter = operation.Parameters.FirstOrDefault(
                apiParam => string.Equals(apiParam.Name, parameterToHide.Name, System.StringComparison.Ordinal)
            );
            if (apiParameter != null)
                operation.Parameters.Remove(apiParameter);
        }
    }
}