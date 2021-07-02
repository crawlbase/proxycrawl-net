using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

using NUnit.Framework;
using RichardSzalay.MockHttp;

using ProxyCrawl;

namespace ProxyCrawlTest
{
    [TestFixture]
    public class APITest
    {
        [Test]
        public void ItRequiresTokenOnInitialization()
        {
            Assert.Throws<Exception>(delegate
            {
                new API(null);
            }, "Token is required");
            Assert.Throws<Exception>(delegate
            {
                new API(string.Empty);
            }, "Token is required");
        }

        [Test]
        public void ItAssignsTokenToProperty()
        {
            var api = new API("testtoken");
            Assert.AreEqual(api.Token, "testtoken");
        }

        [Test]
        public void ItRequiresUrlOnGet()
        {
            var api = new API("testtoken");
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
        public void ItRequiresUrlOnPost()
        {
            var api = new API("testtoken");
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await api.PostAsync(null);
            });
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await api.PostAsync(string.Empty);
            });
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
            var mockBody = "<!doctype html><html><head><title>Test</title></head><body><h1>Test</h1></body></html>";
            mockHttp.Expect("https://api.proxycrawl.com")
                    .WithQueryString("token", "testtoken")
                    .WithQueryString("url", "https://www.apple.com")
                    .WithQueryString("user_agent", "TestAgent")
                    .WithQueryString("page_wait", "1000")
                    .WithQueryString("ajax_wait", "true")
                    .Respond(HttpStatusCode.OK, headers.ToArray(), "text/html", mockBody);
            var api = new API("testtoken");
            api.HttpMessageHandler = mockHttp;
            await api.GetAsync("https://www.apple.com", new Dictionary<string, object>() {
                {"user_agent", "TestAgent"},
                {"page_wait", "1000"},
                {"ajax_wait", "true"},
            });
            Assert.AreEqual(api.Body, "<!doctype html><html><head><title>Test</title></head><body><h1>Test</h1></body></html>");
            Assert.AreEqual(api.OriginalStatus, "201");
            Assert.AreEqual(api.ProxyCrawlStatus, "202");
            Assert.AreEqual(api.URL, "https://www.apple.com");
        }

        [Test]
        public async Task ItSendsJsonGetRequestToProxyCrawlServer()
        {
            var mockHttp = new MockHttpMessageHandler();
            var mockBody = @"{
  ""original_status"": ""201"",
  ""pc_status"": ""202"",
  ""url"": ""https://www.apple.com"",
  ""body"": ""<!doctype html><html><head><title>Test</title></head><body><h1>Test</h1></body></html>""
}";
            mockHttp.Expect("https://api.proxycrawl.com")
                    .WithQueryString("token", "testtoken")
                    .WithQueryString("url", "https://www.apple.com")
                    .WithQueryString("format", "json")
                    .Respond(HttpStatusCode.OK, "text/html", mockBody);
            var api = new API("testtoken");
            api.HttpMessageHandler = mockHttp;
            await api.GetAsync("https://www.apple.com", new Dictionary<string, object>() {
                {"format", "json"},
            });
            Assert.AreEqual(api.Body, "<!doctype html><html><head><title>Test</title></head><body><h1>Test</h1></body></html>");
            Assert.AreEqual(api.OriginalStatus, "201");
            Assert.AreEqual(api.ProxyCrawlStatus, "202");
            Assert.AreEqual(api.URL, "https://www.apple.com");
        }

        [Test]
        public async Task ItSendsPostRequestToProxyCrawlServer()
        {
            var mockHttp = new MockHttpMessageHandler();
            var headers = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("skip_normalize", "true"),
                new KeyValuePair<string, string>("original_status", "201"),
                new KeyValuePair<string, string>("pc_status", "202"),
                new KeyValuePair<string, string>("url", "https://www.apple.com"),
            };
            var mockBody = "<!doctype html><html><head><title>Test</title></head><body><h1>Test</h1></body></html>";
            mockHttp.Expect("https://api.proxycrawl.com")
                    .WithQueryString("token", "testtoken")
                    .WithQueryString("url", "https://www.apple.com")
                    .WithQueryString("user_agent", "TestAgent")
                    .WithQueryString("page_wait", "1000")
                    .WithQueryString("ajax_wait", "true")
                    .WithFormData("field1", "value1")
                    .WithFormData("field2", "value2")
                    .Respond(HttpStatusCode.OK, headers.ToArray(), "text/html", mockBody);
            var api = new API("testtoken");
            api.HttpMessageHandler = mockHttp;
            await api.PostAsync("https://www.apple.com", new Dictionary<string, object>() {
                {"field1", "value1"},
                {"field2", "value2"},
            }, new Dictionary<string, object>() {
                {"user_agent", "TestAgent"},
                {"page_wait", "1000"},
                {"ajax_wait", "true"},
            });
            Assert.AreEqual(api.Body, "<!doctype html><html><head><title>Test</title></head><body><h1>Test</h1></body></html>");
            Assert.AreEqual(api.OriginalStatus, "201");
            Assert.AreEqual(api.ProxyCrawlStatus, "202");
            Assert.AreEqual(api.URL, "https://www.apple.com");
        }

        [Test]
        public async Task ItSendsJsonPostRequestToProxyCrawlServer()
        {
            var mockHttp = new MockHttpMessageHandler();
            var mockBody = @"{
  ""original_status"": ""201"",
  ""pc_status"": ""202"",
  ""url"": ""https://www.apple.com"",
  ""body"": ""<!doctype html><html><head><title>Test</title></head><body><h1>Test</h1></body></html>""
}";
            mockHttp.Expect("https://api.proxycrawl.com")
                    .WithQueryString("token", "testtoken")
                    .WithQueryString("url", "https://www.apple.com")
                    .WithQueryString("format", "json")
                    .WithContent("{\"field1\":\"value1\",\"field2\":\"value2\"}")
                    .Respond(HttpStatusCode.OK, "text/html", mockBody);
            var api = new API("testtoken");
            api.HttpMessageHandler = mockHttp;
            await api.PostAsync("https://www.apple.com", new Dictionary<string, object>() {
                {"field1", "value1"},
                {"field2", "value2"},
            }, new Dictionary<string, object>() {
                {"format", "json"},
            });
            Assert.AreEqual(api.Body, "<!doctype html><html><head><title>Test</title></head><body><h1>Test</h1></body></html>");
            Assert.AreEqual(api.OriginalStatus, "201");
            Assert.AreEqual(api.ProxyCrawlStatus, "202");
            Assert.AreEqual(api.URL, "https://www.apple.com");
        }
    }
}
