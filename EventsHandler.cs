﻿using AtlantaSecurity.API;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AtlantaSecurity
{
    public class EventsHandler
    {
        private readonly HttpClient _httpClient;
        private readonly List<string> _levelPriority = new List<string> { "Low", "Medium", "High", "Extreme" };
        private readonly Config _config;
        private readonly Translator _translator = new Translator();

        public EventsHandler(Config config)
        {
            _httpClient = new HttpClient();
            _config = config;
        }

        public void OnWaitingForPlayers()
        {
            if (!API.Utils.ValidateKeyByIp(Server.IpAddress))
            {
                Log.Warn(_translator.Translate("The server doesn't have a key yet, which means the plugin won't work unless you request one.\nTo request a key, run the getkey command"));
            }
        }
        public async void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (!API.Utils.ValidateKeyByIp(Server.IpAddress)) return;

            string userId = ev.Player.UserId;
            var playerData = await GetPlayerData(userId);

            if (!string.IsNullOrEmpty(playerData.Reason) && !string.IsNullOrEmpty(playerData.Level))
            {
                BroadcastToAdmins(playerData, ev.Player);

                if (ShouldKickPlayer(playerData.Level))
                {
                    string kickMessage = _config.KickMessage
                        .Replace("%reason%", playerData.Reason);

                    ev.Player.Kick(kickMessage);
                }
            }
        }

        private async Task<(string Reason, string Level)> GetPlayerData(string userId)
        {
            string url = "http://sl.lunarscp.it:4000/get-player-data"; 
            var json = $"{{\"userId\": \"{userId}\"}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var playerData = ParsePlayerData(responseData); 

                    return playerData;
                }
                else
                {
                    Log.Error($"Error requesting player data: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error requesting server: {ex.Message}");
            }

            return (null, null);
        }

        private (string Reason, string Level) ParsePlayerData(string json)
        {

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            string reason = data.ContainsKey("reason") ? data["reason"] : null;
            string level = data.ContainsKey("level") ? data["level"] : null;
            return (reason, level);
        }

        private void BroadcastToAdmins((string Reason, string Level) playerData, Player player)
        {
            string messageHint = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=30><color=red>[ATLANTA-BLACKLIST]</color> Player: {player.Nickname} | Reason: {playerData.Reason} | Level: {playerData.Level}</size></color>";
            string message = $"\n<color=red>[ATLANTA-BLACKLIST]</color>\nPlayer: {player.Nickname}\nReason: {playerData.Reason}\nLevel: {playerData.Level}";

            foreach (var admin in Player.List.Where(p => p.RemoteAdminAccess))
            {
                admin.SendConsoleMessage(message, "red");
                admin.ShowHint(messageHint, 10f);
                admin.SendStaffMessage(message);
            }
        }

        private bool ShouldKickPlayer(string playerLevel)
        {
            var configLevel = _config.BlacklistLevel;

            int playerLevelIndex = _levelPriority.IndexOf(playerLevel);
            int configLevelIndex = _levelPriority.IndexOf(configLevel);

            return playerLevelIndex >= configLevelIndex;
        }
    }
}
