using System.Diagnostics;
using System.Net.Http;

namespace greenguard_hub_client
{
    public class GreenGuardHttpClient
    {
        static readonly HttpClient _httpClient = new();

        public void SendHealthCheck(string endpoint, string hubId)
        {
            try
            {
                var path = endpoint + "/api/hubs/" + hubId + "/health";
                using var response = _httpClient.Get(path);
                response.EnsureSuccessStatusCode();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Could not send health check: " + ex.InnerException.Message);
            }
        }
    }
}
