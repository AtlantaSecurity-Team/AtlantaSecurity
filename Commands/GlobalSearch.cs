using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;

namespace AtlantaSecurity.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class GlobalSearch : ICommand
    {
        public string Command { get; } = "globalsearch";
        public string[] Aliases { get; } = { "globalsearch" };
        public string Description { get; } = "Searches the global database for players and retrieves their information.";
        public bool SanitizeResponse => true;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Server.IsVerified)
            {
                response = "Il server non è verificato, AtlantaSecurity è un plugin riservato ai server verificati da Northwood Studios.\nPer verificare il tuo server usa il comando !verify oppure invia una mail a server.verification@scpslgame.com";
                return false;
            }

            Log.Debug("Executing global search command.");

            var allPlayers = Player.List;
            int count = 0;
            List<string> results = new List<string>();

            foreach (var player in allPlayers)
            {
                var playerInfo = GetPlayerInfoFromServer(player.UserId);

                if (playerInfo != null)
                {
                    count++;
                    results.Add($"Player: {player.Nickname}\n" +
                                $"UserID: {playerInfo.UserID}\n" +
                                $"Reason: {playerInfo.Reason}\n" +
                                $"Level: {playerInfo.Level}\n" +
                                $"Expiry Date: {(playerInfo.ExpiryDate.HasValue ? playerInfo.ExpiryDate.Value.ToString("yyyy-MM-dd") : "Permanent")}\n");
                }
            }

            response = $"\n<color=red>[ATLANTA-BLACKLIST]</color>\nNumber of players found: {count}\n" +
                       $"Details:\n" +
                       string.Join("\n", results);

            Log.Debug($"Global search completed. Number of players found: {count}");
            return true;
        }

        private PlayerInfo GetPlayerInfoFromServer(string userId)
        {
            Log.Debug("Requesting player info from the external server...");

            try
            {
                var request = WebRequest.Create("http://sl.lunarscp.it:4000/get-player-data") as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/json";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string json = $"{{\"userId\":\"{userId}\"}}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonString = streamReader.ReadToEnd();
                        var playerInfo = JsonSerializer.Deserialize<PlayerInfo>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        Log.Debug("Player info successfully retrieved from the server.");
                        return playerInfo;
                    }
                }
                else
                {
                    Log.Debug($"Failed to retrieve player info. Server response: {response.StatusCode}");
                }
            }
            catch (WebException ex)
            {
                Log.Debug($"Error during HTTP request: {ex.Message}");
            }

            return null;
        }

        internal class PlayerInfo
        {
            public string UserID { get; set; }
            public string Reason { get; set; }
            public string Level { get; set; }
            public DateTime? ExpiryDate { get; set; }
        }
    }
}
