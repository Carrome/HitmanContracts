using System;
using System.Collections.Generic;
using HarmonyLib;
using static SODHitman.Dev.Describe;

namespace SODHitman.Contracts
{
    // Which Hitman contracts the boards may carry, and extra contracts on request (the Fixer, dev F8).
    //
    // SideJobController.JobCreationCheck first sets every preset's desired count from its frequency curve
    // (JobPreset.GetFrequencyForSocialCreditLevel), so the curve is the one lever:
    //  - presets above the player's career tier get 0 (Career.Unlocked);
    //  - a requested contract adds one slot to its preset for the rest of the session. The slot has to stay: with
    //    fewer slots than active jobs, the next check would take an unaccepted job back off the board.
    internal static class Postings
    {
        private static readonly Dictionary<string, int> ExtraPosts = new();
        private static readonly Dictionary<int, Action<SideJob>> AwaitingPost = new();

        // Set while a requested contract is being created, so the payment can tell (Payment.Calculate).
        internal static string CreatingFor { get; private set; }

        internal static void ResetSession()
        {
            ExtraPosts.Clear();
            AwaitingPost.Clear();
            CreatingFor = null;
        }

        // Asks the game for one more job of this preset now. onPosted runs once its note is on a board.
        // Returns the new job, or null with a reason.
        internal static SideJob Request(string presetName, string requester, Action<SideJob> onPosted, out string failure)
        {
            failure = null;
            var sjc = SideJobController.Instance;
            if (sjc == null)
            {
                failure = "no side job controller";
                return null;
            }

            SideJobController.JobTracking tracking = null;
            var names = new List<string>();
            foreach (var t in Each(sjc.jobTracking))
            {
                if (t == null) continue;
                var name = N(t.preset) ?? t.name;
                names.Add(name);
                if (name == presetName || t.name == presetName) tracking = t;
            }
            if (tracking == null || tracking.preset == null)
            {
                failure = $"no job tracking for '{presetName}' (tracked: {string.Join(", ", names)})";
                return null;
            }

            var key = tracking.preset.name;
            int before = tracking.activeJobs != null ? tracking.activeJobs.Count : 0;
            ExtraPosts.TryGetValue(key, out int extra);
            ExtraPosts[key] = extra + 1;
            CreatingFor = requester;
            try { sjc.JobCreationCheck(); }
            finally { CreatingFor = null; }
            int after = tracking.activeJobs != null ? tracking.activeJobs.Count : 0;
            if (after <= before) ExtraPosts[key] = extra;

            Plugin.Logger.LogInfo($"[Post] '{presetName}' for {requester}: active {before} -> {after}, desired {tracking.desiredActiveInstances} " +
                                  $"(curve + {ExtraPosts[key]} extra), enableJobs={sjc.enableJobs}");
            if (after <= before)
            {
                failure = "the game found no client/target pair that fits the motive";
                return null;
            }

            var job = tracking.activeJobs[after - 1];
            Plugin.Logger.LogInfo($"[Post] New job {Job(job)} reward={job.reward}");
            if (job.state == SideJob.JobState.posted) onPosted?.Invoke(job);
            else if (onPosted != null) AwaitingPost[job.jobID] = onPosted;
            return job;
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.SetJobState))]
        internal static class SideJob_SetJobState
        {
            static void Postfix(SideJob __instance)
            {
                if (AwaitingPost.Count == 0 || __instance == null || __instance.state != SideJob.JobState.posted) return;
                if (!AwaitingPost.TryGetValue(__instance.jobID, out var onPosted)) return;
                AwaitingPost.Remove(__instance.jobID);
                try { onPosted(__instance); }
                catch (Exception e) { Plugin.Logger.LogWarning($"[Post] Posted callback failed: {e.Message}"); }
            }
        }

        [HarmonyPatch(typeof(JobPreset), nameof(JobPreset.GetFrequencyForSocialCreditLevel))]
        internal static class JobPreset_GetFrequencyForSocialCreditLevel
        {
            static void Postfix(JobPreset __instance, ref int __result)
            {
                if (__instance == null || !ContractRegistry.IsHitmanPreset(__instance)) return;
                if (!Career.Unlocked(__instance)) __result = 0;
                if (ExtraPosts.Count > 0 && ExtraPosts.TryGetValue(__instance.name, out int extra)) __result += extra;
            }
        }
    }
}
