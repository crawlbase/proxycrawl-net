using System;

using NUnit.Framework;

using ProxyCrawl;

namespace ProxyCrawlTest
{
    [TestFixture]
    public class LeadsAPITest
    {
        [Test]
        public void ItRequiresTokenOnInitialization()
        {
            Assert.Throws<Exception>(delegate
            {
                new LeadsAPI(null);
            }, "Token is required");
            Assert.Throws<Exception>(delegate
            {
                new LeadsAPI(string.Empty);
            }, "Token is required");
        }

        [Test]
        public void ItAssignsTokenToProperty()
        {
            var api = new LeadsAPI("testtoken");
            Assert.AreEqual(api.Token, "testtoken");
        }

        [Test]
        public void ItRequiresDomainOnGet()
        {
            var api = new LeadsAPI("testtoken");
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await api.GetAsync(null);
            }, "Domain is required");
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await api.GetAsync(string.Empty);
            }, "Domain is required");
        }
    }
}
