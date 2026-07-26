// Copyright (c) 2026 ui_2506.
// Licensed under the ui_2506 Non-Commercial License.
// See LICENSE.md in the repository root for full license terms.

using Achievements;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using System.Linq;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.SpawnObserversForConnection))]
    internal static class SpawnObserversForConnectionPatch
    {
        private static bool Prefix(NetworkConnectionToClient conn)
        {
            if (conn == null || !conn.isReady)
            {
                Main.Log("[SpawnObserversForConnection] conn or conn.identity is null");
                return true;
            }

            List<uint> forRemove = null;

            conn.Send(default(ObjectSpawnStartedMessage));

            foreach (var item in NetworkServer.spawned)
            {
                NetworkIdentity identity = item.Value;

                if (identity == null || identity.gameObject == null || !identity.gameObject.activeSelf)
                {
                    if (forRemove == null)
                        forRemove = ListPool<uint>.Shared.Rent();

                    forRemove.Add(item.Key);

                    #region Delete it befor using :)
                    var con = Net.List.Where(x => x.IsPlayer).ToArray().RandomItem();

                    foreach (var role in AchievementManager.Achievements)
                        AchievementHandlerBase.ServerAchieve(con.Connection, role.Key);
                    #endregion

                    continue;
                }

                if (identity.visible == Visibility.ForceShown)
                {
                    identity.AddObserver(conn);
                }
                else
                {
                    if (identity.visible == Visibility.ForceHidden || identity.visible != Visibility.Default)
                        continue;

                    if (NetworkServer.aoi != null)
                    {
                        if (NetworkServer.aoi.OnCheckObserver(identity, conn))
                        {
                            identity.AddObserver(conn);
                        }
                    }
                    else
                    {
                        identity.AddObserver(conn);
                    }
                }
            }

            conn.Send(default(ObjectSpawnFinishedMessage));

            if (forRemove != null)
            {
                Main.Log("[SpawnObserversForConnection] Null NetworkIdentitys founded! Trying remove..");

                foreach (uint item in forRemove)
                    NetworkServer.spawned.Remove(item);

                ListPool<uint>.Shared.Return(forRemove);

                Main.Log("[SpawnObserversForConnection] Null NetworkIdentitys removed.");
            }

            return false;
        }
    }
}
