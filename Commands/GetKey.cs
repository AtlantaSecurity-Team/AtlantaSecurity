using CommandSystem;
using Exiled.API.Features;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AtlantaSecurity.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class GetKey : ICommand
    {
        public string Command { get; } = "getkey";
        public string[] Aliases { get; } = { "getkey" };
        public string Description { get; } = "Retrieve or generate a key for the server.";
        public bool SanitizeResponse => true;

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10) // Imposta un timeout di 10 secondi
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = string.Empty;

            if (arguments.Count < 1)
            {
                response = "Usage: getkey <IP>";
                return false;
            }

            string serverIp = arguments.At(0);

            if (string.IsNullOrWhiteSpace(serverIp))
            {
                response = "IP mancante";
                return false;
            }

            if (!Server.IsVerified)
            {
                response = "Il server non è verificato, AtlantaSecurity è un plugin riservato ai server verificati da Northwood Studios.\nPer verificare il tuo server usa il comando !verify oppure invia una mail a server.verification@scpslgame.com";
                return false;
            }

            try
            {
                var key = FetchOrGenerateKey(serverIp);

                if (key != null)
                {
                    response = $"Chiave per IP {serverIp}: {key}";
                    return true;
                }
                else
                {
                    response = "Errore nella richiesta della chiave.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Errore durante l'esecuzione del comando getkey: {ex.Message}");
                response = "Errore del server.";
                return false;
            }
        }


        private static string FetchOrGenerateKey(string serverIp)
        {
            string url = "http://sl.lunarscp.it:4000/generate-key";
            string requestBody = $"{{\"ip\": \"{serverIp}\"}}"; // Forma corretta del JSON

            try
            {
                // Crea la richiesta HTTP sincrona
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/json";

                // Scrivi il corpo della richiesta
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestBody);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                // Ottieni la risposta
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            var responseText = streamReader.ReadToEnd();
                            // Assicurati che la risposta JSON contenga la chiave
                            var jsonResponse = JObject.Parse(responseText);
                            return jsonResponse["key"]?.ToString();
                        }
                    }
                    else
                    {
                        Log.Error($"Errore nella risposta del server: {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (WebException ex)
            {
                using (var responseStream = ex.Response?.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var errorText = reader.ReadToEnd();
                            Log.Error($"Errore durante la richiesta della chiave: {ex.Message}, Risposta del server: {errorText}");
                        }
                    }
                }
                return null;
            }
        }


    }
}
