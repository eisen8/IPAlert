namespace IPAlert
{
    /// <summary>
    /// Class for retrieving IP addresses
    /// </summary>
    public class IPRetriever
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const int MaxRetries = 5;
        private Logger _logger;

        public IPRetriever(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Attempts to get the public ip address
        /// </summary>
        /// <returns>The IPAddress if retrieved or "Error" if there was an error.</returns>
        public async Task<string> GetPublicIPAddress()
        {
            int delayMS = 3000;
            int retryCount = 0;

            while (retryCount < MaxRetries)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.GetAsync("https://api.ipify.org");
                    int statusCode = (int)response.StatusCode;

                    if (statusCode >= 400 && statusCode < 500)
                    {
                        // Don't retry on 4XX errors
                        _logger.Error($"Unsuccessful status code while getting public IP Address from ipify. Status Code: {statusCode}.");
                        return "Error";
                    }

                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    _logger.Error("Exception while running GetPublicIPAddress.", e);
                }

                retryCount++;
                if (retryCount < MaxRetries)
                {
                    Thread.Sleep(delayMS);
                }
            }

            return "Error";
        }
    }
}
