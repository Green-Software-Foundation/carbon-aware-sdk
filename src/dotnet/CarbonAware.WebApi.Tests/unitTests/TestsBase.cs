namespace CarbonAware.WepApi.UnitTests
{
    using CarbonAware.Model;
    using CarbonAware.WebApi.Controllers;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// TestsBase for all WebAPI specific tests.
    /// </summary>
    public abstract class TestsBase
    {
        protected TestsBase()
        {
            this.MockRepository = new MockRepository(MockBehavior.Strict);
            this.MockLogger = new Mock<ILogger<CarbonAwareController>>();
            this.MockPlugin = new Mock<ICarbonAware>();
        }

        protected Mock<ILogger<CarbonAwareController>> MockLogger { get; }
        protected Mock<ICarbonAware> MockPlugin { get; }

        protected MockRepository MockRepository { get; }

        protected void SetupPluginWithData(List<EmissionsData> data) =>
            this.MockPlugin.Setup(x =>
                x.GetEmissionsDataAsync(
                    It.IsAny<Dictionary<string, object>>())).ReturnsAsync(data);

        protected void SetupPluginWithException() =>
            this.MockPlugin.Setup(x =>
                x.GetEmissionsDataAsync(
                    It.IsAny<Dictionary<string, object>>())).Throws<Exception>();

        [TestCleanup]
        public void TestCleanup() => this.MockRepository.VerifyAll();
    }
}
