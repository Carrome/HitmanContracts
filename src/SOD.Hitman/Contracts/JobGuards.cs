using System.Collections.Generic;
using HarmonyLib;

namespace SODHitman.Contracts
{
    // For an accepted kill contract the target's death is the goal, so nothing may end or fail it for that reason
    // while the client is alive. A dead client still ends it the vanilla way.
    // SideJobController.DeadPeopleJobCheck (seen in the game code) only ends jobs still on the board, not accepted,
    // whose client or target is dead or the city's killer: a posted contract on a dead target leaves the board, as
    // it should. The guard there stays as a safety net for accepted contracts.
    internal static class JobGuards
    {
        internal const string OwnFailPrefix = "Hitman:";

        private static bool inDeadPeopleCheck;
        private static readonly HashSet<string> Logged = new();

        private static bool TargetDeadClientAlive(SideJob job) =>
            ContractRegistry.IsKillContract(job) && job.accepted
            && job.purp != null && job.purp.isDead
            && !(job.poster != null && job.poster.isDead);

        private static bool Block(SideJob job, string what)
        {
            if (Logged.Add($"{job.jobID}:{what}"))
                Plugin.Logger.LogInfo($"[Guard] Blocked {what} on job #{job.jobID}: target dead, client alive, contract waits for hand-in.");
            return false;
        }

        [HarmonyPatch(typeof(SideJobController), nameof(SideJobController.DeadPeopleJobCheck))]
        internal static class SideJobController_DeadPeopleJobCheck
        {
            static void Prefix() => inDeadPeopleCheck = true;
            static void Postfix() => inDeadPeopleCheck = false;
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.End))]
        internal static class SideJob_End
        {
            static bool Prefix(SideJob __instance) =>
                !(inDeadPeopleCheck && TargetDeadClientAlive(__instance)) || Block(__instance, "End (dead people check)");
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.SetJobState))]
        internal static class SideJob_SetJobState
        {
            static bool Prefix(SideJob __instance, SideJob.JobState newState) =>
                !(newState == SideJob.JobState.ended && inDeadPeopleCheck && TargetDeadClientAlive(__instance))
                || Block(__instance, "SetJobState(ended) (dead people check)");
        }

        // Once the target is dead, the only failures that make sense are the client's death and the mod's own.
        [HarmonyPatch(typeof(SideJob), nameof(SideJob.TriggerFail))]
        internal static class SideJob_TriggerFail
        {
            static bool Prefix(SideJob __instance, string reason)
            {
                if (!TargetDeadClientAlive(__instance)) return true;
                if (reason != null && reason.StartsWith(OwnFailPrefix)) return true;
                return Block(__instance, $"TriggerFail('{reason}')");
            }
        }
    }
}
