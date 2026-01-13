using System.ComponentModel;

namespace AntiSLCrush
{
    public class Config
    {
        public bool Debug { get; set; } = true;

        [Description("Anti SL bots")]
        public string DiscordWebHook { get; set; } = string.Empty;
        public bool ShowSuspiciousPacketLogs { get; set; } = true;
        public bool BanHex { get; set; } = true;
        public bool BanIp { get; set; } = true;
    }
}
