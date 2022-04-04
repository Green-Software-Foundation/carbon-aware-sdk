namespace CarbonAware.WepApi.UnitTests
{
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
}
