//using HarmonyLib;
//using PlayerRoles.FirstPersonControl;

//namespace AntiSLCrush.Patches
//{
//    [HarmonyPatch(typeof(FirstPersonMovementModule), nameof(FirstPersonMovementModule.UpdateMovement))]
//    internal static class FirstPersonMovementModulePatch
//    {
//        private static bool Prefix(FirstPersonMovementModule __instance)
//        {
//            if (__instance.Motor == null)
//            {
//                Main.Log($"[FirstPersonMovementModule] {__instance.Hub?.nicknameSync?.MyNick ?? "NULL"} does not have Motor", true);
//                __instance.Motor = __instance.NewMotor;
//                return false;
//            }

//            if (__instance.MouseLook == null)
//            {
//                Main.Log($"[FirstPersonMovementModule] {__instance.Hub?.nicknameSync?.MyNick ?? "NULL"} does not have MouseLook", true);
//                __instance.MouseLook = __instance.NewMouseLook;
//                return false;
//            }

//            return true;
//        }
//    }
//}
