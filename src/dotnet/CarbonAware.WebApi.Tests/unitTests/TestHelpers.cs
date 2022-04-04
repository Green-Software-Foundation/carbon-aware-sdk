namespace CarbonAware.WepApi.UnitTests;

using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

/// <summary>
/// Tests helpers for all WebAPI specific tests.
/// </summary>
public static class TestHelpers
{
    public static void AssertStatusCode(IActionResult result, int code)
    {
        if (result is not ObjectResult obj)
        {
            // Certain result, like NoContent(), will only return a StatusCodeResult
            // object instead of the full ObjectResult so we handle here.
            var statusCodeResult = result as StatusCodeResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.IsTrue(statusCodeResult!.StatusCode == code);
        }
        else
        {
            Assert.IsTrue(obj!.StatusCode == code);
        }
    }
}
