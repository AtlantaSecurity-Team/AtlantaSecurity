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
        public override Version Version => new Version(1, 0, 0);
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

            Exiled.Events.Handlers.Player.Verified -= _eventHandler.OnPlayerVerified;
            Log.Debug("Plugin successfully disabled.");
            
            Instance = null;
            base.OnDisabled();
        }

       
    }
}
