// Copyright (c) 2026 ui_2506.
// Licensed under the ui_2506 Non-Commercial License.
// See LICENSE.md in the repository root for full license terms.

using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;

namespace AntiSLCrush.Events
{
    internal static class RunInAds
    {
        internal static void Register() => LabApi.Events.Handlers.PlayerEvents.ChangedItem += OnPlayerChangedItem;

        internal static void Unregister() => LabApi.Events.Handlers.PlayerEvents.ChangedItem -= OnPlayerChangedItem;

        private static void OnPlayerChangedItem(PlayerChangedItemEventArgs ev)
        {
            if (ev.NewItem is FirearmItem firearm && firearm.Base.TryGetModule(out LinearAdsModule module) && LinearAdsModule.SyncData.TryGetValue(ev.NewItem.Serial, out LinearAdsModule.AdsData value))
            {
                float adsSpeed = module.Firearm.AttachmentsValue(AttachmentParam.AdsSpeedMultiplier) / module.BaseTimeToTransition;

                value.Update(false, adsSpeed, out bool targetChanged, out bool speedChanged);

                module.OnAdsChanged(false, adsSpeed, targetChanged, speedChanged);
                module._userInput = false;
                module.SendRpc();
            }
        }
    }
}
