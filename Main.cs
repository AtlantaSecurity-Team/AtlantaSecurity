using Exiled.API.Features;
using System;
using PluginPriority = Exiled.API.Enums.PluginPriority;

namespace AtlantaSecurity
{
    public class Main : Plugin<Config>
    {
        public static Main Instance;
        private EventsHandler _eventHandler;

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
                Exiled.Events.Handlers.Server.WaitingForPlayers += _eventHandler.OnWaitingForPlayers;

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
            Exiled.Events.Handlers.Server.WaitingForPlayers -= _eventHandler.OnWaitingForPlayers;

            Log.Debug("Plugin successfully disabled.");
            
            Instance = null;
            base.OnDisabled();
        }

       
    }
}
