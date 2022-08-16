using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using DevLab.JmesPath.Functions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using PropertyName = CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters.PropertyName;

namespace CarbonAware.Aggregators.Tests;

[TestFixture]
public class CarbonAwareParametersTests
{
    public const string RENAMED_MULTIPLE_LOCATIONS_PROPERTY = "myTestLocations";
    public const string RENAMED_START_PROPERTY = "myTestStartAt";
    public const string RENAMED_END_PROPERTY = "myTestEndAt";
    public const string RENAMED_UNINHERITED_PROPERTY = "myCustomProperty";
    public class TestDerivedDto : CarbonAwareParametersBaseDTO
    {
        [BindProperty(Name = RENAMED_MULTIPLE_LOCATIONS_PROPERTY)] override public string[]? MultipleLocations { get; set; }
        [BindProperty(Name = RENAMED_START_PROPERTY)] override public DateTimeOffset? Start { get; set; }
        [BindProperty(Name = RENAMED_END_PROPERTY)] override public DateTimeOffset? End { get; set; }

        [BindProperty(Name = RENAMED_UNINHERITED_PROPERTY)]
        public string? UninheritedBoundProp { get; set; }
        public string? UnboundProp { get; set; }
    }

    [Test]
    public void SetRequiredProperties_NoRequiredProperties()
    {
        // Arrange
        var parameters = new CarbonAwareParameters();

        // Act
        parameters.SetRequiredProperties(Array.Empty<PropertyName>());

        // Assert
        foreach (string name in Enum.GetNames<PropertyName>())
        {
            var propertyName = Enum.Parse<PropertyName>(name);
            Assert.IsFalse(parameters._props[propertyName].IsRequired);
        }
    }

    [Test]
    public void SetRequiredProperties_SetsExpectedFlags()
    {
        // Arrange
        var parameters = new CarbonAwareParameters();

        // Act
        parameters.SetRequiredProperties(PropertyName.MultipleLocations, PropertyName.Start);

        // Assert
        Assert.IsTrue(parameters._props[PropertyName.MultipleLocations].IsRequired);
        Assert.IsTrue(parameters._props[PropertyName.Start].IsRequired);
        Assert.IsFalse(parameters._props[PropertyName.End].IsRequired);
    }

    [TestCase("Start", null, TestName = "set single property, valid")]
    [TestCase("End", "MultipleLocations", TestName = "set multiple, valid")]
    [TestCase(null, null, TestName = "no required properties, valid")]
    public void TestValidate_ValidFor_RequiredPropertySet(PropertyName? propertyName1, PropertyName? propertyName2)
    {
        // Arrange
        var parameters = new CarbonAwareParameters();

        var propertyNames = new List<PropertyName>();
        if (propertyName1 != null) propertyNames.Add((PropertyName)propertyName1);
        if (propertyName2 != null) propertyNames.Add((PropertyName)propertyName2);
        foreach (var name in propertyNames)
        {
            parameters._props[name].IsSet = true;
            parameters.SetRequiredProperties(name);
        }
        // Act & Assert
        parameters.Validate();
    }

