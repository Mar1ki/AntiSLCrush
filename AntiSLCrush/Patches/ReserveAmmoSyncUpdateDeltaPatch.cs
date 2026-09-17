// Copyright (c) 2026 ui_2506.
// Licensed under the ui_2506 Non-Commercial License.
// See LICENSE.md in the repository root for full license terms.

using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms.Ammo;
using NorthwoodLib.Pools;
using PlayerRoles.Spectating;
using System.Collections.Generic;
using Utils.Networking;
using static InventorySystem.Items.Firearms.Ammo.ReserveAmmoSync;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(ReserveAmmoSync), nameof(ReserveAmmoSync.UpdateDelta))]
    internal static class ReserveAmmoSyncUpdateDeltaPatch
    {
        private static bool Prefix()
        {
            List<AutosyncItem> forRemove = null;

            foreach (AutosyncItem instance in AutosyncItem.Instances)
            {
                if (!TryUnpack(instance, out ReferenceHub owner, out ItemType ammoType))
                    continue;

                if (owner == null)
                {
                    forRemove ??= ListPool<AutosyncItem>.Shared.Rent();
                    forRemove.Add(instance);

                    Main.Log($"[ReserveAmmoSyncUpdateDeltaPatch] {instance.Owner?.nicknameSync?.MyNick ?? "NULL"} ({instance.Owner?.authManager?.UserId ?? "NULL"}) ReferenceHub is null and will be remover from AutosyncItem.Instances!");
                    continue;
                }

                int curAmmo = owner.inventory.GetCurAmmo(ammoType);
                LastSent orAdd = ServerLastSent.GetOrAdd(owner, () => new LastSent());
                if (orAdd.AmmoCount != curAmmo || orAdd.AmmoType != ammoType)
                {
                    orAdd.AmmoType = ammoType;
                    orAdd.AmmoCount = curAmmo;
                    new ReserveAmmoMessage(owner, ammoType).SendToHubsConditionally((ReferenceHub x) => x.roleManager.CurrentRole is SpectatorRole);
                }
            }

            if (forRemove != null)
            {
                foreach (AutosyncItem item in forRemove)
                    AutosyncItem.Instances.Remove(item);

                ListPool<AutosyncItem>.Shared.Return(forRemove);
            }

            return false;
        }
    }
}
