using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SODHitman.Contracts
{
    // Pillar 3 of the roadmap (v0.9): a career.
    //  - reputation comes from rated contracts (Rating): tiers Beginner → Professional → Veteran → Legend
    //  - each contract preset needs a tier before the boards carry it (Postings gates the frequency curve)
    //  - higher tiers pay more; contracts the Fixer hands out pay a bonus on top
    //  - non-lethal jobs (intimidation) run in parallel: cheap, always available, never required (decided 20/10/2026)
    internal static class Career
    {
        private static Heat.HeatState H => ContractStore.Heat;

        internal static bool Enabled => HitmanConfig.CareerEnabled.Value;

        private static int[] Thresholds => Parse(HitmanConfig.CareerTierThresholds.Value, int.Parse, new[] { 0, 5, 15, 30 });
        private static float[] PayMultipliers => Parse(HitmanConfig.CareerTierPay.Value, s => float.Parse(s, CultureInfo.InvariantCulture), new[] { 1f, 1.25f, 1.5f, 2f });

        private static T[] Parse<T>(string csv, Func<string, T> parse, T[] fallback)
        {
            try
            {
                var parts = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(parse).ToArray();
                return parts.Length > 0 ? parts : fallback;
            }
            catch { return fallback; }
        }

        internal static int TierFor(int reputation)
        {
            var t = Thresholds;
            int tier = 0;
            for (int i = 0; i < t.Length; i++)
                if (reputation >= t[i]) tier = i;
            return tier;
        }

        internal static int Tier => TierFor(H.Reputation);

        internal static string TierName(int tier) => tier switch
        {
            0 => Text.L("Iniciante", "Beginner"),
            1 => Text.L("Profissional", "Professional"),
            2 => Text.L("Veterano", "Veteran"),
            _ => Text.L("Lenda", "Legend"),
        };

        // Reputation still needed for the next tier, or -1 at the top.
        internal static int ToNextTier()
        {
            var t = Thresholds;
            int tier = Tier;
            return tier + 1 < t.Length ? t[tier + 1] - H.Reputation : -1;
        }

        // ---- presets ----

        private static Dictionary<string, int> presetTiers;

        internal static int MinTier(JobPreset preset)
        {
            if (presetTiers == null)
            {
                presetTiers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in HitmanConfig.CareerPresetTiers.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    var kv = pair.Split(':');
                    if (kv.Length == 2 && int.TryParse(kv[1], out int tier)) presetTiers[kv[0].Trim()] = tier;
                }
            }
            return preset != null && presetTiers.TryGetValue(preset.name, out int t) ? t : 0;
        }

        internal static bool Unlocked(JobPreset preset) => !Enabled || MinTier(preset) <= Tier;

        // The kill contracts the player can get right now, highest tier first.
        internal static List<string> UnlockedKillPresets() =>
            ContractRegistry.KillPresetNames()
                .Where(name => !Enabled || MinTierByName(name) <= Tier)
                .OrderByDescending(MinTierByName)
                .ToList();

        private static int MinTierByName(string name)
        {
            MinTier(null);
            return presetTiers.TryGetValue(name, out int t) ? t : 0;
        }

        // ---- payment ----

        internal static float PayMultiplier(SideJob job)
        {
            float m = 1f;
            if (Enabled)
            {
                var pay = PayMultipliers;
                m *= pay[Math.Min(Tier, pay.Length - 1)];
            }
            if (!ContractRegistry.IsKillContract(job)) m *= HitmanConfig.CareerNonLethalPay.Value;
            if (Postings.CreatingFor == "fixer" || Postings.CreatingFor == Campaign.Requester) m *= 1f + HitmanConfig.FixerBonus.Value;
            if (Postings.CreatingFor == Campaign.Requester) m *= HitmanConfig.CampaignPayMultiplier.Value;
            return m;
        }

        // ---- reputation ----

        internal static void AddReputation(int points, string why)
        {
            if (points == 0) return;
            int before = Tier;
            H.Reputation += points;
            int after = Tier;
            Plugin.Logger.LogInfo($"[Career] Reputation +{points} ({why}) -> {H.Reputation}, tier {after} ({TierName(after)})");
            if (Enabled && after > before)
                Text.Notify($"Sua reputação subiu: agora você é {TierName(after)}. Contratos melhores e mais bem pagos vão aparecer.",
                            $"Your reputation grew: you are now a {TierName(after)}. Better-paid contracts will show up.");
            Fixer.OnReputation();
        }

        internal static void OnContractPaid(int payment, string rating)
        {
            H.ContractsDone++;
            if (payment > 0) H.Earned += payment;
            if (rating == Rating.Silent) H.SilentRatings++;
        }

        // The killer's record (Fixer phone option, dev menu).
        internal static string Sheet() =>
            Text.L($"Você é {TierName(Tier)} (reputação {H.Reputation}{(ToNextTier() > 0 ? $", faltam {ToNextTier()} para o próximo nível" : "")}). " +
                   $"{H.ContractsDone} serviço(s), {H.SilentRatings} como Assassino Silencioso, {H.Kills} morte(s), Cr{H.Earned} ganhos. Heat {H.Points}.",
                   $"You are a {TierName(Tier)} (reputation {H.Reputation}{(ToNextTier() > 0 ? $", {ToNextTier()} to the next tier" : "")}). " +
                   $"{H.ContractsDone} job(s), {H.SilentRatings} as Silent Assassin, {H.Kills} kill(s), Cr{H.Earned} earned. Heat {H.Points}.");
    }
}
