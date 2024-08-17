using Exiled.API.Features;
using MySql.Data.MySqlClient;
using PluginAPI.Core.Attributes;
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using PluginPriority = Exiled.API.Enums.PluginPriority;

namespace AtlantaSecurity
{
    public class Main : Plugin<Config>
    {
        public static Main Instance;
        private EventHandler _eventHandler;
        internal MySqlConnection _connection;

        public override string Name => "AtlantaSecurity";
        public override string Author => "semplicementeInzi";
        public override string Prefix => "AtlantaSecurity";
        public override Version Version => new Version(0, 1, 0);
        public override Version RequiredExiledVersion => new Version(8, 11, 0);
        public override PluginPriority Priority => PluginPriority.Medium;

        public override void OnEnabled()
        {
            Instance = this;

            try
            {
                var connectionString = LoadConnectionString();
                Log.Debug($"Loaded connection string");

                _connection = new MySqlConnection(connectionString);
                _connection.Open(); // Open the connection
                Log.Debug("Successfully connected to MySQL.");

                _eventHandler = new EventHandler(_connection, Config);
                Exiled.Events.Handlers.Player.Verified += _eventHandler.OnPlayerVerified;

                Log.Debug("Plugin successfully initialized.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during plugin initialization: {ex.Message}");
                Log.Error(ex.StackTrace);
            }

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            try
            {
                Exiled.Events.Handlers.Player.Verified -= _eventHandler.OnPlayerVerified;

                if (_connection != null)
                {
                    _connection.Close();
                    _connection = null;
                    Log.Debug("Database connection closed.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error during plugin shutdown: {ex.Message}");
                Log.Error(ex.StackTrace);
            }

            Instance = null;
            base.OnDisabled();
        }

        internal string LoadConnectionString()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "AtlantaSecurity.dbconfig.json";

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new FileNotFoundException("Configuration file not found.", resourceName);
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var config = JObject.Parse(json);
                        Log.Debug("Configuration file read and parsed.");
                        return config["ConnectionString"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading connection string: {ex.Message}");
                throw;
            }
        }
    }
}
