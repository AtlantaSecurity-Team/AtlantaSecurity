using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AtlantaSecurity.API
{
    internal class Translator
    {
        private readonly HttpClient _httpClient;

        public Translator()
        {
            _httpClient = new HttpClient();
        }

        public string Translate(string text)
        {
            var targetLanguage = Main.Instance.Config.Language; // Recupera la lingua dal file di configurazione
            if (targetLanguage == "english")
            {
                return text; // Nessuna traduzione necessaria se la lingua è già inglese
            }
            else if (targetLanguage == "italiano")
            {
                return TranslateToItalian(text);
            }
            else
            {
                throw new Exception("Language configuration is not valid.");
            }
        }

        private string TranslateToItalian(string text)
        {
            var url = "http://localhost:4000/translate"; // URL del webserver

            var requestBody = new
            {
                text = text,
                targetLang = "IT" // Lingua target per la traduzione
            };

            var content = new StringContent(
                JObject.FromObject(requestBody).ToString(),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                // Esegui la richiesta sincrona
                var response = _httpClient.PostAsync(url, content).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var jsonResponse = JObject.Parse(responseBody);

                    // Estrai e ritorna il testo tradotto
                    var translatedText = jsonResponse["translatedText"].ToString();
                    return translatedText;
                }
                else
                {
                    throw new Exception($"Translation API request failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during translation: {ex.Message}");
            }
        }
    }
}
