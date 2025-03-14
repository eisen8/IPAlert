using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace IPAlert
{
    /// <summary>
    /// Class for retrieving IP addresses
    /// </summary>
    public class IPRetriever
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const int MaxRetries = 5;

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

                    if (((int)response.StatusCode) >= 400 && ((int)response.StatusCode) < 500)
                    {
                        // Don't retry on 4XX errors
                        return "Error";
                    }

                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
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
