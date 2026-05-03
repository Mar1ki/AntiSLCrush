//using CentralAuth;
//using HarmonyLib;
//using NorthwoodLib.Pools;
//using PlayerRoles;
//using PlayerRoles.RoleAssign;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Reflection.Emit;

//namespace AntiSLCrush.Patches
//{
//    [HarmonyPatch(typeof(ScpSpawner), nameof(ScpSpawner.AssignScp))]
//    internal static class ScpSpawnerPatch
//    {
//        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
//        {
//            List<CodeInstruction> newinstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

//            int roleManagerIndex = newinstructions.FindIndex(x =>
//                x.opcode == OpCodes.Ldfld &&
//                (FieldInfo)x.operand == AccessTools.Field(typeof(ReferenceHub), nameof(ReferenceHub.roleManager)));

//            int roleSetIndex = newinstructions.FindIndex(x =>
//                x.opcode == OpCodes.Callvirt &&
//                (MethodInfo)x.operand == AccessTools.Method(typeof(PlayerRoleManager), nameof(PlayerRoleManager.ServerSetRole)));

//            Label continueLabel = generator.DefineLabel();
//            newinstructions[roleSetIndex + 2].labels.Add(continueLabel);

//            CodeInstruction hubLoadInstruction = newinstructions[roleManagerIndex - 1];

//            newinstructions.InsertRange(roleManagerIndex - 1, new CodeInstruction[]
//            {
//                new CodeInstruction(hubLoadInstruction.opcode, hubLoadInstruction.operand),
//                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ScpSpawnerPatch), nameof(ScpSpawnerPatch.IsValid))),
//                new CodeInstruction(OpCodes.Brfalse, continueLabel)
//            });

//            for (ushort i = 0; i < newinstructions.Count; i++)
//                yield return newinstructions[i];

//            ListPool<CodeInstruction>.Shared.Return(newinstructions);
//        }

//        private static bool IsValid(ReferenceHub hub) => hub && (hub.authManager.InstanceMode is ClientInstanceMode.ReadyClient || hub.authManager.InstanceMode is ClientInstanceMode.Dummy);
//    }
//}
