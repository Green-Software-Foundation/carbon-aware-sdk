using CarbonAware.WebApi.Filters;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using CarbonAware.WebApi.Models;

namespace CarbonAware.WepApi.UnitTests;

[TestFixture]

public class CarbonAwareParametersBaseDtoSchemaFilterTests
{

    // Not relevant for tests which populate this field via the [SetUp] attribute.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private SchemaRepository _schemaRepository;
    private ISchemaGenerator _schemaGenerator;

#pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        this._schemaRepository = new SchemaRepository("document-name");
        ISerializerDataContractResolver dataContractResolver = new JsonSerializerDataContractResolver(new JsonSerializerOptions());
        this._schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), dataContractResolver);
    }

    [TestCase(typeof(CarbonIntensityBatchParametersDTO), new string[] { "MultipleLocations", "Duration", "Requested" }, TestName = "CarbonIntensityBatchParametersDTO, Removing nonOverriddenInheritedProperties")]
    [TestCase(typeof(CarbonIntensityParametersDTO), new string[] { "MultipleLocations", "Duration", "Requested" }, TestName = "CarbonIntensityParametersDTO, Removing nonOverriddenInheritedProperties")]
    [TestCase(typeof(EmissionsDataForLocationsParametersDTO), new string[] { "SingleLocation", "Duration", "Requested" }, TestName = "EmissionsDataForLocationsParametersDTO, Removing nonOverriddenInheritedProperties")]
    [TestCase(typeof(EmissionsForecastBatchParametersDTO), new string[] { "MultipleLocations" }, TestName = "EmissionsForecastBatchParametersDTO, Removing nonOverriddenInheritedProperties")]
    [TestCase(typeof(EmissionsForecastCurrentParametersDTO), new string[] { "SingleLocation", "Requested" }, TestName = "EmissionsForecastCurrentParametersDTO, Removing nonOverriddenInheritedProperties")]
    [TestCase(typeof(EmissionsDataDTO), new string[] {}, TestName = "EmissionsDataDTO, Does not remove any properties from non-child class")]
    public void Apply_RemovesCorrectProperties_FromSchema(Type type, string[] nonOverriddenProperties)
    {
        // Arrange
        string[] carbonAwareParametersBaseDTOProperties = {
            "SingleLocation",
            "MultipleLocations",
            "Start",
            "End",
            "Duration",
            "Requested"
        };

        var schemaFilterContext = new SchemaFilterContext(
            type,
            _schemaGenerator,
            _schemaRepository);

        var caBaseDtoSchemaFilter = new CarbonAwareParametersBaseDtoSchemaFilter();

        var openApiProperties = new Dictionary<string, OpenApiSchema>();
        foreach (var property in carbonAwareParametersBaseDTOProperties)
        {
            openApiProperties.Add(property, new OpenApiSchema());
        }

        var schema = new OpenApiSchema()
        {
            Properties = openApiProperties
        };

        // Act & Assert
        Assert.DoesNotThrow(() => caBaseDtoSchemaFilter.Apply(schema, schemaFilterContext));
        foreach (var property in carbonAwareParametersBaseDTOProperties)
        {
            if(nonOverriddenProperties.Contains(property))
            {
                Assert.That(schema.Properties.ContainsKey(property), Is.False);
            }
            else
            {
                Assert.That(schema.Properties.ContainsKey(property), Is.True);
            }
        }
    }
}