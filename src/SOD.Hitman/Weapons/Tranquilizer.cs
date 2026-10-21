using System;
using System.Collections.Generic;
using UnityEngine;
using static SODHitman.Dev.Describe;

namespace SODHitman.Weapons
{
    // A dart puts the citizen to sleep for SleepDurationSeconds of real, unpaused play (decided 19/10/2026: 3 minutes)
    // and the citizen forgets who shot them: no pursuit, combat or "eye" on the player, before or after waking.
    //
    // Found in the game code (4th test, darted citizens slept "forever" and the player stayed hunted):
    //  - NewAIController.SetKO(forced) sets koTime = gameTime + forcedDuration, and gameTime is in game HOURS, so the
    //    old 120 meant five days.
    //  - Citizen.RecieveDamage from the player starts a pursuit (SetPersue) even with zero damage; asleep, the citizen
    //    kept chasing the player through Actor.persuedBy, which is what keeps the eye on screen.
    // So the dart skips the game's damage reaction entirely (WeaponPatches), and this class does the knock-out, the
    // real-time wake-up and the forgetting.
    internal static class Tranquilizer
    {
        private static readonly Dictionary<int, float> WakeAt = new();   // humanID -> wake time on the play clock
        private static readonly Dictionary<int, float> ForceAt = new();  // woken through koTime but still down
        private static float clock;        // seconds of unpaused play this session
        private static float nextCheck;
        private static double hoursPerSecond = 1.0 / 60.0; // game hours per real second, measured while playing

        internal static void ResetSession()
        {
            WakeAt.Clear();
            ForceAt.Clear();
        }

        // Called every frame (HitmanBehaviour).
        internal static void Update()
        {
            var session = SessionData.Instance;
            float dt = Time.deltaTime;
            if (session == null || !session.play || dt <= 0f) return;
            clock += dt;
            double passed = session.gameTimePassedThisFrame;
            if (passed > 0.0 && passed < 1.0) hoursPerSecond = hoursPerSecond * 0.98 + passed / dt * 0.02;

            if ((WakeAt.Count == 0 && ForceAt.Count == 0) || clock < nextCheck) return;
            nextCheck = clock + 0.5f;

            foreach (var id in new List<int>(WakeAt.Keys))
                if (clock >= WakeAt[id])
                {
                    WakeAt.Remove(id);
                    Wake(id);
                }
            foreach (var id in new List<int>(ForceAt.Keys))
                if (clock >= ForceAt[id])
                {
                    ForceAt.Remove(id);
                    ForceWake(id);
                }
        }

        internal static void Sleep(Citizen citizen, Vector3 position, Vector3 direction)
        {
            float seconds = HitmanConfig.SleepSeconds.Value;
            // Fallback for the game's own timer (a load loses ours): the same span in game time, with some margin.
            float hours = (float)Math.Clamp(hoursPerSecond * seconds * 1.25, 0.05, 6.0);
            Forget(citizen);
            citizen.ai.SetKO(true, position, direction, true, hours, true, 0f);
            Forget(citizen);
            WakeAt[citizen.humanID] = clock + seconds;
            // v0.7: a witness who is darted before going to the police wakes up without the memory.
            if (HitmanConfig.WitnessDartSilences.Value && Heat.KillEvidence.Silence(citizen, "darted"))
                Text.Notify($"{citizen.GetCitizenName()} vai acordar sem lembrar do que viu.", $"{citizen.GetCitizenName()} will wake up without remembering what they saw.");
            Plugin.Logger.LogInfo($"[Weapons] Dart: {Who(citizen)} asleep for {seconds:0}s of play (game timer {hours:0.00} h, {hoursPerSecond * 3600:0} game s per real s)");
        }

