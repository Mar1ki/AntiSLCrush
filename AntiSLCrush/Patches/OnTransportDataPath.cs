// Copyright (c) 2026 ui_2506.
// Licensed under the ui_2506 Non-Commercial License.
// See LICENSE.md in the repository root for full license terms.

using HarmonyLib;
using Mirror;
using System.Collections.Generic;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.OnTransportData))]
    internal static class OnTransportDataPath
    {
        private static readonly List<int> _kickedList = new List<int>();

        private static bool Prefix(int connectionId)
        {
            if (connectionId == 0)
                return true;

            if (!NetworkServer.connections.ContainsKey(connectionId))
            {
                Main.Log($"[OnTransportDataPath] HandleData Unknown connectionId:{connectionId}, trying to disconnect.");

                if (!_kickedList.Contains(connectionId))
                {
                    Main.Log($"[OnTransportDataPath] HandleData Unknown connectionId:{connectionId}, disconnecting.");
                    Transport.active?.ServerDisconnect(connectionId);

                    _kickedList.Add(connectionId);
                }

                return false;
            }

            return true;
        }
    }
}
