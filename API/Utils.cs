using Exiled.API.Features;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;

namespace AtlantaSecurity.API
{
    public static class Utils
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _apiUrl = "http://sl.lunarscp.it:4000/check-ip";

        public static bool ValidateKeyByIp(string serverIp)
        {
            try
            {
                var requestBody = new
                {
                    ip = serverIp
                };

                var content = new StringContent(
                    JObject.FromObject(requestBody).ToString(),
                    Encoding.UTF8,
                    "application/json"
                );

                var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
                {
                    Content = content
                };

                HttpResponseMessage response = _httpClient.SendAsync(request).Result;

                return response.IsSuccessStatusCode;

            }
            catch (Exception ex)
            {
                Log.Error("Error during IP validation: " + ex.Message);
                return false;
            }
        }
    }
}
