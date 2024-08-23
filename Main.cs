using Exiled.API.Features;
using PluginAPI.Core.Attributes;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginPriority = Exiled.API.Enums.PluginPriority;

namespace AtlantaSecurity
{
    public class Main : Plugin<Config>
    {
        public static Main Instance;
        private EventsHandler _eventHandler;
        private static readonly HttpClient HttpClient = new HttpClient();
        private const string ServerUrl = "http://sl.lunarscp.it:4000";

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
                _eventHandler = new EventsHandler(Config);
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
                Log.Debug("Plugin successfully disabled.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during plugin shutdown: {ex.Message}");
                Log.Error(ex.StackTrace);
            }

            Instance = null;
            base.OnDisabled();
        }

        internal static async Task<bool> ValidateServerKeyAsync(string key)
        {
            var url = $"{ServerUrl}/validate-key";
            var requestBody = new { key = key };
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await HttpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    Log.Debug($"Chiave valida: {responseData}");
                    return true;
                }
                else
                {
                    Log.Debug($"Chiave non valida: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Errore durante la richiesta al server: {ex.Message}");
                return false;
            }
        }
    }
}
