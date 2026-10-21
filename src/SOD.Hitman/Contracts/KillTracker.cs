using System.Collections.Generic;
using HarmonyLib;

namespace SODHitman.Contracts
{
    // A contract is fulfilled only by the target's death, whoever or whatever killed them (spec 2).
    // KO or tranquilliser sleep never count: the test is Actor.isDead.
    internal static class KillTracker
    {
        internal static void OnDeath(Human victim, Human killer, string source)
        {
            if (victim == null) return;
            foreach (var job in ContractRegistry.ActiveHitmanJobs())
                if (job.purp == victim && ContractRegistry.IsKillContract(job)) CompleteKill(job, killer, source);
        }

        // Catches deaths that did not go through Human.Murder, and contracts loaded from a save.
        internal static void Sweep()
        {
            foreach (var job in ContractRegistry.ActiveHitmanJobs())
            {
                if (job.purp == null || !job.purp.isDead || !ContractRegistry.IsKillContract(job)) continue;
                if (ContractStore.TryGet(job, out var state) && state.KillRecorded) continue;
                CompleteKill(job, job.purp.death != null ? job.purp.death.GetKiller() : null, "sweep");
            }
        }

        internal static void CompleteKill(SideJob job, Human killer, string source)
        {
            var state = ContractStore.Get(job);
            var question = ContractRegistry.KillQuestion(job);
            if (question != null)
            {
                question.completedRevenge = true;
                question.revengeObjPassed = 1f;
                question.SetProgress(1f, true);
            }
            try { job.UpdateResolveAnswers(); }
            catch (System.Exception e) { Plugin.Logger.LogWarning($"[Kill] UpdateResolveAnswers failed on job #{job.jobID}: {e.Message}"); }

            if (state.KillRecorded) return;
            state.KillRecorded = true;
            Plugin.Logger.LogInfo($"[Kill] Job #{job.jobID}: target {Dev.Describe.Who(job.purp)} dead (killer {Dev.Describe.Who(killer)}, via {source}). " +
                                  $"Question progress={question?.progress} completedRevenge={question?.completedRevenge} isCorrect={question?.isCorrect}");
            Text.Notify(
                "Contrato cumprido. Preencha o formulário de resolução e entregue o caso.",
                "Contract fulfilled. Fill in the resolve form and hand in the case.");
        }

        [HarmonyPatch(typeof(Human), nameof(Human.Murder))]
        internal static class Human_Murder
        {
            // While loading, the game re-runs Human.Murder for saved corpses; Sweep looks at the contracts afterwards.
            static void Postfix(Human __instance, Human killer)
            {
                if (ContractStore.Loading) return;
                if (__instance != null && __instance.isDead) OnDeath(__instance, killer, "Human.Murder");
            }
        }

        // HitmanKill uses answerMethod "ManualTrigger", which the mod completes itself (CompleteKill above).
        // RevengeObjective.ManualTrigger must NOT be patched: its body (return false) is folded by IL2CPP with
        // 386 other methods, HDRP rendering included, so a patch would run for all of them (tools/foldcheck).
    }
}