    [Test]
    public void TestValidate_ThrowsFor_RequiredPropertyNotSet()
    {
        // Arrange
        var parameters = new CarbonAwareParameters();
        parameters.SetRequiredProperties(PropertyName.Start);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parameters.Validate());
    }

    [Test]
    public void TestValidate_ThrowsFor_RequiredIEnumerablePropertyEmpty()
    {
        // Arrange
        var parameters = new CarbonAwareParameters()
        {
            MultipleLocations = Enumerable.Empty<Location>(),
        };

        parameters.SetRequiredProperties(PropertyName.MultipleLocations);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => parameters.Validate());
    }

    [Test]
    public void TestValidate_StartAfterEndThrows()
    {
        // Arrange
        var parameters = new CarbonAwareParameters()
        {
            Start = new DateTimeOffset(2022, 1, 5, 12, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2022, 1, 1, 12, 0, 0, TimeSpan.Zero),
        };
        parameters.SetRequiredProperties(new PropertyName[] { PropertyName.Start, PropertyName.End });

        // Assert
        Assert.Throws<ArgumentException>(() => parameters.Validate());
    }

    [Test]
    public void TestValidate_StartBeforeEnd()
    {
        // Arrange
        var parameters = new CarbonAwareParameters()
        {
            Start = new DateTimeOffset(2022, 1, 1, 12, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2022, 1, 5, 12, 0, 0, TimeSpan.Zero),
        };
        parameters.SetRequiredProperties(new PropertyName[] { PropertyName.Start, PropertyName.End });

        // Act
        parameters.Validate();

        // Shouldn't throw an error
    }

    [TestCase("2022-01-01T12:00:00Z", "2022-01-01T16:00:00Z", "2022-01-01T12:00:00Z", TestName = "start set, default not used")]
    [TestCase(null, "2022-01-01T16:00:00Z", "2022-01-01T16:00:00Z", TestName = "start not set, default used")]
    public void TestGetStartOrDefault(DateTimeOffset? startValue, DateTimeOffset defaultValue, DateTimeOffset expectedResult)
    {
        // Arrange
        var parameters = new CarbonAwareParameters();
        if (startValue != null) parameters.Start = (DateTimeOffset)startValue;

        // Act
        DateTimeOffset result = parameters.GetStartOrDefault(defaultValue);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    [TestCase("2022-01-01T12:00:00Z", "2022-01-01T16:00:00Z", "2022-01-01T12:00:00Z", TestName = "end set, default not used")]
    [TestCase(null, "2022-01-01T16:00:00Z", "2022-01-01T16:00:00Z", TestName = "end not set, default used")]
    public void TestGetEndOrDefault(DateTimeOffset? endValue, DateTimeOffset defaultValue, DateTimeOffset expectedResult)
    {
        // Arrange
        var parameters = new CarbonAwareParameters();
        if (endValue != null) parameters.End = (DateTimeOffset)endValue;

        // Act
        DateTimeOffset result = parameters.GetEndOrDefault(defaultValue);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }

    [TestCase("eastus", "2022-01-01", "2022-01-02", TestName = "implicit operator casts DTO: locations, start, end")]
    [TestCase(null, "2022-01-01", "2022-01-02", TestName = "implicit operator casts DTO: start, end")]
    [TestCase("eastus", null, null, TestName = "implicit operator casts DTO: locations")]
    [TestCase(null, null, null, TestName = "implicit operator casts DTO: no set properties")]
    public void ImplicitOperator_CastsDtoAsExpected(string? location, DateTimeOffset? start, DateTimeOffset? end)
    {
        // Arrange
        var dto = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { location! },
            Start = start,
            End = end
        };
        bool multipleLocationsIsSet = !string.IsNullOrEmpty(location);
        bool startIsSet = start.HasValue;
        bool endIsSet = end.HasValue;

        // Act
        var result = (CarbonAwareParameters) dto;

        // Assert
        Assert.AreEqual(multipleLocationsIsSet, result._props[PropertyName.MultipleLocations].IsSet);
        Assert.AreEqual(startIsSet, result._props[PropertyName.Start].IsSet);
        Assert.AreEqual(endIsSet, result._props[PropertyName.End].IsSet);
        if (multipleLocationsIsSet) { Assert.AreEqual(location, result.MultipleLocations.First().RegionName); }
        if (startIsSet) { Assert.AreEqual(start!.Value, result.Start); }
        if (endIsSet) { Assert.AreEqual(end!.Value, result.End); }
    }

    [Test]
    public void ImplicitOperator_CastsDtoWithNameMapping()
    {
        // Arrange
        var dto = new TestDerivedDto();

        // Act
        var result = (CarbonAwareParameters) dto;

        // Assert
        Assert.AreEqual(RENAMED_MULTIPLE_LOCATIONS_PROPERTY, result._props[PropertyName.MultipleLocations].DisplayName);
        Assert.AreEqual(RENAMED_START_PROPERTY, result._props[PropertyName.Start].DisplayName);
        Assert.AreEqual(RENAMED_END_PROPERTY, result._props[PropertyName.End].DisplayName);
    }

    [Test]
    public void PropertyNameEnum_MatchesModelAndDtoClassProperties()
    {
        // Arrange
        var dtoClassPropertyNames = typeof(CarbonAwareParametersBaseDTO).GetProperties().Select(p => p.Name);
        var modelClassPropertyNames = typeof(CarbonAwareParameters).GetProperties().Select(p => p.Name);
        var enumPropertyNames = Enum.GetNames<PropertyName>();

        // Assert
        CollectionAssert.AreEquivalent(enumPropertyNames, dtoClassPropertyNames, "CarbonAwareParametersBaseDTO class properties do not match expected enums");
        CollectionAssert.AreEquivalent(enumPropertyNames, modelClassPropertyNames, "CarbonAwareParameters class properties do not match expected enums");
    }
}
