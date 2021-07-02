using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace ProxyCrawl
{
    public class ScraperAPI : API
    {

        #region Properties

        public int RemainingRequests { get; private set; }

        #endregion

        #region Constructors

        public ScraperAPI(string token) : base(token)
        {
        }

        #endregion

        #region Methods

        public override Task PostAsync(string url, IDictionary<string, object> data = null, IDictionary<string, object> options = null)
        {
            throw new Exception("Only GET is allowed for the ScraperAPI");
        }

        #endregion

        #region Helper Methods

        protected override string GetBaseUrl()
        {
            return "https://api.proxycrawl.com/scraper";
        }

        protected override async Task PrepareResponse(HttpResponseMessage response, string format)
        {
            await base.PrepareResponse(response, "json");
        }

        protected override void ExtractJsonBody(JsonElement jsonBody)
        {
            base.ExtractJsonBody(jsonBody);
            var remainingRequests = 0;
            var remainingRequestsString = jsonBody.GetProperty("remaining_requests").ToString();
            int.TryParse(remainingRequestsString, out remainingRequests);
            RemainingRequests = remainingRequests;
        }

        #endregion
    }
}
