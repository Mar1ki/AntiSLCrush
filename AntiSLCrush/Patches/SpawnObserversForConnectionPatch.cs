using HarmonyLib;
using Mirror;

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

            conn.Send(default(ObjectSpawnStartedMessage));

            foreach (NetworkIdentity identity in NetworkServer.spawned.Values)
            {
                if (identity == null || identity.gameObject == null || !identity.gameObject.activeSelf)
                    continue;

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
            return false;
        }
    }
}
