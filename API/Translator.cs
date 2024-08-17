using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AtlantaSecurity.API;
using Newtonsoft.Json.Linq;

namespace AtlantaSecurity.API
{
    internal class Translator
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public Translator()
        {
            _apiKey = ConfigLoader.LoadDeepLApiKey(); // Recupera la chiave API dal file di configurazione
            _httpClient = new HttpClient();
        }

        public async Task<string> TranslateToEnglish(string text)
        {
            var url = "https://api-free.deepl.com/v2/translate";

            // Prepara il contenuto della richiesta
            var requestBody = new
            {
                text = text,
                target_lang = "EN",
                auth_key = _apiKey
            };

            var content = new StringContent(
                JObject.FromObject(requestBody).ToString(),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                // Imposta un timeout per la richiesta
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.PostAsync(url, content, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseBody);

                    // Estrai e ritorna il testo tradotto
                    var translatedText = jsonResponse["translations"][0]["text"].ToString();
                    return translatedText;
                }
                else
                {
                    throw new Exception($"Translation API request failed with status code {response.StatusCode}");
                }
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Translation request timed out.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during translation: {ex.Message}");
            }
        }
    }
}
