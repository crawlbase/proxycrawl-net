using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace ProxyCrawl
{
    public class LeadsAPI
    {
        #region Constants

        private const string INVALID_TOKEN = "Token is required";
        private const string INVALID_DOMAIN = "Domain is required";

        #endregion

        #region Properties
        
        public string Token { get; private set; }

        public string Body { get; private set; }

        public string StatusCode { get; private set; }

        #endregion

        #region Constructors

        public LeadsAPI(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception(INVALID_TOKEN);
            }
            Token = token;
        }

        #endregion

        #region Methods

        public virtual async Task GetAsync(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                throw new Exception(INVALID_DOMAIN);
            }
            var uriBuilder = new UriBuilder("https://api.proxycrawl.com/leads");
            var query = $"token={Uri.EscapeDataString(Token)}&domain={Uri.EscapeDataString(domain)}";
            uriBuilder.Query = query;
            var uri = uriBuilder.Uri;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                Body = await response.Content.ReadAsStringAsync();
                StatusCode = ((int)response.StatusCode).ToString();
            }

        }

        #endregion
    }
}
