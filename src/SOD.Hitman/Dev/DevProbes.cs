using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SODHitman.Contracts;
using static SODHitman.Dev.Describe;

namespace SODHitman.Dev
{
    // Read-only logging of the game paths the mod depends on (deaths and side job lifecycle).
    // Every patch here is skipped unless Dev.EnableProbes is on.
    internal static class DevProbes
    {
        private static void Log(string msg) => Plugin.Logger.LogInfo("[Probe] " + msg);

        private static string GameStack()
        {
            try { return new Il2CppSystem.Diagnostics.StackTrace(false).ToString().Replace("\n", " | "); }
            catch (Exception e) { return "(unavailable: " + e.Message + ")"; }
        }

        // Skip the noise of the hundreds of jobs the game posts and expires on its own.
        private static bool Interesting(SideJob j) =>
            j.accepted || ContractRegistry.IsHitmanJob(j) || (j.purp != null && j.purp.isDead) || (j.poster != null && j.poster.isDead);

        [HarmonyPatch(typeof(Human), nameof(Human.Murder))]
        internal static class Human_Murder
        {
            static bool Prepare() => HitmanConfig.DevProbes.Value;

            static void Prefix(Human __instance, out bool __state) => __state = __instance.isDead;

            static void Postfix(Human __instance, Human killer, MurderController.Murder murder, Interactable weapon, bool __state)
            {
                var roles = new List<string>();
                foreach (var j in ContractRegistry.AllJobs())
                {
                    if (j.purp == __instance) roles.Add("purp of " + Job(j));
                    if (j.poster == __instance) roles.Add("poster of " + Job(j));
                }
                Log($"Human.Murder victim={Who(__instance)} killer={Who(killer)} murderObj={murder != null} weapon={Where(weapon) ?? "null"} " +
                    $"wasDead={__state} isDead={__instance.isDead} roles=[{string.Join("; ", roles)}]");
            }
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.SetJobState))]
        internal static class SideJob_SetJobState
        {
            static bool Prepare() => HitmanConfig.DevProbes.Value;

            static void Prefix(SideJob __instance, out SideJob.JobState __state) => __state = __instance.state;

            static void Postfix(SideJob __instance, SideJob.JobState __state)
            {
                if (__state == __instance.state) return;
                if (__instance.state == SideJob.JobState.ended || Interesting(__instance))
                    Log($"SetJobState {__state} -> {__instance.state}: {Job(__instance)}");
                // Which game code ended a contract (the managed stack stops at the Harmony trampoline).
                if (__instance.state == SideJob.JobState.ended && ContractRegistry.IsHitmanJob(__instance) && __instance.accepted)
                    Log($"  ended by (IL2CPP stack): {GameStack()}");
            }
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.TriggerFail))]
        internal static class SideJob_TriggerFail
        {
            static bool Prepare() => HitmanConfig.DevProbes.Value;

            static void Prefix(SideJob __instance, string reason) => Log($"TriggerFail '{reason}': {Job(__instance)}");
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.OnAcquireJobInfo), new[] { typeof(DialogPreset) })]
        internal static class SideJob_OnAcquireJobInfo_Dialog
        {
            static bool Prepare() => HitmanConfig.DevProbes.Value;

            static void Postfix(SideJob __instance, DialogPreset dialog) =>
                Log($"OnAcquireJobInfo(dialog={N(dialog)}): {Job(__instance)}");
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.OnAcquireJobInfo), new[] { typeof(string) })]
        internal static class SideJob_OnAcquireJobInfo_Message
        {
            static bool Prepare() => HitmanConfig.DevProbes.Value;

            static void Postfix(SideJob __instance, string infoDialogMessage) =>
                Log($"OnAcquireJobInfo(msg={infoDialogMessage}): {Job(__instance)}");
        }

        // Parameterless lifecycle steps, logged after they run.
        // Only methods with their own machine code: IL2CPP folds identical bodies into one function, and a patch on
        // a folded method runs for every method sharing it. OnGooseChaseCallTriggered is empty in the base class and
        // shares its code with 4245 other methods; patching it crashed the game at startup (19/10/2026).
        // Check any new target with tools/foldcheck before adding it.
        [HarmonyPatch]
        internal static class SideJob_Lifecycle
        {
            private static readonly string[] Methods =
            {
                "PostJob", "AcceptJob", "OnPlayerCall", "OnGooseChaseSuccess", "OnGooseChaseEnd",
                "SetHandIn", "SubmitCase", "Complete", "OnRewarded", "End",
            };

            static bool Prepare() => HitmanConfig.DevProbes.Value;

            static IEnumerable<MethodBase> TargetMethods()
            {
                foreach (var name in Methods)
                {
                    var m = AccessTools.Method(typeof(SideJob), name, Type.EmptyTypes);
                    if (m != null) yield return m;
                }
            }

            static void Postfix(SideJob __instance, MethodBase __originalMethod)
            {
                if (Interesting(__instance))
                    Log($"{__originalMethod.Name}: {Job(__instance)} gameTime={SessionData.Instance.gameTime:0.00}");
            }
        }

        // Shows what the game does to jobs whose poster or target has died. Logs each job once per state.
        [HarmonyPatch(typeof(SideJobController), nameof(SideJobController.DeadPeopleJobCheck))]
        internal static class SideJobController_DeadPeopleJobCheck
        {
            private static readonly HashSet<string> Seen = new();

            static bool Prepare() => HitmanConfig.DevProbes.Value;

            static void Prefix(out List<(SideJob job, SideJob.JobState state, bool failed)> __state)
            {
                __state = new List<(SideJob, SideJob.JobState, bool)>();
                foreach (var j in ContractRegistry.AllJobs())
                {
                    if (j.state == SideJob.JobState.ended) continue;
                    if ((j.purp != null && j.purp.isDead) || (j.poster != null && j.poster.isDead))
                        __state.Add((j, j.state, j.failed));
                }
            }

            static void Postfix(List<(SideJob job, SideJob.JobState state, bool failed)> __state)
            {
                foreach (var (job, state, failed) in __state)
                {
                    var key = $"{job.jobID}:{state}:{failed}->{job.state}:{job.failed}";
                    if (!Seen.Add(key)) continue;
                    Log($"DeadPeopleJobCheck: before state={state} failed={failed}, after {Job(job)} " +
                        $"purpDead={job.purp != null && job.purp.isDead} posterDead={job.poster != null && job.poster.isDead}");
                }
            }
        }
    }
}
