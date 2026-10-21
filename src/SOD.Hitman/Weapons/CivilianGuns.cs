using System.Collections.Generic;
using HarmonyLib;

namespace SODHitman.Weapons
{
    // 6th test (20/10/2026): ordinary citizens drew pistols and shot at the player. A citizen fights with the best
    // weapon in their inventory (NewAIController.UpdateCurrentWeapon -> SetCurrentWeapon), and guns got there: the mod
    // stocks pistols and rifles at the weapons dealers and black market, where citizens can buy them
    // (ActionController.PurchaseItem), and the tranquilizer is a spawnable copy of the game's pistol.
    // Unless [Arsenal] CiviliansUseGuns is on, a gun only stays in the hand of someone who should have one; the others
    // fall back to their fists, as the game does without a weapon (SetCurrentWeapon(null)).
    internal static class CivilianGuns
    {
        private static readonly HashSet<int> Logged = new();

        private static bool IsGun(Interactable item)
        {
            var weapon = item != null && item.preset != null ? item.preset.weapon : null;
            if (weapon == null) return false;
            var type = weapon.type;
            return type == MurderWeaponPreset.WeaponType.handgun || type == MurderWeaponPreset.WeaponType.rifle || type == MurderWeaponPreset.WeaponType.shotgun;
        }

        private static bool MayCarryGun(Human h)
        {
            if (h == null || h.isPlayer || h.isEnforcer) return true;
            var occupation = h.job != null && h.job.preset != null ? h.job.preset.name : null;
            if (Contracts.Payment.IsFighter(occupation)) return true;
            var heat = Contracts.ContractStore.Heat;
            if (heat.HunterIds.Contains(h.humanID) || (heat.PersonalHunterIds != null && heat.PersonalHunterIds.Contains(h.humanID))) return true;
            var murders = MurderController.Instance;
            if (murders == null) return false;
            if (murders.currentMurderer != null && murders.currentMurderer.humanID == h.humanID) return true;
            foreach (var m in Dev.Describe.Each(murders.activeMurders))
                if (m != null && m.murderer != null && m.murderer.humanID == h.humanID) return true;
            return false;
        }

        [HarmonyPatch(typeof(NewAIController), nameof(NewAIController.SetCurrentWeapon))]
        internal static class NewAIController_SetCurrentWeapon
        {
            static bool Prefix(NewAIController __instance, Interactable obj)
            {
                if (obj == null || HitmanConfig.CiviliansUseGuns.Value || !IsGun(obj)) return true;
                var human = __instance.human;
                if (MayCarryGun(human)) return true;
                if (Logged.Add(human.humanID))
                    Plugin.Logger.LogInfo($"[Weapons] {Dev.Describe.Who(human)} ({human.job?.preset?.name ?? "no job"}) won't fight with {obj.preset.name}: bare hands.");
                __instance.SetCurrentWeapon(null);
                return false;
            }
        }
    }
}
