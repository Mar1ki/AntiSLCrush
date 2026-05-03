using HarmonyLib;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Logger = LabApi.Features.Console.Logger;

namespace AntiSLCrush
{
    public sealed class Main : Plugin<Config>
    {
        public override string Name { get; } = "AntiSLCrush";
        public override string Author { get; } = "ui_2506";
        public override string Description { get; } = "AntiSLCrush";
        public override Version Version { get; } = new Version(3, 0, 0);
        public override Version RequiredApiVersion { get; } = new Version(1, 1, 1);
        public override LoadPriority Priority { get; } = LoadPriority.Lowest;

        internal static readonly List<string> BannedIp = new List<string>();
        internal static readonly List<string> BannedHEX = new List<string>();

        internal static float LastLogTime = 0;

        internal static Config PluginConfig;

        private Harmony _harmony;

        public override void Enable()
        {
            PluginConfig = Config;
            _harmony = new Harmony(Name);

            _harmony.PatchAll();
        }

        public override void Disable()
        {
            _harmony.UnpatchAll();

            PluginConfig = null;
            _harmony = null;
        }

        internal static void Log(string message, bool isError = false)
        {
            Debug.LogWarning(message);

            if (isError && LastLogTime + 1 < Time.time) // DONT SPAM ME!
            {
                Logger.Error(message);
                LastLogTime = Time.time;
            }
            else
            {
                Logger.Debug(message, PluginConfig.Debug);
            }
        }

        internal static void BanIpAtSystemLevel(string ip, string reason)
        {
            if (BannedIp.Contains(ip))
                return;

            Logger.Warn($"{ip} will be banned with iptables. Reason: {reason}");

            string command = $"iptables -A INPUT -s {ip} -j DROP";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "sudo";
                proc.StartInfo.Arguments = command;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();

                if (string.IsNullOrEmpty(error))
                {
                    Logger.Warn($"{ip} succesfully banned with IPTables! Reason: {reason}");
                    WebHook.Send($"{ip} succesfully banned with IPTables! Reason: {reason}");

                    BannedIp.Add(ip);
                }
                else
                {
                    Logger.Warn($"Error while HEX banning. stdout: {output}, stderr: {error}");
                    WebHook.Send($"Error while HEX banning. stdout: {output}, stderr: {error}");
                }
            }
            else
            {
                Logger.Error($"[IP BAN] Unable to block IP because your OS is not Linux kernel. Please set ban_ip and ban_hex to false, or install OS with Linux kernel. Your current OS: {RuntimeInformation.OSDescription}");
                WebHook.Send($"[IP BAN] Unable to block IP because your OS is not Linux kernel. Please set ban_ip and ban_hex to false, or install OS with Linux kernel. Your current OS: {RuntimeInformation.OSDescription}");
            }
        }

        internal static void BanHexAtSystemLevel(string hex, int dataLength, string reason)
        {
            if (BannedHEX.Contains(hex))
                return;

            int totalLength = dataLength + 28;

            Logger.Warn($"{hex} will be banned with iptables. Reason: {reason}");

            if (hex.Length > 254)
                hex = hex.Substring(0, 254);

            string hexString = hex.ToLowerInvariant();

            string command = $"sudo iptables -A INPUT -p udp --dport {Server.Port} -m length --length {totalLength}:{totalLength} -m string --algo bm --hex-string \"|{hexString}|\" -j DROP";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "sudo";
                proc.StartInfo.Arguments = command;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();

                if (string.IsNullOrEmpty(error))
                {
                    Logger.Warn($"{hex} succesfully banned with IPTables! Length: {totalLength} Reason: {reason}");
                    WebHook.Send($"{hex} succesfully banned with IPTables! Length: {totalLength} Reason: {reason}");

                    BannedHEX.Add(hex);
                }
                else
                {
                    Logger.Warn($"Error while HEX banning. stdout: {output}, stderr: {error}");
                    WebHook.Send($"Error while HEX banning. stdout: {output}, stderr: {error}");
                }
            }
            else
            {
                Logger.Error($"[HEX BAN] Unable to block HEX because your OS is not Linux kernel. Please set ban_ip and ban_hex to false, or install OS with Linux kernel. Your current OS: {RuntimeInformation.OSDescription}");
                WebHook.Send($"[HEX BAN] Unable to block HEX because your OS is not Linux kernel. Please set ban_ip and ban_hex to false, or install OS with Linux kernel. Your current OS: {RuntimeInformation.OSDescription}");
            }
        }
    }
}