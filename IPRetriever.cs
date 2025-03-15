namespace IPAlert
{
    /// <summary>
    /// Class for retrieving IP addresses
    /// </summary>
    public class IPRetriever
    {
        public const string NO_CONNECTION_STRING = "No Connection";

        private const int MAX_RETRIES = 3;
        private static readonly HttpClient _httpClient = new HttpClient();
        private Logger _logger;

        public IPRetriever(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Attempts to get the public ip address
        /// </summary>
        /// <returns>The IPAddress if retrieved or "No Connection" if unable to get the IPAddress.</returns>
        public async Task<string> GetPublicIPAddress()
        {
            int retryDelayMS = 1000;
            int retryCount = 0;

            while (retryCount < MAX_RETRIES)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync("https://api.ipify.org");
                    int statusCode = (int)response.StatusCode;

                    if (statusCode >= 400 && statusCode < 500)
                    {
                        // Don't retry on 4XX errors
                        _logger.Error($"Unsuccessful status code while getting public IP Address from ipify. Status Code: {statusCode}.");
                        return NO_CONNECTION_STRING;
                    }

                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    _logger.Error("Exception while running GetPublicIPAddress.", e);
                }

                retryCount++;
                if (retryCount < MAX_RETRIES)
                {
                    Thread.Sleep(retryDelayMS);
                }
            }

            return NO_CONNECTION_STRING;
        }
    }
}
