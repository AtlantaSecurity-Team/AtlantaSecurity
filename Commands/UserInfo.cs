using CommandSystem;
using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using AtlantaSecurity.API;

namespace AtlantaSecurity.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class UserInfo : ICommand
    {
        public string Command { get; } = "userinfo";
        public string[] Aliases { get; } = { "userinfo" };
        public string Description { get; } = "Checks if a player is present in the database and retrieves their information.";
        public bool SanitizeResponse => true;

        private readonly Translator _translator = new Translator();

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = _translator.Translate("Usage: userinfo <ID/PlayerName>");
                Log.Debug("No arguments provided. Usage message sent.");
                return false;
            }

            if (!Server.IsVerified)
            {
                response = _translator.Translate("Il server non è verificato, AtlantaSecurity è un plugin riservato ai server verificati da Northwood Studios.\nPer verificare il tuo server usa il comando !verify oppure invia una mail a server.verification@scpslgame.com");
                return false;
            }
            if (!API.Utils.ValidateKeyByIp(Server.IpAddress))
            {
                response = _translator.Translate("The server is not authorized to use this command. Use the getKey command to get a key.");
                return false;
            }

            string identifier = arguments.At(0);
            Log.Debug($"Received command with identifier: {identifier}");

            Player player = Player.Get(identifier);

            if (player == null)
            {
                response = _translator.Translate("Player not found.");
                Log.Debug($"Player not found for identifier: {identifier}");
                return false;
            }

            Log.Debug($"Player found: {player.Nickname} with UserID: {player.UserId}");

            var playerInfo = GetPlayerInfoFromServer(player.UserId);

            if (playerInfo != null)
            {
                response = _translator.Translate($"\n<color=red>[ATLANTA-BLACKLIST]</color>\nPlayer Information:\n" +
                           $"Nickname: {player.Nickname}\n" +
                           $"UserID: {playerInfo.UserID}\n" +
                           $"Reason: {playerInfo.Reason}\n" +
                           $"Level: {playerInfo.Level}\n" +
                           $"Expiry Date: {(playerInfo.ExpiryDate.HasValue ? playerInfo.ExpiryDate.Value.ToString("yyyy-MM-dd") : "Permanent")}");

                Log.Debug($"Player information retrieved for {player.Nickname}: {response}");
            }
            else
            {
                response = _translator.Translate($"Player {player.Nickname} was not found in the database.");
                Log.Debug($"Player {player.Nickname} was not found in the database.");
            }

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


        internal class PlayerInfo
        {
            public string UserID { get; set; }
            public string Reason { get; set; }
            public string Level { get; set; }
            public DateTime? ExpiryDate { get; set; }
        }

