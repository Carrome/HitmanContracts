using Il2CppInterop.Runtime;
using SODHitman.Contracts;

namespace SODHitman.Weapons
{
    // Death by the lethal pistol. It goes through Human.Murder with the player as killer, so contracts (KillTracker)
    // and heat (HeatSystem) see it. Kill sequence adapted from "SODLethalActionReborn" by Venomaus, MIT License.
    internal static class WeaponKills
    {
        internal static void Kill(Human victim, Interactable weapon, string how)
        {
            if (victim == null || victim.isDead) return;
            Arsenal.CurrentMethod = how;
            try { MurderByPlayer(victim, weapon, how.StartsWith("garrote") ? 0f : 1f); }
            finally { Arsenal.CurrentMethod = null; }
            if (!ContractStore.WeaponKills.Contains(victim.humanID)) ContractStore.WeaponKills.Add(victim.humanID);
            Plugin.Logger.LogInfo($"[Weapons] {Dev.Describe.Who(victim)} killed ({how}); isDead={victim.isDead}, death recorded={DeathRecorded(victim)}");
        }

        // v0.8: a death that looks natural (poison): no killer on the death record, no scream. Heat and contracts
        // still learn it was the player's doing (KillEvidence treats it as a natural death).
        internal static void DieNaturally(Human victim, string how)
        {
            if (victim == null || victim.isDead) return;
            Arsenal.CurrentMethod = how;
            try
            {
                victim.SetHealth(0f);
                victim.Murder(victim, true, BlankMurder(), null, 0f);
                Settle(victim);
                if (!Heat.HeatSystem.Counted(victim)) Heat.HeatSystem.OnPlayerKill(victim);
                KillTracker.OnDeath(victim, Player.Instance, how);
            }
            finally { Arsenal.CurrentMethod = null; }
            if (!ContractStore.WeaponKills.Contains(victim.humanID)) ContractStore.WeaponKills.Add(victim.humanID);
            Plugin.Logger.LogInfo($"[Weapons] {Dev.Describe.Who(victim)} died of {how}; isDead={victim.isDead}");
        }

        // Records the death the way the game records a murder victim's: time and place of death, killer, body, the
        // dead list and the side-job clean-up. In this game build Human.Death's constructor reads its Murder argument
        // without a null check, so Human.Murder(killer, setTimeOfDeath: true, murder: null, ...) throws halfway: the
        // victim is flagged dead but none of the rest happens, and no Human.Murder postfix (heat, contracts) runs.
        // A blank Murder (allocated without running its constructor, so no murder case starts) satisfies it.
        // The weapon stays out of the call: with a Murder object, Human.Murder would make it that case's weapon and
        // build a moniker from the blank case. It is written on the death record afterwards.
        internal static void MurderByPlayer(Human victim, Interactable weapon, float chanceToScream)
        {
            var player = Player.Instance;
            victim.SetHealth(0f);
            victim.Murder(player, true, BlankMurder(), null, chanceToScream);
            if (weapon != null && DeathRecorded(victim)) victim.death.weapon = weapon.id;
            Settle(victim);

            // Heat and contracts hear about the death from their Human.Murder postfixes. Called again here in case
            // the game threw inside Murder; both skip a victim they have already counted.
            if (!Heat.HeatSystem.Counted(victim)) Heat.HeatSystem.OnPlayerKill(victim);
            KillTracker.OnDeath(victim, player, "player kill");
        }

        // murderID -1 is the game's own "no murder" value (Death sets it before reading the Murder), so
        // Death.GetMurder() finds no case for these deaths. The game code that calls GetMurder handles null.
        private static MurderController.Murder BlankMurder()
        {
            var murder = new MurderController.Murder(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<MurderController.Murder>.NativeClassPtr));
            murder.murderID = -1;
            return murder;
        }

        private static bool DeathRecorded(Human victim) =>
            victim.death != null && victim.death.isDead && victim.death.victim == victim.humanID;

        // The game's save keeps the death record of a citizen killed this way and restores it on load
        // (SaveStateController.LoadSaveState). This catches the rest, such as kills from 0.5.3 and earlier, which
        // never got a record. Runs while ContractStore.Loading is still set, so it adds no heat.
        internal static void ReapplyOnLoad()
        {
            if (!HitmanConfig.ReapplyKillsOnLoad.Value || CityData.Instance == null) return;
            int revived = 0;
            foreach (var id in ContractStore.WeaponKills)
            {
                Human human = null;
                if (!CityData.Instance.GetHuman(id, out human, false) || human == null) continue;
                if (!human.isDead)
                {
                    human.SetHealth(0f);
                    if (DeathRecorded(human)) human.Murder(Player.Instance, false, null, null, 0f);
                    else human.Murder(Player.Instance, true, BlankMurder(), null, 0f);
                    revived++;
                }
                Settle(human);
            }
            if (ContractStore.WeaponKills.Count > 0)
                Plugin.Logger.LogInfo($"[Weapons] Checked {ContractStore.WeaponKills.Count} kill(s) on load; {revived} had come back.");
        }

        // Keep the city's bookkeeping consistent: dead list, and the AI killer's current murderer/victim.
        private static void Settle(Human victim)
        {
            var dead = CityData.Instance.deadCitizensDirectory;
            if (dead != null && !dead.Contains(victim)) dead.Add(victim);

            var mc = MurderController.Instance;
            if (mc == null) return;
            if (mc.currentMurderer != null && mc.currentMurderer.humanID == victim.humanID)
            {
                mc.currentMurderer = null;
                mc.PickNewMurderer();
            }
            if (mc.currentVictim != null && mc.currentVictim.humanID == victim.humanID)
            {
                mc.currentVictim = null;
                mc.PickNewVictim();
            }
        }
    }
}
