using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlantaSecurity
{
    public class Config : IConfig
    {
        [Description("Do you want to Enable this plugin?")]
        public bool IsEnabled { get; set; } = true;

        [Description("Do you want to enable debugging")]
        public bool Debug { get; set; } = false;

        /*[Description("What language would you like to use? (english, italian)")]
        public string Language { get; set; } = "english";*/

        [Description("Should I kick a player if he has a certain level of danger?")]
        public bool KickForDangerousness { get; set; } = true;

        [Description("At what threat level does Atlanta kick the suspected player from the server? (Low, Medium, High, Extreme) (ignore if KickForDangerousness is disabled)")]
        public string BlacklistLevel { get; set; } = "Extreme";

        [Description("Kick message (use %reason% to get the reason for the blacklist) ")]
        public string KickMessage { get; set; } = "Kicked due to blacklist: %reason%";

    }
}
