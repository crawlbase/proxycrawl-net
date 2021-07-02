using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ProxyCrawl
{
    public class ScreenshotsAPI : API
    {
        #region Constants

        private const string INVALID_SAVE_TO_PATH_FILENAME = "Filename must end with .jpg or .jpeg";
        private const string SAVE_TO_PATH_FILENAME_PATTERN = @".+\.(jpg|JPG|jpeg|JPEG)$";
        private const string SAVE_TO_PATH_KEY = "save_to_path";

        #endregion

        #region Properties

        public string ScreenshotPath { get; private set; }

        public bool IsSuccess { get; private set; }

        public int RemainingRequests { get; private set; }

        public string ScreenshotUrl { get; private set; }

        #endregion

        #region Constructors

        public ScreenshotsAPI(string token) : base(token)
        {
        }

        #endregion

        #region Methods

        public override Task PostAsync(string url, IDictionary<string, object> data = null, IDictionary<string, object> options = null)
        {
            throw new Exception("Only GET is allowed for the ScreenshotsAPI");
        }

        public override async Task GetAsync(string url, IDictionary<string, object> options = null)
        {
            if (options == null)
            {
                options = new Dictionary<string, object>();
            }
            string screenshotPath = null;
            if (options.ContainsKey(SAVE_TO_PATH_KEY))
            {
                screenshotPath = options[SAVE_TO_PATH_KEY].ToString();
                options.Remove(SAVE_TO_PATH_KEY);
            }
            else
            {
                screenshotPath = GenerateFilePath();
            }
            ScreenshotPath = screenshotPath;
            var regex = new Regex(SAVE_TO_PATH_FILENAME_PATTERN);
            if (!regex.IsMatch(screenshotPath))
            {
                throw new Exception(INVALID_SAVE_TO_PATH_FILENAME);
            }
            await base.GetAsync(url, options);
        }

        #endregion

        #region Helper Methods

        protected override string GetBaseUrl()
        {
            return "https://api.proxycrawl.com/screenshots";
        }

        protected override async Task PrepareResponse(HttpResponseMessage response, string format)
        {
            await base.PrepareResponse(response, null);
        }

        protected override async Task<object> ReadResponseBody(HttpResponseMessage response)
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                using (var fileStream = File.Create(ScreenshotPath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
            }
            byte[] bytes = File.ReadAllBytes(ScreenshotPath);
            return Convert.ToBase64String(bytes);
        }

        protected override void ExtractResponseBody(HttpResponseMessage response, object body)
        {
            base.ExtractResponseBody(response, body);

            IEnumerable<string> remainingRequestsEnumerable;
            response.Headers.TryGetValues("remaining_requests", out remainingRequestsEnumerable);
            if (remainingRequestsEnumerable != null && remainingRequestsEnumerable.Count() > 0)
            {
                int remainingRequests = 0;
                int.TryParse(remainingRequestsEnumerable.FirstOrDefault().ToString(), out remainingRequests);
                RemainingRequests = remainingRequests;
            }

            IEnumerable<string> successEnumerable;
            response.Headers.TryGetValues("success", out successEnumerable);
            if (successEnumerable != null && successEnumerable.Count() > 0)
            {
                bool isSuccess = false;
                bool.TryParse(successEnumerable.FirstOrDefault().ToString(), out isSuccess);
                IsSuccess = isSuccess;
            }

            IEnumerable<string> screenshotUrlEnumerable;
            response.Headers.TryGetValues("screenshot_url", out screenshotUrlEnumerable);
            if (screenshotUrlEnumerable != null && screenshotUrlEnumerable.Count() > 0)
            {
                ScreenshotUrl = screenshotUrlEnumerable.FirstOrDefault();
            }
        }

        private string GenerateFilename()
        {
            return $@"{Guid.NewGuid()}.jpg";
        }

        private string GenerateFilePath()
        {
            return Path.Combine(Path.GetTempPath(), GenerateFilename());
        }

        #endregion
    }
}
