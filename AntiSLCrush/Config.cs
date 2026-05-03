using System.ComponentModel;

namespace AntiSLCrush
{
    public sealed class Config
    {
        public bool Debug { get; set; } = true;

        //[Description("\n# -----Anti host disconnect-----" +
        //    "\n# How many host disconnect attempts are allowed before the server restarts. (Use 0 to disable auto restart. 0-255)")]
        //public byte MaxHostDisconnectAttempts { get; set; } = 10;

        [Description("\n# -----Anti SL bots-----")]
        public string DiscordWebHook { get; set; } = string.Empty;
        [Description("Ban HEX that is sent too frequently")]
        public bool BanHex { get; set; } = true;
        [Description("Ban IP that is sent too frequently")]
        public bool BanIp { get; set; } = true;
    }
}
