// Copyright (c) 2026 ui_2506.
// Licensed under the ui_2506 Non-Commercial License.
// See LICENSE.md in the repository root for full license terms.

using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using System.Collections.Generic;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.BroadcastToConnection))]
    internal static class BroadcastToConnectionPatch
    {
        private static bool Prefix(NetworkConnectionToClient connection)
        {
            if (connection == null)
            {
                Main.Log("[BroadcastToConnectionPatch] Connection is null in BroadcastToConnectionPatch.");
                return false;
            }

            List<NetworkIdentity> forRemove = null;

            foreach (NetworkIdentity item in connection.observing)
            {
                if (item == null)
                {
                    forRemove ??= ListPool<NetworkIdentity>.Shared.Rent();
                    forRemove.Add(item);

                    continue;
                }

                NetworkWriter networkWriter = NetworkServer.SerializeForConnection(item, connection);

                if (networkWriter != null)
                {
                    EntityStateMessage message = new()
                    {
                        netId = item.netId,
                        payload = networkWriter.ToArraySegment()
                    };

                    connection.Send(message);
                }
            }

            if (forRemove != null)
            {
                Main.Log("[BroadcastToConnectionPatch] Null NetworkIdentitys founded! Trying remove..");

                foreach (NetworkIdentity item in forRemove)
                {
                    Main.Log($"[BroadcastToConnectionPatch] {item.assetId} (netId={item.netId}) will be removed!");

                    connection.observing.Remove(item);
                }

                ListPool<NetworkIdentity>.Shared.Return(forRemove);

                Main.Log("[BroadcastToConnectionPatch] Null NetworkIdentitys removed.");
            }

            return false;
        }
    }
}
