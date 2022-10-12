namespace CarbonAware.WepApi.UnitTests;

using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Net;

/// <summary>
/// Tests helpers for all WebAPI specific tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Helper function to assert the HTTP status code of an action result
    /// </summary>
    /// <param name="result">Action result. </param>
    /// <param name="code">Expected HTTP status code. </param>
    /// <remarks>Certain results, like NoContent(), return a StatusCodeResult
    /// instead of an ObjectResult so we handle both here.</remarks>
    public static void AssertStatusCode(IActionResult? result, HttpStatusCode code)
    {
        Assert.IsNotNull(result);
        if (result is not ObjectResult obj)
        {
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual((int)code, statusCodeResult!.StatusCode);
        }
        else
        {
            Assert.AreEqual((int)code, obj.StatusCode);
        }
    }
}
