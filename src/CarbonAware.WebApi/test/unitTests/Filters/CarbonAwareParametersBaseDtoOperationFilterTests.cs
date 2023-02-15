using CarbonAware.WebApi.Filters;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Reflection;

namespace CarbonAware.WepApi.UnitTests;

[TestFixture]
public class CarbonAwareParametersBaseDtoOperationFilterTests
{
    // Not relevant for tests which populate this field via the [SetUp] attribute.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private SchemaRepository _schemaRepository;
    private ISchemaGenerator _schemaGenerator;
    private OpenApiOperation _operation;
    private MethodInfo? _methodInfo;

#pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        this._schemaRepository = new SchemaRepository("document-name");
        this._operation = new OpenApiOperation();

        ISerializerDataContractResolver dataContractResolver = new JsonSerializerDataContractResolver(new JsonSerializerOptions());
        this._schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), dataContractResolver);

        this._methodInfo = _schemaRepository.GetType().GetMethod("some-method");
    }

    [Test]
    public void Apply_ReturnsSuccessfully()
    {
        // Arrange
        OperationFilterContext operationFiltercontext = new OperationFilterContext(
            new ApiDescription(),
            _schemaGenerator,
            _schemaRepository,
            _methodInfo);

        CarbonAwareParametersBaseDtoOperationFilter caBaseDtoOperationFilter = new();

        // Act & Assert
        Assert.DoesNotThrow(() => caBaseDtoOperationFilter.Apply(_operation, operationFiltercontext));
    }
}