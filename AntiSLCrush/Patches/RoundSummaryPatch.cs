using CentralAuth;
using GameCore;
using HarmonyLib;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using MEC;
using Mirror;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Utils.NonAllocLINQ;
using static RoundSummary;
using Logger = LabApi.Features.Console.Logger;

namespace AntiSLCrush.Patches
{
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary._ProcessServerSideCode))]
    internal static class ProcessServerSideCodePatch
    {
        static bool Prefix(RoundSummary __instance, ref IEnumerator<float> __result)
        {
            __result = SafeProcessServerSideCode(__instance);
            return false;
        }

        private static IEnumerator<float> SafeProcessServerSideCode(RoundSummary inst)
        {
            float time = Time.unscaledTime;

            while (inst != null)
            {
                yield return Timing.WaitForSeconds(2.5f);

                // ===== STAGE 1: Preconditions =====
                try
                {
                    if (NetworkServer.active)
                        inst.UpdateTargetCount();

                    if (!inst.IsRoundEnded &&
                        (RoundSummary.RoundLock ||
                         (inst.KeepRoundOnOne &&
                          ReferenceHub.AllHubs.Count(x =>
                              x.authManager.InstanceMode != ClientInstanceMode.DedicatedServer) < 2) ||
                         !RoundSummary.RoundInProgress() ||
                         Time.unscaledTime - time < 15f))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[RoundSummary] STAGE 1 FAILED\n{ex}");
                    continue;
                }

                // ===== STAGE 2: Collect players =====
                SumInfo_ClassList newList = default;

                try
                {
                    foreach (var hub in ReferenceHub.AllHubs)
                    {
                        switch (hub.GetTeam())
                        {
                            case Team.ClassD: newList.class_ds++; break;
                            case Team.ChaosInsurgency: newList.chaos_insurgents++; break;
                            case Team.FoundationForces: newList.mtf_and_guards++; break;
                            case Team.Scientists: newList.scientists++; break;
                            case Team.Flamingos: newList.flamingos++; break;
                            case Team.SCPs:
                                if (hub.GetRoleId() == RoleTypeId.Scp0492)
                                    newList.zombies++;
                                else
                                    newList.scps_except_zombies++;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"[RoundSummary] STAGE 2 (COUNT HUBS) FAILED\n{ex}");
                    continue;
                }

                yield return float.NegativeInfinity;

                // ===== STAGE 3: Warhead =====
                try
                {
                    newList.warhead_kills = AlphaWarheadController.Detonated
                        ? AlphaWarheadController.Singleton.WarheadKills
                        : -1;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[RoundSummary] STAGE 3 (WARHEAD) FAILED\n{ex}");
                }

                yield return float.NegativeInfinity;

                // ===== STAGE 4: Math & conditions =====
                LeadingTeam leadingTeam = LeadingTeam.Draw;

                try
                {
                    int mtf = newList.mtf_and_guards + newList.scientists;
                    int chaos = newList.chaos_insurgents + newList.class_ds;
                    int scps = newList.scps_except_zombies + newList.zombies;
                    int flamingos = newList.flamingos;

                    RoundSummary.SurvivingSCPs = newList.scps_except_zombies;

                    int aliveTeams = 0;
                    if (mtf > 0) aliveTeams++;
                    if (chaos > 0) aliveTeams++;
                    if (scps > 0) aliveTeams++;
                    if (flamingos > 0) aliveTeams++;
                    if (inst.ExtraTargets > 0) aliveTeams++;

                    inst.IsRoundEnded = aliveTeams <= 1;

                    var checkArgs = new RoundEndingConditionsCheckEventArgs(inst.IsRoundEnded);
                    ServerEvents.OnRoundEndingConditionsCheck(checkArgs);
                    inst.IsRoundEnded = checkArgs.CanEnd;

                    if (!inst.IsRoundEnded)
                        continue;

                    if (mtf > 0)
                        leadingTeam = RoundSummary.EscapedScientists < RoundSummary.EscapedClassD
                            ? LeadingTeam.Draw
                            : LeadingTeam.FacilityForces;
                    else if (scps > 0)
                        leadingTeam = RoundSummary.EscapedClassD > RoundSummary.SurvivingSCPs
                            ? LeadingTeam.ChaosInsurgency
                            : LeadingTeam.Anomalies;
                    else if (chaos > 0)
                        leadingTeam = LeadingTeam.ChaosInsurgency;
                    else if (flamingos > 0)
                        leadingTeam = LeadingTeam.Flamingos;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[RoundSummary] STAGE 4 (LOGIC) FAILED\n{ex}");
                    continue;
                }

                // ===== STAGE 5: Events =====
                try
                {
                    var endingArgs = new RoundEndingEventArgs(leadingTeam);
                    ServerEvents.OnRoundEnding(endingArgs);
                    if (!endingArgs.IsAllowed)
                        continue;

                    leadingTeam = endingArgs.LeadingTeam;

                    var field = typeof(RoundSummary)
                        .GetField(nameof(RoundSummary.OnRoundEnded),
                            BindingFlags.Static | BindingFlags.NonPublic);

                    var del = field?.GetValue(null) as Action<LeadingTeam, SumInfo_ClassList>;
                    del?.Invoke(leadingTeam, newList);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[RoundSummary] STAGE 5 (EVENTS) FAILED\n{ex}");
                }

                yield return Timing.WaitForSeconds(1.5f);

                // ===== STAGE 6: Restart =====
                try
                {
                    var endedArgs = new RoundEndedEventArgs(leadingTeam);
                    ServerEvents.OnRoundEnded(endedArgs);

                    int restartTime = Mathf.Clamp(
                        ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10),
                        5, 1000);

                    if (inst != null && endedArgs.ShowSummary)
                    {
                        inst.RpcShowRoundSummary(
                            inst.classlistStart,
                            newList,
                            leadingTeam,
                            RoundSummary.EscapedClassD,
                            RoundSummary.EscapedScientists,
                            RoundSummary.KilledBySCPs,
                            restartTime,
                            (int)RoundStart.RoundLength.TotalSeconds);
                    }

                    inst._roundEndCoroutine =
                        Timing.RunCoroutine(inst.InitiateRoundEnd(restartTime), Segment.FixedUpdate);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[RoundSummary] STAGE 6 (RESTART) FAILED\n{ex}");
                }

                yield return Timing.WaitUntilDone(inst._roundEndCoroutine);
            }
        }

    }

}
