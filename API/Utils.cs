using Exiled.API.Features;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;

namespace AtlantaSecurity.API
{
    public static class Utils
    {
        // Inizializzazione statica dell'HttpClient
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _apiUrl = "http://sl.lunarscp.it:4000/check-ip";

        public static bool ValidateKeyByIp(string serverIp)
        {
            try
            {
                // Crea il corpo della richiesta come JSON
                var requestBody = new
                {
                    ip = serverIp
                };

                var content = new StringContent(
                    JObject.FromObject(requestBody).ToString(),
                    Encoding.UTF8,
                    "application/json"
                );

                // Crea la richiesta HTTP POST
                var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
                {
                    Content = content
                };

                // Invia la richiesta in modo sincrono utilizzando .Result
                HttpResponseMessage response = _httpClient.SendAsync(request).Result;

                // Verifica se la risposta ha avuto successo
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
