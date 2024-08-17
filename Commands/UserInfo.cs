using CommandSystem;
using Exiled.API.Features;
using MySql.Data.MySqlClient;
using System;

namespace AtlantaSecurity.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class UserInfo : ICommand
    {
        public string Command { get; } = "userinfo";
        public string[] Aliases { get; } = { "userinfo" };
        public string Description { get; } = "Checks if a player is present in the database and retrieves their information.";
        public bool SanitizeResponse => true;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: userinfo <ID/PlayerName>";
                Log.Debug("No arguments provided. Usage message sent.");
                return false;
            }

            string identifier = arguments.At(0);
            Log.Debug($"Received command with identifier: {identifier}");

            Player player = Player.Get(identifier);

            if (player == null)
            {
                response = "Player not found.";
                Log.Debug($"Player not found for identifier: {identifier}");
                return false;
            }

            Log.Debug($"Player found: {player.Nickname} with UserID: {player.UserId}");

            var playerInfo = GetPlayerInfoFromDatabase(player.UserId);

            if (playerInfo != null)
            {
                response = $"\n<color=red>[ATLANTA-BLACKLIST]</color>\nPlayer Information:\n" +
                           $"Nickname: {player.Nickname}\n" +
                           $"UserID: {playerInfo.UserID}\n" +
                           $"Reason: {playerInfo.Reason}\n" +
                           $"Level: {playerInfo.Level}\n" +
                           $"Expiry Date: {(playerInfo.ExpiryDate.HasValue ? playerInfo.ExpiryDate.Value.ToString("yyyy-MM-dd") : "Permanent")}";

                Log.Debug($"Player information retrieved for {player.Nickname}: {response}");
            }
            else
            {
                response = $"Player {player.Nickname} was not found in the database.";
                Log.Debug($"Player {player.Nickname} with UserID {player.UserId} not found in the database.");
            }

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
                    connection.Open(); // Apri la connessione

                    using (var command = new MySqlCommand(query, connection))
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
                } // La connessione viene chiusa automaticamente qui
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
