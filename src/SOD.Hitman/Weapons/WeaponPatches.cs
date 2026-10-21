using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SODHitman.Weapons
{
    internal static class WeaponPatches
    {
        // Darts / lethal hits taken by each citizen; cleared when they wake up.
        // Counter pattern adapted from "SODLethalActionReborn" by Venomaus, MIT License.
        private static readonly Dictionary<int, int> Darts = new();
        private static readonly Dictionary<int, int> LethalHits = new();
        private static readonly Dictionary<int, int> BladeHits = new();

        internal static void ResetCounters()
        {
            Darts.Clear();
            LethalHits.Clear();
            BladeHits.Clear();
        }

        // From Guns (Spyraxi, MIT): no "put that gun away" barks while holding a pistol; reset aim state on draw.
        [HarmonyPatch(typeof(FirstPersonItemController), nameof(FirstPersonItemController.FinishedDrawingNewItem))]
        internal static class FirstPersonItemController_FinishedDrawingNewItem
        {
            static void Prefix()
            {
                if (WeaponController.HeldKind() == PistolKind.None) return;
                var held = WeaponController.HeldItem();
                if (held != null && held.preset != null && held.preset.fpsItem != null) held.preset.fpsItem.barkTriggerChance = 0f;
            }

            static void Postfix() => WeaponController.OnItemDrawn();
        }

        // The pistols' effect on a citizen hit by the player's shot:
        //   tranquilizer: the game's damage reaction is skipped (it would start a pursuit of the player even at zero
        //                 damage); asleep after DartsRequiredToSleep darts, for SleepDurationSeconds of real play, and
        //                 the citizen forgets the player (Tranquilizer)
        //   lethal:       full damage; a hit that knocks out, empties health or reaches ShotsToKill kills.
        //                 The game's own kill (enableKill) stays off: it calls Human.Murder with no Murder object,
        //                 which throws for the player (see WeaponKills.MurderByPlayer), so the postfix kills instead.
        [HarmonyPatch(typeof(Citizen), nameof(Citizen.RecieveDamage))]
        internal static class Citizen_RecieveDamage
        {
            private static PistolKind hit;
            private static MeleeKind melee;

            static bool Prefix(Citizen __instance, Actor fromWho, ref float amount, ref bool alertSurrounding, ref bool enableKill)
            {
                hit = PistolKind.None;
                melee = MeleeKind.None;
                if (__instance.isPlayer || __instance.isDead || fromWho == null || !fromWho.isPlayer) return true;
                hit = WeaponController.GunshotKind();
                if (hit == PistolKind.Tranquilizer && __instance.ai != null) return false;
                if (hit == PistolKind.Lethal)
                {
                    amount *= HitmanConfig.LethalDamageMultiplier.Value;
                    enableKill = false;
                    alertSurrounding = HitmanConfig.LethalAlertSurrounding.Value || Arsenal.IsLoudFirearm(WeaponController.HeldItem()?.preset);
                    return true;
                }

                // v0.8, melee with the player's held item (not a shot).
                if (hit != PistolKind.None || __instance.ai == null) return true;
                melee = Arsenal.HeldMelee();
                switch (melee)
                {
                    case MeleeKind.Poison:
                        // Only on someone asleep or knocked out; awake, it is just a bash.
                        if (!__instance.ai.ko) { melee = MeleeKind.None; return true; }
                        Arsenal.Poison(__instance);
                        melee = MeleeKind.None;
                        return false;
                    case MeleeKind.Garrote:
                        // From behind, or on someone down: one silent move. Otherwise a normal hit.
                        if (!__instance.ai.ko && !Arsenal.IsBehind(__instance, fromWho)) { melee = MeleeKind.None; return true; }
                        melee = MeleeKind.None;
                        WeaponKills.Kill(__instance, WeaponController.HeldItem(), "garrote");
                        return false;
                    case MeleeKind.Blade:
                        amount *= HitmanConfig.ArsenalBladeDamageMultiplier.Value;
                        enableKill = false;
                        return true;
                }
                return true;
            }

            static void Postfix(Citizen __instance, Vector3 damagePosition, Vector3 damageDirection)
            {
                var kind = hit;
                hit = PistolKind.None;
                var blade = melee == MeleeKind.Blade;
                melee = MeleeKind.None;
                if (blade && !__instance.isDead && __instance.ai != null)
                {
                    int bid = __instance.humanID;
                    BladeHits.TryGetValue(bid, out int cuts);
                    BladeHits[bid] = ++cuts;
                    if (__instance.ai.ko || __instance.currentHealth <= 0f || cuts >= HitmanConfig.ArsenalBladeHitsToKill.Value)
                    {
                        BladeHits.Remove(bid);
                        WeaponKills.Kill(__instance, WeaponController.HeldItem(), $"blade hit #{cuts}");
                    }
                    return;
                }
                if (kind == PistolKind.None || __instance.isDead || __instance.ai == null) return;
                int id = __instance.humanID;

                if (kind == PistolKind.Tranquilizer)
                {
                    if (__instance.ai.ko)
                    {
                        Tranquilizer.Redart(__instance);
                        return;
                    }
                    Darts.TryGetValue(id, out int darts);
                    Darts[id] = ++darts;
                    if (darts < HitmanConfig.DartsToSleep.Value)
                    {
                        Tranquilizer.Forget(__instance);
                        return;
                    }
                    Darts.Remove(id);
                    Tranquilizer.Sleep(__instance, damagePosition, damageDirection);
                    return;
                }

                LethalHits.TryGetValue(id, out int hits);
                LethalHits[id] = ++hits;
                if (__instance.ai.ko || __instance.currentHealth <= 0f || hits >= HitmanConfig.LethalShotsToKill.Value)
                {
                    LethalHits.Remove(id);
                    WeaponKills.Kill(__instance, WeaponController.HeldItem(), $"lethal shot #{hits}");
                }
            }
        }

        // From LethalActionReborn (Venomaus, MIT): forget the counters once the citizen wakes.
        [HarmonyPatch(typeof(NewAIController), nameof(NewAIController.SetKO))]
        internal static class NewAIController_SetKO
        {
            static void Postfix(NewAIController __instance, bool val)
            {
                if (val || __instance.human == null) return;
                Darts.Remove(__instance.human.humanID);
                LethalHits.Remove(__instance.human.humanID);
                BladeHits.Remove(__instance.human.humanID);
            }
        }
    }
}
