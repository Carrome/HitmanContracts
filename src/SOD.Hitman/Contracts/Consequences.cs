using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using static SODHitman.Dev.Describe;

namespace SODHitman.Contracts
{
    // A killer on the way, sent by a client or by someone close to a victim.
    public sealed class PendingHunter
    {
        public float At { get; set; }
        public string Reason { get; set; }
        public string MessagePt { get; set; }
        public string MessageEn { get; set; }
    }

    // A client who kept the player's money. Killing them collects the debt: the chained contract of v1.1.
    public sealed class Grudge
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public int Owed { get; set; }
        public int JobId { get; set; }
    }

    // A contract target laying low at home (the game's LayLow goal) until a game time.
    public sealed class ScaredTarget
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Until { get; set; }
    }

    // v1.1 of the roadmap: consequences. Twists only come from clients and targets (decided 20/10/2026); the
    // corrupt cop never betrays and heat still only goes down through him and the interrogation.
    //  - traitor client at the hand-in (TraitorClientChance, never on the first job), one of:
    //      nopay:  keeps the rest of the money; killing them collects it (Grudge)
    //      police: calls the Enforcers to the hand-in (+TraitorPoliceHeat)
    //      hunter: sends a killer to tie up loose ends, hours later
    //  - revenge: once a victim's body is found, someone close to them (partner, housemate, friend) may hire a killer
    //  - scared target: a target who refused a bribe or saw the player kill lays low at home for ScaredTargetHours
    internal static class Consequences
    {
        internal const string NoPay = "nopay", Police = "police", Hunter = "hunter", None = "none";

        private static Heat.HeatState H => ContractStore.Heat;
        private static readonly Random Rng = new();
        private static readonly Dictionary<int, NewAIGoal> LayLowGoals = new();

        // Dev menu: the next hand-in betrays the player this way, chance and first-job rule aside.
        internal static string ForceNext;

        internal static bool Enabled => HitmanConfig.ConsequencesEnabled.Value;

        internal static void ResetSession()
        {
            LayLowGoals.Clear();
            ForceNext = null;
        }

        // ---- traitor client ----

        private static SideJob JobOf(Case c)
        {
            if (c == null) return null;
            foreach (var job in ContractRegistry.AllJobs())
                if (ContractRegistry.IsHitmanJob(job) && job.thisCase != null && job.thisCase.Pointer == c.Pointer) return job;
            return null;
        }

        private static string Roll(SideJob job, ContractState state)
        {
            if (ForceNext != null)
            {
                var forced = ForceNext;
                ForceNext = null;
                return forced;
            }
            // The campaign's client has a story of their own (Campaign.cs).
            if (!Enabled || !ContractRegistry.IsKillContract(job) || state.BribedAway || H.ContractsDone == 0 || job.jobID == H.CampaignJobId) return None;
            if (Rng.NextDouble() >= HitmanConfig.TraitorChance.Value) return None;
            return Rng.Next(3) switch { 0 => NoPay, 1 => Police, _ => Hunter };
        }

        // Case.Resolve pays the case reward itself (GameplayController.AddMoney "case reward"), and the rating comes
        // with SideJob.Complete. The betrayal is decided before both, so a client who keeps the money pays nothing.
        [HarmonyPatch(typeof(Case), nameof(Case.Resolve))]
        internal static class Case_Resolve
        {
            static void Prefix(Case __instance, out int __state)
            {
                __state = -1;
                try
                {
                    var job = JobOf(__instance);
                    if (job == null) return;
                    var state = ContractStore.Get(job);
                    if (state.Betrayal != null || !state.KillRecorded) return;
                    state.Betrayal = Roll(job, state);
                    if (state.Betrayal == None) return;
                    Plugin.Logger.LogInfo($"[Twist] Job #{job.jobID}: the client {Who(job.poster)} betrays the player ({state.Betrayal})");
                    __state = GameplayController.Instance != null ? GameplayController.Instance.money : 0;
                    if (state.Betrayal == NoPay)
                    {
                        var question = ContractRegistry.KillQuestion(job);
                        if (question != null) question.reward = 0;
                    }
                }
                catch (Exception e) { Plugin.Logger.LogWarning($"[Twist] Resolve prefix failed: {e.Message}"); }
            }

            static void Postfix(Case __instance, int __state)
            {
                if (__state < 0) return;
                try
                {
                    var job = JobOf(__instance);
                    if (job != null) Betray(job, ContractStore.Get(job), __state);
                }
                catch (Exception e) { Plugin.Logger.LogWarning($"[Twist] Resolve postfix failed: {e.Message}"); }
            }
        }

        private static void Betray(SideJob job, ContractState state, int moneyBefore)
        {
            var client = job.poster;
            string name = client != null ? client.GetCitizenName() : Text.L("O cliente", "The client");
            switch (state.Betrayal)
            {
                case NoPay:
                {
                    var gc = GameplayController.Instance;
                    int paidNow = gc != null ? gc.money - moneyBefore : 0;
                    if (paidNow > 0) gc.AddMoney(-paidNow, false, "Hitman: the client kept the money");
                    int owed = Math.Max(0, state.Payment - state.DepositPaid);
                    Plugin.Logger.LogInfo($"[Twist] Job #{job.jobID}: {name} kept Cr{owed} (took back Cr{Math.Max(0, paidNow)} the game paid)");
                    if (client != null && !client.isDead && owed > 0)
                    {
                        H.Grudges ??= new List<Grudge>();
                        H.Grudges.Add(new Grudge { ClientId = client.humanID, ClientName = name, Owed = owed, JobId = job.jobID });
                        Text.Notify($"{name} ficou com o resto do seu dinheiro (Cr{owed}) e sumiu. Cobre do seu jeito: se {name} morrer, o dinheiro é seu.",
                                    $"{name} kept the rest of your money (Cr{owed}) and vanished. Collect it your way: if {name} dies, the money is yours.");
                    }
                    else
                        Text.Notify($"{name} não pagou o resto do serviço.", $"{name} never paid the rest of the job.");
                    break;
                }
                case Police:
                {
                    int heat = HitmanConfig.TraitorPoliceHeat.Value;
                    Heat.HeatSystem.Change(heat, $"client {name} called the police at the hand-in of job #{job.jobID}");
                    NewGameLocation where = Player.Instance != null ? Player.Instance.currentGameLocation : null;
                    if (where == null && client != null) where = client.home;
                    if (where != null && GameplayController.Instance != null)
                    {
                        GameplayController.Instance.CallEnforcers(where, false, false, 0f);
                        Plugin.Logger.LogInfo($"[Twist] Enforcers called to {N(where)}");
                    }
                    Text.Notify($"{name} pagou, mas chamou a polícia para você. Saia daí! (heat +{heat})",
                                $"{name} paid, but called the police on you. Get out of there! (heat +{heat})");
                    break;
                }
                case Hunter:
                    // No warning now: the player only finds out when the killer shows up.
                    Schedule($"client {name} of job #{job.jobID} cleaning up loose ends",
                             $"{name} não quer pontas soltas. Alguém foi pago para te apagar.",
                             $"{name} doesn't want loose ends. Someone has been paid to erase you.");
                    break;
            }
        }

        // ---- the debt (chained contract) ----

        internal static bool IsGrudgeTarget(Human victim) =>
            victim != null && H.Grudges != null && H.Grudges.Any(g => g.ClientId == victim.humanID);

        // Every death the player causes (HeatSystem.OnPlayerKill).
        internal static void OnPlayerKill(Human victim)
        {
            if (!IsGrudgeTarget(victim)) return;
            foreach (var g in H.Grudges.Where(g => g.ClientId == victim.humanID).ToList())
            {
                H.Grudges.Remove(g);
                GameplayController.Instance?.AddMoney(g.Owed, true, "Hitman: debt collected");
                H.Earned += g.Owed;
                Plugin.Logger.LogInfo($"[Twist] Debt of {g.ClientName} (job #{g.JobId}) collected: Cr{g.Owed}");
                Text.Notify($"Dívida cobrada: {g.ClientName} não vai enganar mais ninguém. Cr{g.Owed} são seus.",
                            $"Debt collected: {g.ClientName} won't cheat anyone again. Cr{g.Owed} is yours.");
                Career.AddReputation(1, "debt collected");
            }
        }

        // ---- revenge ----

        // KillEvidence.BodyFound: the police, and the victim's people, now know.
        internal static void OnBodyFound(Heat.KillRecord record)
        {
            if (!Enabled || record == null || record.Natural || record.Suicide) return;
            bool evidence = record.Camera || record.Witnesses.Any(w => w.State == "reported");
            double chance = HitmanConfig.RevengeChance.Value * (evidence ? 2.0 : 1.0);
            if (Rng.NextDouble() >= chance) return;
            var avenger = CloseOne(record.VictimId);
            if (avenger == null)
            {
                Plugin.Logger.LogInfo($"[Twist] Revenge rolled for {record.VictimName}, but nobody close is alive.");
                return;
            }
            string who = avenger.GetCitizenName();
            Schedule($"{who} avenging {record.VictimName}",
                     $"{who} não perdoou a morte de {record.VictimName}. Alguém foi pago para te pegar.",
                     $"{who} never forgave {record.VictimName}'s death. Someone has been paid to get you.");
        }

        // The victim's partner, else the closest friend, housemate or lover they had.
        private static Human CloseOne(int victimId)
        {
            if (CityData.Instance == null || !CityData.Instance.GetHuman(victimId, out var victim, false) || victim == null) return null;
            if (victim.partner != null && !victim.partner.isDead) return victim.partner;
            Human best = null;
            float bestLike = 0.5f;
            foreach (var a in Each(victim.acquaintances))
            {
                if (a == null || a.with == null || a.with.isDead || a.with.isPlayer || a.like <= bestLike) continue;
                foreach (var c in Each(a.connections))
                {
                    if (c != Acquaintance.ConnectionType.friend && c != Acquaintance.ConnectionType.housemate &&
                        c != Acquaintance.ConnectionType.lover && c != Acquaintance.ConnectionType.paramour) continue;
                    best = a.with;
                    bestLike = a.like;
                    break;
                }
            }
            return best;
        }

        private static void Schedule(string reason, string pt, string en, float delayHours = -1f)
        {
            if (delayHours < 0f)
            {
                float min = HitmanConfig.RevengeDelayMinHours.Value, max = Math.Max(min, HitmanConfig.RevengeDelayMaxHours.Value);
                delayHours = min + (float)Rng.NextDouble() * (max - min);
            }
            H.PendingHunters ??= new List<PendingHunter>();
            var p = new PendingHunter { At = Text.Now + delayHours, Reason = reason, MessagePt = pt, MessageEn = en };
            H.PendingHunters.Add(p);
            Plugin.Logger.LogInfo($"[Twist] Killer scheduled at {Text.Clock(p.At)}: {reason}");
        }

        // ---- scared targets ----

        // A target who saw the player kill (KillEvidence.OnKill) lays low.
        internal static void OnWitnesses(List<Human> witnesses)
        {
            if (!Enabled || witnesses == null || witnesses.Count == 0) return;
            foreach (var job in ContractRegistry.ActiveHitmanJobs())
                if (ContractRegistry.IsKillContract(job) && job.purp != null && witnesses.Any(w => w.humanID == job.purp.humanID))
                    Scare(job.purp, "saw the player kill");
        }

        internal static void Scare(Human target, string why)
        {
            if (!Enabled || target == null || target.isDead || target.isPlayer) return;
            H.Scared ??= new List<ScaredTarget>();
            float until = Text.Now + HitmanConfig.ScaredHours.Value;
            var entry = H.Scared.FirstOrDefault(s => s.Id == target.humanID);
            if (entry != null) entry.Until = Math.Max(entry.Until, until);
            else H.Scared.Add(new ScaredTarget { Id = target.humanID, Name = target.GetCitizenName(), Until = until });
            Plugin.Logger.LogInfo($"[Twist] {Who(target)} is scared ({why}) and lays low until {Text.Clock(until)}");
            Text.Notify($"{target.GetCitizenName()} ficou com medo e vai se trancar em casa por um tempo.",
                        $"{target.GetCitizenName()} got scared and will lock themselves at home for a while.");
        }

        private static void LayLow(Human target)
        {
            if (LayLowGoals.ContainsKey(target.humanID) || target.ai == null || target.home == null) return;
            if (target.ai.inFleeState || target.ai.ko) return; // after the flight, once awake
            var preset = RoutineControls.Instance != null ? RoutineControls.Instance.layLow : null;
            if (preset == null) return;
            try
            {
                // As the game does for the chapter's notewriter: no trigger time, no duration, the home as the place.
                LayLowGoals[target.humanID] = target.ai.CreateNewGoal(preset, 0f, 0f, null, null, target.home, null, null, -2);
                Plugin.Logger.LogInfo($"[Twist] {Who(target)} lays low at {N(target.home)}");
            }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Twist] LayLow failed for {Who(target)}: {e.Message}"); }
        }

        private static void Calm(ScaredTarget s)
        {
            if (!LayLowGoals.TryGetValue(s.Id, out var goal)) return;
            LayLowGoals.Remove(s.Id);
            try { goal?.Remove(); }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Twist] Removing LayLow from {s.Name} failed: {e.Message}"); }
        }

        // ---- every game minute ----

        internal static void Tick(float now)
        {
            if (CityData.Instance == null) return;

            foreach (var p in (H.PendingHunters ?? new List<PendingHunter>()).Where(p => now >= p.At).ToList())
            {
                H.PendingHunters.Remove(p);
                Text.Notify(p.MessagePt, p.MessageEn);
                Heat.HeatSystem.SendPersonalHunter(p.Reason);
            }

            foreach (var g in (H.Grudges ?? new List<Grudge>()).ToList())
            {
                if (!CityData.Instance.GetHuman(g.ClientId, out var client, false) || client == null || !client.isDead) continue;
                if (Heat.HeatSystem.Counted(client)) continue; // the player's kill: collected in OnPlayerKill
                H.Grudges.Remove(g);
                Plugin.Logger.LogInfo($"[Twist] {g.ClientName} died of something else; the debt of job #{g.JobId} is lost.");
                Text.Notify($"{g.ClientName} morreu, e a sua dívida morreu junto.", $"{g.ClientName} died, and your debt died with them.");
            }

            foreach (var s in (H.Scared ?? new List<ScaredTarget>()).ToList())
            {
                bool alive = CityData.Instance.GetHuman(s.Id, out var target, false) && target != null && !target.isDead;
                if (!alive || now >= s.Until)
                {
                    Calm(s);
                    H.Scared.Remove(s);
                    if (alive) Plugin.Logger.LogInfo($"[Twist] {s.Name} is no longer scared.");
                    continue;
                }
                LayLow(target);
            }
        }

        // ---- dev menu ----

        internal static string Status() =>
            Text.L($"dívidas {H.Grudges?.Count ?? 0}, caçadores a caminho {H.PendingHunters?.Count ?? 0}, alvos escondidos {H.Scared?.Count ?? 0}" +
                   (ForceNext != null ? $", próxima entrega: {ForceNext}" : ""),
                   $"debts {H.Grudges?.Count ?? 0}, killers on the way {H.PendingHunters?.Count ?? 0}, targets laying low {H.Scared?.Count ?? 0}" +
                   (ForceNext != null ? $", next hand-in: {ForceNext}" : ""));

        internal static void DevForceNext(string how)
        {
            ForceNext = how;
            Text.Notify($"[Menu] A próxima entrega de contrato vai ter traição: {how}.", $"[Menu] The next contract hand-in will be a betrayal: {how}.");
        }

        internal static void DevScareTarget()
        {
            var job = ContractRegistry.CurrentAcceptedJob();
            if (job == null || job.purp == null) Text.Notify("[Menu] Nenhum contrato aceito com alvo.", "[Menu] No accepted contract with a target.");
            else Scare(job.purp, "dev menu");
        }

        internal static void DevRevengeNow() =>
            Schedule("dev menu revenge", "[Menu] Alguém próximo de uma vítima pagou para te pegar.", "[Menu] Someone close to a victim paid to get you.", 0f);

        internal static void DevList()
        {
            foreach (var g in H.Grudges ?? new List<Grudge>())
                Text.Notify($"[Menu] {g.ClientName} te deve Cr{g.Owed} (contrato #{g.JobId}).", $"[Menu] {g.ClientName} owes you Cr{g.Owed} (job #{g.JobId}).");
            foreach (var p in H.PendingHunters ?? new List<PendingHunter>())
                Text.Notify($"[Menu] Caçador às {Text.Clock(p.At)}: {p.Reason}.", $"[Menu] Killer at {Text.Clock(p.At)}: {p.Reason}.");
            foreach (var s in H.Scared ?? new List<ScaredTarget>())
                Text.Notify($"[Menu] {s.Name} escondido até {Text.Clock(s.Until)}.", $"[Menu] {s.Name} laying low until {Text.Clock(s.Until)}.");
            Text.Notify("[Menu] " + Status(), "[Menu] " + Status());
        }
    }
}
