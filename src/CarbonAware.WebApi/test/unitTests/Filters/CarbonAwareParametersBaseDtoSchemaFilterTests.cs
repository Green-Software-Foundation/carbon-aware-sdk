using CarbonAware.WebApi.Filters;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;
using GSF.CarbonAware.Handlers.CarbonAware;
using System.Text.Json;

namespace CarbonAware.WepApi.UnitTests;

[TestFixture]

public class CarbonAwareParametersBaseDtoSchemaFilterTests
{

// Not relevant for tests which populate this field via the [SetUp] attribute.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private OpenApiSchema _schema;
    private Type _type;
    private SchemaRepository _schemaRepository;
    private ISchemaGenerator _schemaGenerator;

#pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        this._schema = new OpenApiSchema();
        this._type = typeof(CarbonAwareParametersBaseDTO);
        this._schemaRepository = new SchemaRepository("document-name");
        ISerializerDataContractResolver dataContractResolver = new JsonSerializerDataContractResolver(new JsonSerializerOptions());
        this._schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), dataContractResolver);
    }

    [Test]
    public void Apply_ReturnsSuccessfully()
    {
        // Arrange
        var schemaFilterContext = new SchemaFilterContext(
            _type,
            _schemaGenerator,
            _schemaRepository);

        CarbonAwareParametersBaseDtoSchemaFilter caBaseDtoSchemaFilter = new();

        // Act & Assert
        Assert.DoesNotThrow(() => caBaseDtoSchemaFilter.Apply(_schema, schemaFilterContext));
    }
}