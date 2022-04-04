namespace CarbonAware.WepApi.UnitTests
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests helpers for all WebAPI specific tests.
    /// </summary>
    public static class TestHelpers
    {
        public static void AssertStatusCode(IActionResult result, int code)
        {
            var obj = result as ObjectResult;

            //Assert
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.StatusCode == code);
        }
    }
}
