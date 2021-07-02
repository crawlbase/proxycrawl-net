using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using NUnit.Framework;
using RichardSzalay.MockHttp;

using ProxyCrawl;
using System.Net.Http;

namespace ProxyCrawlTest
{
    [TestFixture]
    public class ScreenshotsAPITest
    {
        [Test]
        public void ItRequiresTokenOnInitialization()
        {
            Assert.Throws<Exception>(delegate
            {
                new ScreenshotsAPI(null);
            }, "Token is required");
            Assert.Throws<Exception>(delegate
            {
                new ScreenshotsAPI(string.Empty);
            }, "Token is required");
        }

        [Test]
        public void ItAssignsTokenToProperty()
        {
            var api = new ScreenshotsAPI("testtoken");
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
            }, "Only GET is allowed for the ScreenshotsAPI");
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await api.PostAsync("https://www.apple.com");
            }, "Only GET is allowed for the ScreenshotsAPI");
        }

        [Test]
        public async Task ItSendsGetRequestToProxyCrawlServer()
        {
            var mockHttp = new MockHttpMessageHandler();
            var headers = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("skip_normalize", "true"),
                new KeyValuePair<string, string>("original_status", "201"),
                new KeyValuePair<string, string>("pc_status", "202"),
                new KeyValuePair<string, string>("url", "https://www.apple.com"),
            };
            mockHttp.Expect("https://api.proxycrawl.com/screenshots")
                    .WithQueryString("token", "testtoken")
                    .WithQueryString("url", "https://www.apple.com")
                    .WithQueryString("user_agent", "TestAgent")
                    .WithQueryString("page_wait", "1000")
                    .WithQueryString("ajax_wait", "true")
                    .Respond(HttpStatusCode.OK, headers.ToArray(), "text/html", (HttpRequestMessage requestMessage) =>
                    {
                        var imagePath = Path.GetFullPath("./../../../images/test.jpg");
                        MemoryStream memoryStream = null;
                        using (var fs = File.OpenRead(imagePath))
                        {
                            byte[] tmpBytes = new byte[fs.Length];
                            fs.Read(tmpBytes, 0, Convert.ToInt32(fs.Length));
                            memoryStream = new MemoryStream(tmpBytes);
                        }
                        return memoryStream;
                    });
            var api = new ScreenshotsAPI("testtoken");
            api.HttpMessageHandler = mockHttp;
            await api.GetAsync("https://www.apple.com", new Dictionary<string, object>() {
                {"save_to_path", @".\test.jpg"},
                {"user_agent", "TestAgent"},
                {"page_wait", "1000"},
                {"ajax_wait", "true"},
            });

            Assert.AreEqual(api.Body, Convert.ToBase64String(File.ReadAllBytes(@".\test.jpg")));
            Assert.AreEqual(api.OriginalStatus, "201");
            Assert.AreEqual(api.ProxyCrawlStatus, "202");
            Assert.AreEqual(api.URL, "https://www.apple.com");

            File.Delete(@".\test.jpg");
        }
    }
}
