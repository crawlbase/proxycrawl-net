using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace ProxyCrawl
{
    public class API
    {
        #region Constants

        private const string INVALID_TOKEN = "Token is required";
        private const string INVALID_URL = "URL is required";

        #endregion

        #region Properties

        public string Token { get; protected set; }

        public string Body { get; protected set; }

        public string StatusCode { get; protected set; }

        public string OriginalStatus { get; protected set; }

        public string ProxyCrawlStatus { get; protected set; }

        public string URL { get; protected set; }

        public HttpMessageHandler HttpMessageHandler { get; set; }

        #endregion

        #region Constructors

        public API(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception(INVALID_TOKEN);
            }
            Token = token;
        }

        #endregion

        #region Methods

        public virtual async Task GetAsync(string url, IDictionary<string, object> options = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception(INVALID_URL);
            }
            if (options == null)
            {
                options = new Dictionary<string, object>();
            }
            string format = null;
            if (options.ContainsKey("format") && options["format"] != null)
            {
                format = options["format"].ToString();
            }
            var uri = PrepareUri(url, options);
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync(uri);
                await PrepareResponse(response, format);
            }
        }

        public virtual async Task PostAsync(string url, IDictionary<string, object> data = null, IDictionary<string, object> options = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception(INVALID_URL);
            }
            if (options == null)
            {
                options = new Dictionary<string, object>();
            }
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }
            string format = null;
            if (options.ContainsKey("format") && options["format"] != null)
            {
                format = options["format"].ToString();
            }
            var uri = PrepareUri(url, options);
            HttpContent content = null;
            if (format == "json")
            {
                string json = JsonSerializer.Serialize(data);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            else
            {
                var nameValueCollection = new List<KeyValuePair<string, string>>();
                foreach (var key in data.Keys)
                {
                    nameValueCollection.Add(new KeyValuePair<string, string>(key, data[key].ToString()));
                }
                var formContent = new FormUrlEncodedContent(nameValueCollection.ToArray());
                content = formContent;
            }
            using (var client = CreateHttpClient())
            {
                var response = await client.PostAsync(uri, content);
                await PrepareResponse(response, format);
            }
        }

        #endregion

        #region Helper Methods

        protected virtual HttpClient CreateHttpClient()
        {
            if (HttpMessageHandler != null)
            {
                return new HttpClient(HttpMessageHandler);
            }
            return new HttpClient();
        }

        protected virtual string GetBaseUrl()
        {
            return "https://api.proxycrawl.com";
        }

        private Uri PrepareUri(string url, IDictionary<string, dynamic> options)
        {
            if (options.ContainsKey("url"))
            {
                options.Remove("url");
            }
            options["url"] = Uri.EscapeDataString(url);
            if (options.ContainsKey("token"))
            {
                options.Remove("token");
            }
            options["token"] = Token;
            var uriBuilder = new UriBuilder(GetBaseUrl());
            var query = string.Join('&', (from key in options.Keys select $"{key}={options[key]}").ToArray());
            uriBuilder.Query = query;
            return uriBuilder.Uri;
        }

        protected virtual async Task PrepareResponse(HttpResponseMessage response, string format)
        {
            object body = await ReadResponseBody(response);
            StatusCode = ((int)response.StatusCode).ToString();

            if (format == "json")
            {
                JsonElement jsonBody = JsonSerializer.Deserialize<dynamic>(body.ToString());
                ExtractJsonBody(jsonBody);
            }
            else
            {
                ExtractResponseBody(response, body);
            }
        }

        protected virtual async Task<object> ReadResponseBody(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }

        protected virtual void ExtractJsonBody(JsonElement jsonBody)
        {
            OriginalStatus = jsonBody.GetProperty("original_status").ToString();
            ProxyCrawlStatus = jsonBody.GetProperty("pc_status").ToString();
            URL = jsonBody.GetProperty("url").ToString();
            Body = jsonBody.GetProperty("body").ToString();
        }

        protected virtual void ExtractResponseBody(HttpResponseMessage response, object body)
        {
            IEnumerable<string> originalStatusEnumerable;
            response.Headers.TryGetValues("original_status", out originalStatusEnumerable);
            if (originalStatusEnumerable != null && originalStatusEnumerable.Count() > 0)
            {
                OriginalStatus = originalStatusEnumerable.FirstOrDefault();
            }

            IEnumerable<string> pc_statusEnumerable = null;
            response.Headers.TryGetValues("pc_status", out pc_statusEnumerable);
            if (pc_statusEnumerable != null && pc_statusEnumerable.Count() > 0)
            {
                ProxyCrawlStatus = pc_statusEnumerable.FirstOrDefault();
            }

            IEnumerable<string> urlEnumerable;
            response.Headers.TryGetValues("url", out urlEnumerable);
            if (urlEnumerable != null && urlEnumerable.Count() > 0)
            {
                URL = urlEnumerable.FirstOrDefault();
            }

            Body = body.ToString();
        }

        #endregion
    }
}
