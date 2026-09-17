// Copyright (c) 2026 ui_2506.
// Licensed under the ui_2506 Non-Commercial License.
// See LICENSE.md in the repository root for full license terms.

using AntiSLCrush.Patches;
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
        public override Version Version { get; } = new Version(3, 1, 0);
        public override Version RequiredApiVersion { get; } = new Version(1, 1, 7);
        public override LoadPriority Priority { get; } = LoadPriority.Lowest;

        private static float _lastLogTime = 0;
        private static Harmony _harmony;

        public override void Enable()
        {
            _harmony = new Harmony(Name);
            _harmony.PatchAll();

            LabApi.Events.Handlers.ServerEvents.RoundRestarted += OnTransportDataPatch.Clear;
        }

        public override void Disable()
        {
            _harmony.UnpatchAll();
            _harmony = null;

            LabApi.Events.Handlers.ServerEvents.RoundRestarted -= OnTransportDataPatch.Clear;
        }

        internal static void Log(string message, bool isError = false)
        {
            Debug.LogWarning(message);

            if (isError && _lastLogTime + 1 < Time.time)
            {
                Logger.Error(message);
                _lastLogTime = Time.time;
            }
            else
            {
                Logger.Debug(message, Config.Instance.Debug);
            }
        }
    }
}