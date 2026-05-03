//using HarmonyLib;
//using LabApi.Features.Wrappers;
//using Mirror;
//using System.Diagnostics;

//namespace AntiSLCrush.Patches
//{
//    [HarmonyPatch(typeof(LocalConnectionToServer), nameof(LocalConnectionToServer.Disconnect))]
//    internal static class DisconnectLoggerPatch
//    {
//        private static byte _count = 0;

//        private static bool Prefix(LocalConnectionToServer __instance)
//        {
//            if (__instance.connectionId != 0)
//                return true;

//            StackTrace stack = new StackTrace(true);

//            Main.Log($"[AntiExploit] Attempt to disconnect host (id 0)\n{stack}", true);

//            if (Main.PluginConfig.MaxHostDisconnectAttempts > 0 && ++_count >= Main.PluginConfig.MaxHostDisconnectAttempts)
//            {
//                Main.Log("[AntiExploit] Too many attempts. Restarting server...", true);
//                Server.Restart();
//            }

//            return false;
//        }
//    }
//}
