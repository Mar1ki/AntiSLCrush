using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using UnityEngine;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.OnAdded))]
    internal static class Scp330BagOnAddedPatch
    {
        private static float _lastCalledTime = 0;

        private static bool Prefix(Scp330Bag __instance)
        {
            if (_lastCalledTime + 0.01f > Time.time)
            {
                Main.Log($"[Scp330Bag.OnAdded] {__instance.Owner?.nicknameSync?.MyNick ?? "NULL"}. Too fast calling. Canceling call", true);
                return false;
            }

            _lastCalledTime = Time.time;
            return true;
        }
    }
}
