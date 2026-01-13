using HarmonyLib;
using LiteNetLib;
using Mirror.LiteNetLib4Mirror;
using System;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(LiteNetLib4MirrorServer), nameof(LiteNetLib4MirrorServer.OnPeerConnected))]
    internal static class LiteNetLibPatch
    {
        private static bool Prefix(NetPeer peer)
        {
            int idx = peer.Id + 1;

            if (idx >= LiteNetLib4MirrorServer.Peers.Length)
                Array.Resize(ref LiteNetLib4MirrorServer.Peers, Math.Max(LiteNetLib4MirrorServer.Peers.Length * 2, idx + 1));

            LiteNetLib4MirrorServer.Peers[idx] = peer;

            if (idx > LiteNetLib4MirrorServer._maxId)
                LiteNetLib4MirrorServer._maxId = idx;

            LiteNetLib4MirrorTransport.Singleton.OnServerConnected(idx);

            return false;
        }
    }
}
