using CommandSystem;
using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using AtlantaSecurity.API;
using static AtlantaSecurity.Commands.UserInfo;

namespace AtlantaSecurity.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class GlobalSearch : ICommand
    {
        public string Command { get; } = "globalsearch";
        public string[] Aliases { get; } = { "gsearch" };
        public string Description { get; } = "Searches the global database for players and retrieves their information.";
        public bool SanitizeResponse => true;

        private readonly Translator _translator = new Translator();

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Server.IsVerified)
            {
                response = _translator.Translate("The server is not verified, AtlantaSecurity is a plugin reserved for servers verified by Northwood Studios.\nTo verify your server use the !verify command or send an email to server.verification@scpslgame.com");
                return false;
            }
            if(!API.Utils.ValidateKeyByIp(Server.IpAddress))
            {
                response = _translator.Translate("The server is not authorized to use this command. Use the getKey command to get a key.");
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

            response = _translator.Translate($"\n<color=red>[ATLANTA-BLACKLIST]</color>\nNumber of players found: {count}\n" +
                       $"Details:\n" +
                       string.Join("\n", results));

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
                }

                var response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonString = streamReader.ReadToEnd();
                        if (string.IsNullOrEmpty(jsonString))
                        {
                            Log.Debug("Empty or null JSON response.");
                            return null;
                        }

                        try
                        {
                            var playerInfo = JsonConvert.DeserializeObject<PlayerInfo>(jsonString);
                            Log.Debug("Player info successfully retrieved from the server.");
                            return playerInfo;
                        }
                        catch (JsonException ex)
                        {
                            Log.Debug($"Deserialization error: {ex.Message} - {ex.StackTrace}");
                        }
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
    }
}
