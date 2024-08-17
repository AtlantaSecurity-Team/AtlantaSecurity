using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtlantaSecurity
{
    public class EventHandler
    {
        private readonly MySqlConnection _connection;
        private readonly List<string> _levelPriority = new List<string> { "Low", "Medium", "High", "Extreme" };
        private readonly Config _config;

        public EventHandler(MySqlConnection connection, Config config)
        {
            _connection = connection;
            _config = config;
        }

        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            string userId = ev.Player.UserId;
            var playerData = GetPlayerData(userId);

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

        private (string Reason, string Level) GetPlayerData(string userId)
        {
            string query = "SELECT reason, livello FROM SLBlacklist WHERE userId = @UserId";

            try
            {
                using (var command = new MySqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string reason = reader.GetString("reason");
                            string level = reader.GetString("livello");
                            return (reason, level);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error querying player data: {ex.Message}");
            }

            return (null, null);
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
