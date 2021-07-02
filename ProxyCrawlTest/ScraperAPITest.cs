using System;
using NUnit.Framework;

using ProxyCrawl;

namespace ProxyCrawlTest
{
    [TestFixture]
    public class ScraperAPITest
    {
        [Test]
        public void ItRequiresTokenOnInitialization()
        {
            Assert.Throws<Exception>(delegate
            {
                new ScraperAPI(null);
            }, "Token is required");
            Assert.Throws<Exception>(delegate
            {
                new ScraperAPI(string.Empty);
            }, "Token is required");
        }

        [Test]
        public void ItAssignsTokenToProperty()
        {
            var api = new ScraperAPI("testtoken");
            Assert.AreEqual(api.Token, "testtoken");
        }

        [Test]
        public void ItRequiresUrlOnGet()
        {
            var api = new ScraperAPI("testtoken");
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await api.GetAsync(null);
            }, "URL is required");
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await api.GetAsync(string.Empty);
            }, "URL is required");
        }

        [Test]
        public void ItDoesntAllowPost()
        {
            var api = new ScraperAPI("testtoken");
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await api.PostAsync("https://www.apple.com");
            }, "Only GET is allowed for the ScraperAPI");
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await api.PostAsync("https://www.apple.com");
            }, "Only GET is allowed for the ScraperAPI");
        }
    }
}
