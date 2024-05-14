using CarbonAware.Configuration;

namespace CarbonAware.Tests.Configuration;

class EmissionsDataCacheConfigurationTests
{
    [TestCase(10, TestName = "AssertValid: ExpirationMin greater than 0")]
    public void AssertValid_ExpirationMinGreaterThanOrEqualsToZero_DoesNotThrowException(int expirationMin)
    {
        EmissionsDataCacheConfiguration emissionsDataCacheConfig = new EmissionsDataCacheConfiguration()
        {
            Enabled = true,
            ExpirationMin = expirationMin
        };
        
        Assert.DoesNotThrow(() => emissionsDataCacheConfig.AssertValid());
    }

    [TestCase(0, TestName = "AssertValid: ExpirationMin equals to 0")]
    [TestCase(-10, TestName = "AssertValid: ExpirationMin less than 0")]
    public void AssertValid_ExpirationMinLessThanZero_ThrowException(int expirationMin)
    {
        EmissionsDataCacheConfiguration emissionsDataCacheConfig = new EmissionsDataCacheConfiguration()
        {
            Enabled = true,
            ExpirationMin = expirationMin
        };
        
        Assert.Throws<ArgumentException>(() => emissionsDataCacheConfig.AssertValid());
    }
}
