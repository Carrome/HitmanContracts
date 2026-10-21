using System;
using System.Collections.Generic;
using HarmonyLib;

namespace SODHitman.Contracts
{
    // Contract price by the target's job (spec 5.2b, provisional):
    //   P = (Base + salary / SalaryDivisor) x risk x random(1 +/- RandomSpread), rounded to 10 and clamped.
    // Seeded by jobID so a contract keeps its price across reloads.
    internal static class Payment
    {
        private static HashSet<string> fighterJobs;
        private static readonly HashSet<int> WarnedNoQuestion = new();

        internal static bool IsFighter(string occupation)
        {
            fighterJobs ??= new HashSet<string>(
                HitmanConfig.FighterJobs.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase);
            return occupation != null && fighterJobs.Contains(occupation);
        }

        // Yearly salary in credits. The game stores it in thousands (salary 100 shows as Cr100000 on the job), which
        // up to 1.2.0 made salaries count for almost nothing and every bribe fail (limit Cr3 in the 1.2.0 log).
        internal static float AnnualSalary(Occupation occupation) => occupation != null ? occupation.salary * 1000f : 0f;

        internal static int Calculate(SideJob job, out string breakdown)
        {
            var occupation = job.purp != null ? job.purp.job : null;
            float salary = AnnualSalary(occupation);
            string occupationName = occupation != null && occupation.preset != null ? occupation.preset.name : "(none)";
            float risk = IsFighter(occupationName) ? HitmanConfig.PaymentFighterRisk.Value : 1f;

            var rng = new Random(unchecked(job.jobID * 7919 + 17));
            float spread = HitmanConfig.PaymentRandomSpread.Value;
            float roll = 1f + (float)(rng.NextDouble() * 2.0 - 1.0) * spread;

            float career = Career.PayMultiplier(job); // v0.9: career tier, non-lethal job, Fixer bonus
            float raw = (HitmanConfig.PaymentBase.Value + salary / HitmanConfig.PaymentSalaryDivisor.Value) * risk * roll * career;
            int rounded = (int)Math.Round(raw / 10f) * 10;
            int payment = Math.Clamp(rounded, HitmanConfig.PaymentMin.Value, HitmanConfig.PaymentMax.Value);

            // Bribe rule (spec 5.1), logged with the price so balance can be read from the log.
            bool employed = occupation != null && occupation.employer != null;
            bool accepts = Bribe.WouldAccept(salary, employed, payment, out float bribeLimit);
            breakdown = $"({HitmanConfig.PaymentBase.Value} + {salary:0} / {HitmanConfig.PaymentSalaryDivisor.Value}) x risk {risk} ({occupationName}) " +
                        $"x roll {roll:0.00} x career {career:0.00} = {raw:0} -> {payment}. Bribe: offer {2 * payment} vs limit {bribeLimit:0} -> {(accepts ? "would accept" : "would refuse")}";
            return payment;
        }

        // Writes the price into the kill question (what the hand-in pays) and the job total (what the posting shows).
        internal static void Apply(SideJob job, string when)
        {
            var question = ContractRegistry.KillQuestion(job);
            if (question == null)
            {
                if (WarnedNoQuestion.Add(job.jobID))
                    Plugin.Logger.LogWarning($"[Payment] No kill question on job #{job.jobID} ({when})");
                return;
            }

            var state = ContractStore.Get(job);
            if (state.Payment < 0 && job.purp != null)
            {
                state.Payment = Calculate(job, out var breakdown);
                state.PaymentBreakdown = breakdown;
                Plugin.Logger.LogInfo($"[Payment] Job #{job.jobID} target {job.purp.citizenName}: {breakdown}");
            }
            if (state.Payment < 0) return;

            // IL2CPP wrappers are new objects on every access: compare native pointers, not references.
            int others = 0;
            foreach (var q in Dev.Describe.Each(ContractRegistry.Questions(job)))
                if (q != null && q.Pointer != question.Pointer) others += q.reward;

            // What is left to pay at the hand-in, after the deposit handed over at the meeting.
            int due = Math.Max(0, state.Payment - state.DepositPaid);
            if (question.reward != due || job.reward != due + others)
            {
                Plugin.Logger.LogInfo($"[Payment] Job #{job.jobID} ({when}): question reward {question.reward} -> {due}, job reward {job.reward} -> {due + others}" +
                                      (state.DepositPaid > 0 ? $" (deposit Cr{state.DepositPaid} already paid)" : ""));
                question.reward = due;
                job.reward = due + others;
            }
        }

        // Decided 20/10/2026 (v0.6): the client hands over part of the payment at the meeting, the rest comes with the
        // hand-in. Paid once per contract.
        internal static void PayDeposit(SideJob job, ContractState state)
        {
            float share = HitmanConfig.PaymentDepositShare.Value;
            if (share <= 0f || state.DepositPaid > 0 || state.KillRecorded || GameplayController.Instance == null) return;
            if (state.Payment < 0) Apply(job, "deposit");
            if (state.Payment <= 0) return;

            int deposit = (int)Math.Round(state.Payment * Math.Min(share, 1f) / 10f) * 10;
            if (deposit <= 0) return;
            state.DepositPaid = deposit;
            GameplayController.Instance.AddMoney(deposit, true, "Hitman deposit");
            Apply(job, "deposit");
            Plugin.Logger.LogInfo($"[Payment] Job #{job.jobID}: deposit Cr{deposit} of Cr{state.Payment} paid at the meeting.");
            Text.Notify($"O cliente adiantou Cr{deposit}. O resto, Cr{state.Payment - deposit}, vem na entrega.",
                        $"The client paid Cr{deposit} up front. The rest, Cr{state.Payment - deposit}, comes with the hand-in.");
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.GenerateResolveQuestions))]
        internal static class SideJob_GenerateResolveQuestions
        {
            static void Postfix(SideJob __instance)
            {
                if (ContractRegistry.IsHitmanJob(__instance)) Apply(__instance, "GenerateResolveQuestions");
            }
        }
    }
}
