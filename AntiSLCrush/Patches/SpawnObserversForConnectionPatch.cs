using HarmonyLib;
using Mirror;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(NetworkServer), nameof(NetworkServer.SpawnObserversForConnection))]
    internal static class SpawnObserversForConnectionPatch
    {
        private static bool Prefix(NetworkConnectionToClient conn)
        {
            if (conn == null || !conn.isReady || conn.identity == null)
            {
                Main.Log("[SpawnObserversForConnection] conn or conn.identity is null");
                return false;
            }

            conn.Send(default(ObjectSpawnStartedMessage));

            foreach (NetworkIdentity value in NetworkServer.spawned.Values)
            {
                if (value == null || value.gameObject == null || !value.gameObject.activeSelf)
                {
                    Main.Log("[SpawnObserversForConnection] (NetworkServer.spawned.Values) value or value.gameObject is null");
                    continue;
                }

                if (value.visible == Visibility.ForceShown)
                {
                    value.AddObserver(conn);
                }
                else if (value.visible == Visibility.Default)
                {
                    if (NetworkServer.aoi != null)
                    {
                        if (NetworkServer.aoi.OnCheckObserver(value, conn))
                            value.AddObserver(conn);
                    }
                    else
                    {
                        value.AddObserver(conn);
                    }
                }
            }

            conn.Send(default(ObjectSpawnFinishedMessage));
            return false;
        }
    }
}
