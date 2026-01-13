using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp330;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.OnAdded))]
    internal static class Scp330BagOnAddedPatch
    {
        private static float _lastCalledTime = 0;

        private static bool Prefix(Scp330Bag __instance, ItemPickupBase pickup)
        {
            string nickName = __instance.Owner?.nicknameSync?.MyNick ?? "NULL";

            if (_lastCalledTime + 0.01f > Time.time)
            {
                Logger.Info($"[Scp330Bag.OnAdded] {nickName}. Too fast calling. Canceling call");
                return false;
            }

            _lastCalledTime = Time.time;
            return true;
        }
    }
}
