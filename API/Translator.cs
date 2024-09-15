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
            var targetLanguage = Main.Instance.Config.Language;

             if (targetLanguage == "italiano")
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
            var url = "http://sl.lunarscp.it:4000/translate"; 

            var requestBody = new
            {
                text = text,
                targetLang = "IT"
            };

            var content = new StringContent(
                JObject.FromObject(requestBody).ToString(),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = _httpClient.PostAsync(url, content).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var jsonResponse = JObject.Parse(responseBody);

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
