using CommandSystem;
using Exiled.API.Features;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

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
            Log.Debug("Executing global search command.");

            // Get all players from the server
            var allPlayers = Player.List;
            int count = 0;
            List<string> results = new List<string>();

            foreach (var player in allPlayers)
            {
                var playerInfo = GetPlayerInfoFromDatabase(player.UserId);

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

        private PlayerInfo GetPlayerInfoFromDatabase(string userId)
        {
            Log.Debug("Checking if the player's Steam ID is in the database...");

            string query = "SELECT userId, reason, livello, expiryDate FROM SLBlacklist WHERE userId = @UserID";
            Log.Debug($"Executing query: {query}");

            try
            {
                using (var connection = new MySqlConnection(Main.Instance.LoadConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        Log.Debug($"Query parameter set: @UserID = {userId}");

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Log.Debug("Player record found in the database.");
                                return new PlayerInfo
                                {
                                    UserID = reader["userId"].ToString(),
                                    Reason = reader["reason"].ToString(),
                                    Level = reader["livello"].ToString(),
                                    ExpiryDate = reader["expiryDate"] as DateTime?
                                };
                            }
                            else
                            {
                                Log.Debug("No record found for the specified UserID.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"Database query error: {ex.Message}");
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
