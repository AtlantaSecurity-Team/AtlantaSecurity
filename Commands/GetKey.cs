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
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class GetKey : ICommand
    {
        public string Command { get; } = "getkey";
        public string[] Aliases { get; } = { "getkey" };
        public string Description { get; } = "Retrieve or generate a key for the server.";
        public bool SanitizeResponse => true;

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10) 
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {

            string serverIp = Server.IpAddress;  

            if (string.IsNullOrWhiteSpace(serverIp))
            {
                response = "Failed to retrieve the server's IP address.";
                return false;
            }

            if (!Server.IsVerified)
            {
                response = "This server is not verified. AtlantaSecurity is a plugin reserved for servers verified by Northwood Studios.\nTo verify your server, use the command !verify or send an email to server.verification@scpslgame.com";
                return false;
            }

            try
            {
                // Fetch or generate the key using the server's IP
                var key = FetchOrGenerateKey(serverIp);

                if (key != null)
                {
                    response = $"Successfully obtained key for this IP: {serverIp}";
                    return true;
                }
                else
                {
                    response = "Error while requesting the key.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error executing the getkey command: {ex.Message}");
                response = "Server error.";
                return false;
            }
        }

  
        private static string FetchOrGenerateKey(string serverIp)
        {
            string url = "http://sl.lunarscp.it:4000/generate-key";
            string requestBody = $"{{\"ip\": \"{serverIp}\"}}"; 

            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/json";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestBody);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            var responseText = streamReader.ReadToEnd();
                            var jsonResponse = JObject.Parse(responseText);
                            return jsonResponse["key"]?.ToString();
                        }
                    }
                    else
                    {
                        Log.Error($"Server response error: {response.StatusCode}");
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
                            Log.Error($"Error while requesting the key: {ex.Message}, Server response: {errorText}");
                        }
                    }
                }
                return null;
            }
        }
    }
}
