using HarmonyLib;
using Mirror;
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
                Main.Log("[BroadcastToConnectionPatch] Connection is null in BroadcastToConnectionPatch.", true);
                return false;
            }

            List<NetworkIdentity> forRemove = null;

            foreach (NetworkIdentity item in connection.observing)
            {
                if (item == null || item.gameObject == null)
                {
                    if (forRemove == null)
                        forRemove = new List<NetworkIdentity>();

                    forRemove.Add(item);

                    continue;
                }

                NetworkWriter networkWriter = NetworkServer.SerializeForConnection(item, connection);

                if (networkWriter != null)
                {
                    EntityStateMessage message = new EntityStateMessage
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
                    connection.observing.Remove(item);

                Main.Log("[BroadcastToConnectionPatch] Null NetworkIdentitys removed.");
            }

            return false;
        }
    }
}
