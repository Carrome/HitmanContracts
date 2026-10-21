using System;
using System.Collections.Generic;
using System.Linq;
using SODHitman.Contracts;
using UnityEngine;
using static SODHitman.Dev.Describe;

namespace SODHitman.Weapons
{
    internal enum MeleeKind
    {
        None,
        Blade,
        Garrote,
        Poison,
    }

    public sealed class PoisonRecord
    {
        public int VictimId { get; set; }
        public string VictimName { get; set; }
        public float DeathAt { get; set; }
    }

    // Pillar 2 of the roadmap (v0.8): the game's own weapons, each with a cost.
    //  - extra firearms (revolver, shotgun, rifles): fire like the lethal pistol, but they are LOUD. Everyone in
    //    earshot becomes a witness, and the shot is illegal whoever sees it. Sniper rifles zoom further.
    //  - blades (knives, katana, box cutter, razor): a melee hit that knocks out or empties health kills
    //  - garrote (rope, wire): from behind, a silent kill in one move
    //  - poison (syringe, poison bottle) on a sleeping or knocked-out citizen: they die hours later of "natural
    //    causes" (little heat, no crime scene)
    //  - forged suicide: a poison or garrote death with a suicide note in the victim's home counts as suicide
    // No new animations or models: every item is a vanilla one, used with the vanilla swing.
    internal static class Arsenal
    {
        private static HashSet<string> loud, snipers, blades, garrotes, poisons;
        private static readonly Dictionary<string, InteractablePreset> Extra = new();

        // The kill method of the death being processed (read by HeatSystem.OnPlayerKill).
        internal static string CurrentMethod { get; set; }

        private static HashSet<string> Set(string csv) =>
            new((csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);

        internal static void OnCityReady()
        {
            loud = Set(HitmanConfig.ArsenalFirearms.Value);
            snipers = Set(HitmanConfig.ArsenalSnipers.Value);
            blades = Set(HitmanConfig.ArsenalBlades.Value);
            garrotes = Set(HitmanConfig.ArsenalGarrotes.Value);
            poisons = Set(HitmanConfig.ArsenalPoisons.Value);
            Extra.Clear();
            foreach (var name in loud.Concat(blades).Concat(garrotes).Concat(poisons))
            {
                var preset = WeaponController.FindPreset(name);
                if (preset != null) Extra[preset.presetName] = preset;
                else Plugin.Logger.LogWarning($"[Arsenal] Item '{name}' not found in this game build.");
            }
            Plugin.Logger.LogInfo($"[Arsenal] {Extra.Count} item(s): firearms [{string.Join(", ", loud)}], blades [{string.Join(", ", blades)}], " +
                                  $"garrotes [{string.Join(", ", garrotes)}], poisons [{string.Join(", ", poisons)}]");
            if (HitmanConfig.ArsenalSellFirearms.Value)
                WeaponShops.Stock(loud.Select(WeaponController.FindPreset).Where(p => p != null).ToArray());
        }

        private static bool In(HashSet<string> set, InteractablePreset preset) =>
            set != null && preset != null && (set.Contains(preset.presetName) || set.Contains(preset.name));

        internal static bool IsLoudFirearm(InteractablePreset preset) => In(loud, preset);
        internal static bool IsSniper(InteractablePreset preset) => In(snipers, preset);

        internal static MeleeKind HeldMelee()
        {
            var preset = WeaponController.HeldItem()?.preset;
            if (In(garrotes, preset)) return MeleeKind.Garrote;
            if (In(poisons, preset)) return MeleeKind.Poison;
            if (In(blades, preset)) return MeleeKind.Blade;
            return MeleeKind.None;
        }

        internal static IEnumerable<InteractablePreset> Items => Extra.Values;

        // ---- garrote ----

        // The player stands behind the victim: their facing points away from the player.
        internal static bool IsBehind(Human victim, Actor player)
        {
            var toVictim = victim.transform.position - player.transform.position;
            toVictim.y = 0f;
            var facing = victim.transform.forward;
            facing.y = 0f;
            return toVictim.sqrMagnitude > 0.0001f && Vector3.Dot(facing.normalized, toVictim.normalized) > 0.5f;
        }

        // ---- loud shots: who heard them ----

        internal static float LastLoudShotTime { get; set; } = -100f;
        internal static Vector3 LastLoudShotPosition { get; set; }

        internal static bool RecentLoudShot => Time.time - LastLoudShotTime <= 3f;

        // ---- poison ----

        private static List<PoisonRecord> Poisoned => ContractStore.Heat.Poisoned ??= new List<PoisonRecord>();

        internal static void Poison(Citizen victim)
        {
            if (victim == null || victim.isDead || Poisoned.Any(p => p.VictimId == victim.humanID)) return;
            float min = HitmanConfig.PoisonDelayMinHours.Value, max = Math.Max(min, HitmanConfig.PoisonDelayMaxHours.Value);
            float at = Text.Now + min + (float)new System.Random().NextDouble() * (max - min);
            Poisoned.Add(new PoisonRecord { VictimId = victim.humanID, VictimName = victim.GetCitizenName(), DeathAt = at });
            Plugin.Logger.LogInfo($"[Arsenal] {Who(victim)} poisoned; dies at {Text.Clock(at)}");
            Text.Notify($"Você envenenou {victim.GetCitizenName()}. Em algumas horas vai parecer morte natural.",
                        $"You poisoned {victim.GetCitizenName()}. In a few hours it will look like natural causes.");
        }

        internal static void Tick(float now)
        {
            if (Poisoned.Count == 0) return;
            foreach (var p in Poisoned.ToList())
            {
                if (now < p.DeathAt) continue;
                Poisoned.Remove(p);
                if (CityData.Instance == null || !CityData.Instance.GetHuman(p.VictimId, out var h, false) || h == null || h.isDead) continue;
                WeaponKills.DieNaturally(h, "poison");
            }
        }

        // ---- forged suicide ----

        // A suicide note anywhere in the victim's home.
        internal static bool SuicideNoteAtHome(Human victim)
        {
            var home = victim != null ? victim.home : null;
            var all = CityData.Instance != null ? CityData.Instance.interactableDirectory : null;
            if (home == null || all == null) return false;
            foreach (var i in Each(all))
            {
                if (i == null || i.preset == null || i.node == null) continue;
                var name = i.preset.presetName ?? i.preset.name;
                if (name == null || name.IndexOf("SuicideNote", StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (i.node.gameLocation == home) return true;
            }
            return false;
        }
    }
}
