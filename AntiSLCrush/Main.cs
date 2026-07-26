// Copyright (c) 2026 ui_2506.
// Licensed under the ui_2506 Non-Commercial License.
// See LICENSE.md in the repository root for full license terms.

global using Net = LabApi.Features.Wrappers.Player;

using HarmonyLib;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using System;
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

            if (isError && LastLogTime + 1 < Time.time)
            {
                Logger.Error(message);
                LastLogTime = Time.time;
            }
            else
            {
                Logger.Debug(message, PluginConfig.Debug);
            }
        }
    }
}