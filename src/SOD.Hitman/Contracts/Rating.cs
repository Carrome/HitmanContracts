using System.Linq;
using HarmonyLib;
using SODHitman.Heat;

namespace SODHitman.Contracts
{
    // v0.7: a rating for each contract handed in, Hitman style. Pays a bonus and builds reputation (career, v0.9).
    //   Silent Assassin: only the target died since accepting, no witness reported, no camera, body never found
    //   Professional:    only the target died and no witness reported (camera or a found body are fine)
    //   Sloppy:          anything else
    internal static class Rating
    {
        internal const string Silent = "silent", Professional = "professional", Sloppy = "sloppy";

        internal static string Name(string rating) => rating switch
        {
            Silent => Text.L("Assassino Silencioso", "Silent Assassin"),
            Professional => Text.L("Profissional", "Professional"),
            Sloppy => Text.L("Desleixado", "Sloppy"),
            _ => Text.L("Negociado", "Negotiated"),
        };

        internal static string Rate(SideJob job, ContractState state)
        {
            var heat = ContractStore.Heat;
            int extraKills = state.KillsAtAccept >= 0 ? heat.Kills - state.KillsAtAccept - 1 : 0;
            var record = job.purp != null ? KillEvidence.RecordFor(job.purp.humanID) : null;
            if (record == null) return extraKills <= 0 ? Professional : Sloppy;
            bool witnessTalked = record.Witnesses.Any(w => w.State == "reported");
            if (extraKills <= 0 && !witnessTalked && !record.Camera && !record.BodyFound) return Silent;
            if (extraKills <= 0 && !witnessTalked) return Professional;
            return Sloppy;
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.AcceptJob))]
        internal static class SideJob_AcceptJob
        {
            static void Postfix(SideJob __instance)
            {
                if (!ContractRegistry.IsHitmanJob(__instance)) return;
                var state = ContractStore.Get(__instance);
                if (state.KillsAtAccept < 0) state.KillsAtAccept = ContractStore.Heat.Kills;
                if (__instance.purp != null)
                {
                    state.TargetId = __instance.purp.humanID;
                    state.TargetName = __instance.purp.GetCitizenName();
                }
            }
        }

        // SideJob.Complete pays the job (it calls OnRewarded); the rating comes right after.
        [HarmonyPatch(typeof(SideJob), nameof(SideJob.Complete))]
        internal static class SideJob_Complete
        {
            static void Postfix(SideJob __instance)
            {
                if (!ContractRegistry.IsHitmanJob(__instance)) return;
                var state = ContractStore.Get(__instance);
                if (state.Rating != null) return;
                if (state.BribedAway || !ContractRegistry.IsKillContract(__instance))
                {
                    // Bribed targets and non-lethal jobs: no rating bonus, a little reputation.
                    state.Rating = state.BribedAway ? "negotiated" : "nonlethal";
                    Career.OnContractPaid(state.Payment, state.Rating);
                    Career.AddReputation(1, state.Rating);
                    Plugin.Logger.LogInfo($"[Rating] Job #{__instance.jobID}: {state.Rating}, no rating bonus.");
                    return;
                }
                string rating = Rate(__instance, state);
                state.Rating = rating;
                float share = rating == Silent ? HitmanConfig.RatingBonusSilent.Value : rating == Professional ? HitmanConfig.RatingBonusPro.Value : 0f;
                // v1.1: a client who keeps the money pays no bonus either; only the deposit was earned.
                bool unpaid = state.Betrayal == Consequences.NoPay;
                if (unpaid) share = 0f;
                int bonus = state.Payment > 0 ? (int)System.Math.Round(state.Payment * share / 10f) * 10 : 0;
                int rep = rating == Silent ? 3 : rating == Professional ? 2 : 1;
                if (bonus > 0) GameplayController.Instance?.AddMoney(bonus, true, "Hitman rating bonus");
                Career.OnContractPaid(unpaid ? state.DepositPaid : state.Payment + bonus, rating);
                Plugin.Logger.LogInfo($"[Rating] Job #{__instance.jobID}: {rating}, bonus Cr{bonus}, reputation +{rep}");
                Text.Notify(
                    $"Avaliação do serviço: {Name(rating)}." + (bonus > 0 ? $" Bônus de Cr{bonus}." : ""),
                    $"Job rating: {Name(rating)}." + (bonus > 0 ? $" Bonus Cr{bonus}." : ""));
                Career.AddReputation(rep, rating);
            }
        }
    }
}