        // Saves from 0.5.5 and earlier hold citizens darted for 120 game hours. The game's own knock-outs last minutes,
        // so anyone still due to sleep more than 6 game hours is woken (and made to forget the player) on load.
        internal static void WakeOversleepers()
        {
            var citizens = CityData.Instance != null ? CityData.Instance.citizenDirectory : null;
            var session = SessionData.Instance;
            if (citizens == null || session == null) return;
            int woken = 0;
            foreach (var c in Each(citizens))
            {
                if (c == null || c.isDead || c.ai == null || !c.ai.ko || c.ai.koTime - session.gameTime < 6f) continue;
                Forget(c);
                c.ai.koTime = session.gameTime;
                woken++;
            }
            if (woken > 0) Plugin.Logger.LogInfo($"[Weapons] Woke {woken} citizen(s) left asleep for days by an older dart.");
        }

        internal static int Sleeping => WakeAt.Count;

        // Dev menu: wake every citizen asleep from a dart now.
        internal static int WakeAllNow()
        {
            var ids = new List<int>(WakeAt.Keys);
            WakeAt.Clear();
            foreach (var id in ids) Wake(id);
            return ids.Count;
        }

        // Dev menu: every citizen forgets the player (pursuit, combat, sightings).
        internal static int ForgetEveryone()
        {
            var citizens = CityData.Instance != null ? CityData.Instance.citizenDirectory : null;
            int n = 0;
            foreach (var c in Each(citizens))
            {
                if (c == null || c.isDead || c.ai == null) continue;
                Forget(c);
                n++;
            }
            return n;
        }

        // A dart on a citizen already asleep only clears memory again.
        internal static void Redart(Citizen citizen) => Forget(citizen);

        private static void Wake(int id)
        {
            if (!TryHuman(id, out var human) || human.isDead || human.ai == null || !human.ai.ko) return;
            Forget(human);
            // The game wakes a knocked-out citizen once koTime has passed, with its own get-up.
            human.ai.koTime = SessionData.Instance.gameTime;
            ForceAt[id] = clock + 5f;
            Plugin.Logger.LogInfo($"[Weapons] Dart wears off: {Who(human)}");
        }

        private static void ForceWake(int id)
        {
            if (!TryHuman(id, out var human) || human.isDead || human.ai == null) return;
            if (human.ai.ko)
            {
                Plugin.Logger.LogInfo($"[Weapons] {Who(human)} still down 5 s after koTime; waking through SetKO(false).");
                try { human.ai.SetKO(false, human.transform.position, Vector3.forward, false, 0f, true, 1f); }
                catch (Exception e) { Plugin.Logger.LogWarning($"[Weapons] SetKO(false) failed: {e.Message}"); }
            }
            Forget(human);
        }

        // Drops every trace of the player from this citizen: pursuit, combat, investigation, tracking, what they saw,
        // and the player's own list of pursuers and witnesses.
        internal static void Forget(Human human)
        {
            var player = Player.Instance;
            var ai = human != null ? human.ai : null;
            if (ai == null || player == null) return;

            Try(() => ai.CancelCombat());
            Try(() => ai.CancelPersue());
            Try(() => ai.SetPersuit(false));
            Try(() => ai.SetSeesOnPersuit(false));
            Try(() => ai.ResetInvestigate());
            Try(() => ai.SetReactionState(NewAIController.ReactionState.none));
            Try(() =>
            {
                var tracked = ai.trackedTargets;
                if (tracked == null) return;
                for (int i = tracked.Count - 1; i >= 0; i--)
                    if (tracked[i] != null && tracked[i].actor != null && tracked[i].actor.Pointer == player.Pointer) tracked.RemoveAt(i);
            });
            Try(() => human.ClearSeesIllegal());
            Try(() => player.RemovePersuedBy(human));
            Try(() => human.RemovePersuedBy(player));
            Try(() =>
            {
                var witnesses = player.witnessesToIllegalActivity;
                if (witnesses != null && witnesses.Contains(human)) witnesses.Remove(human);
            });
            Try(() => player.UpdateIllegalStatus());
        }

        private static void Try(Action step)
        {
            try { step(); }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Weapons] Forget step failed: {e.Message}"); }
        }

        private static bool TryHuman(int id, out Human human)
        {
            human = null;
            return CityData.Instance != null && CityData.Instance.GetHuman(id, out human, false) && human != null;
        }
    }
}
