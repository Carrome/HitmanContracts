using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SODHitman.Contracts;
using static SODHitman.Dev.Describe;

namespace SODHitman.Heat
{
    public sealed class WitnessRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float ReportAt { get; set; }
        // pending, reported, silenced
        public string State { get; set; } = "pending";
        public string How { get; set; }
    }

    // One death caused by the player, and what the police know about it.
    public sealed class KillRecord
    {
        public int VictimId { get; set; }
        public string VictimName { get; set; }
        public float Time { get; set; }
        public string Place { get; set; }
        public bool Contract { get; set; }
        public bool PublicPlace { get; set; }
        public bool Camera { get; set; }
        // v0.8: how it was done; natural (poison) and suicide (forged) deaths bring little heat and no crime scene.
        public string Method { get; set; }
        public bool Natural { get; set; }
        public bool Suicide { get; set; }
        // Heat the body brings when found: the kill itself, public place, camera footage.
        public int BodyPoints { get; set; }
        public bool BodyFound { get; set; }
        public float FoundTime { get; set; } = -1f;
        public string FoundBy { get; set; }
        public List<WitnessRecord> Witnesses { get; set; } = new();
    }

    // Pillar 1 of the roadmap (decided 20/10/2026), heat from what the police find out:
    //  - a death adds heat only when the body is found (Death.SetReported); a hidden body adds nothing, and a found
    //    one becomes a crime scene the Enforcers come to
    //  - each witness goes to the police WitnessDelay hours later, unless silenced first (bribed, darted, or dead);
    //    only then does their heat count
    //  - only a live camera that had the player in view counts
    // Heat still never goes down here: stealth only keeps it from going up (decision of 19/10/2026 kept).
    internal static class KillEvidence
    {
        private static HeatState H => ContractStore.Heat;
        private static readonly Random Rng = new();
        private const int MaxRecords = 60;

        internal static bool Enabled => HitmanConfig.StealthEnabled.Value;

        // ---- at the kill ----

        // A live camera in the room that has the player in view (or is tracking them).
        internal static bool CameraSawPlayer(Player player, NewGameLocation loc)
        {
            if (player == null || loc == null || loc.securityCameras == null) return false;
            foreach (var camera in Each(loc.securityCameras))
            {
                if (camera == null) continue;
                try
                {
                    var system = camera.controller != null ? camera.controller.gameObject.GetComponentInChildren<SecuritySystem>() : null;
                    if (system != null)
                    {
                        if (!system.isActive) continue;
                        if (system.trackingTarget != null && system.trackingTarget.Pointer == player.Pointer) return true;
                    }
                    else if (!camera.sw0) continue;
                    var room = camera.node != null ? camera.node.room : null;
                    if (room != null && room == player.currentRoom) return true;
                }
                catch { }
            }
            return false;
        }

        internal static void OnKill(Human victim, bool contract, List<Human> witnesses, bool camera, bool isPublic, NewGameLocation loc, string method = null)
        {
            int basePoints = contract ? HitmanConfig.HeatContractKill.Value : HitmanConfig.HeatOtherKill.Value;
            int bodyPoints = basePoints + (isPublic ? HitmanConfig.HeatPublicPlace.Value : 0) + (camera ? HitmanConfig.HeatCamera.Value : 0);
            bool natural = method == "poison";
            bool suicide = (natural || (method != null && method.StartsWith("garrote"))) && Weapons.Arsenal.SuicideNoteAtHome(victim);
            if (suicide) bodyPoints = HitmanConfig.ArsenalSuicidePoints.Value;
            else if (natural) bodyPoints = HitmanConfig.ArsenalNaturalDeathPoints.Value;
            float now = Text.Now;
            var record = new KillRecord
            {
                VictimId = victim.humanID,
                VictimName = victim.GetCitizenName(),
                Time = now,
                Place = N(loc),
                Contract = contract,
                PublicPlace = isPublic,
                Camera = camera,
                BodyPoints = bodyPoints,
                Method = method,
                Natural = natural && !suicide,
                Suicide = suicide,
            };
            if (suicide) Plugin.Logger.LogInfo($"[Evidence] {Who(victim)}: suicide note at home, the death will pass for suicide.");
            float min = HitmanConfig.WitnessDelayMinHours.Value, max = Math.Max(min, HitmanConfig.WitnessDelayMaxHours.Value);
            foreach (var w in witnesses)
                record.Witnesses.Add(new WitnessRecord { Id = w.humanID, Name = w.GetCitizenName(), ReportAt = now + min + (float)Rng.NextDouble() * (max - min) });

            Consequences.OnWitnesses(witnesses); // a contract target who saw it lays low (v1.1)

            string summary = $"killed {Who(victim)}: {(contract ? "contract" : "other")}, witnesses {witnesses.Count} [{string.Join(", ", witnesses.Select(Who))}], camera {camera}, public {isPublic}, at {N(loc)}";
            if (!Enabled)
            {
                // Without stealth rules everything counts at once (the 0.5 behaviour).
                record.BodyFound = true;
                foreach (var w in record.Witnesses) w.State = "reported";
                Add(record);
                HeatSystem.Change(bodyPoints + witnesses.Count * HitmanConfig.HeatPerWitness.Value, summary);
                return;
            }

            Add(record);
            Plugin.Logger.LogInfo($"[Evidence] {summary}; body worth {bodyPoints} pts when found");
            if (!HitmanConfig.StealthHints.Value) return;
            if (witnesses.Count > 0)
                Text.Notify(
                    witnesses.Count == 1
                        ? $"{witnesses[0].GetCitizenName()} viu tudo. Em cerca de {min:0}–{max:0} h vai à polícia, a não ser que mude de ideia."
                        : $"{witnesses.Count} pessoas viram tudo. Em cerca de {min:0}–{max:0} h vão à polícia, a não ser que mudem de ideia.",
                    witnesses.Count == 1
                        ? $"{witnesses[0].GetCitizenName()} saw everything. In about {min:0}–{max:0} h they will go to the police, unless they change their mind."
                        : $"{witnesses.Count} people saw everything. In about {min:0}–{max:0} h they will go to the police, unless they change their minds.");
            if (camera)
                Text.Notify("Uma câmera de segurança registrou você.", "A security camera caught you on tape.");
        }

        private static void Add(KillRecord record)
        {
            H.KillRecords ??= new List<KillRecord>();
            H.KillRecords.Add(record);
            if (H.KillRecords.Count > MaxRecords) H.KillRecords.RemoveRange(0, H.KillRecords.Count - MaxRecords);
        }

        // ---- body found ----

        internal static KillRecord RecordFor(int victimId) =>
            H.KillRecords?.LastOrDefault(r => r.VictimId == victimId);

        private static void BodyFound(KillRecord record, string foundBy, Human.Death death)
        {
            if (record == null || record.BodyFound) return;
            record.BodyFound = true;
            record.FoundTime = Text.Now;
            record.FoundBy = foundBy;
            HeatSystem.Change(record.BodyPoints, $"body of {record.VictimName} found by {foundBy}");
            string looksPt = record.Suicide ? " Parece suicídio." : record.Natural ? " Parece morte natural." : "";
            string looksEn = record.Suicide ? " It looks like suicide." : record.Natural ? " It looks like natural causes." : "";
            Text.Notify($"O corpo de {record.VictimName} foi encontrado.{looksPt} (heat +{record.BodyPoints})",
                        $"{record.VictimName}'s body was found.{looksEn} (heat +{record.BodyPoints})");
            Consequences.OnBodyFound(record);

            if (record.Natural || record.Suicide) return; // nothing for the police to investigate
            if (!HitmanConfig.StealthCrimeScene.Value || GameplayController.Instance == null) return;
            try
            {
                if (death == null && CityData.Instance.GetHuman(record.VictimId, out var victim, false)) death = victim.death;
                var where = death != null ? death.GetDeathLocation() : null;
                if (where != null)
                {
                    GameplayController.Instance.CallEnforcers(where, true, false, 0f);
                    Plugin.Logger.LogInfo($"[Evidence] Crime scene: Enforcers called to {N(where)}");
                }
            }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Evidence] Crime scene call failed: {e.Message}"); }
        }

        [HarmonyPatch(typeof(Human.Death), nameof(Human.Death.SetReported))]
        internal static class Death_SetReported
        {
            static void Postfix(Human.Death __instance, Human newFoundBy, Human.Death.ReportType newReportType)
            {
                if (!Enabled || ContractStore.Loading || __instance == null) return;
                if (newFoundBy != null && newFoundBy.isPlayer) return;
                var record = RecordFor(__instance.victim);
                if (record == null || record.BodyFound) return;
                Plugin.Logger.LogInfo($"[Evidence] Body of {record.VictimName} found by {Who(newFoundBy)} ({newReportType})");
                BodyFound(record, newFoundBy != null ? newFoundBy.GetCitizenName() : newReportType.ToString(), __instance);
            }
        }

        // ---- witnesses ----

        internal static IEnumerable<(KillRecord record, WitnessRecord witness)> Pending()
        {
            foreach (var r in H.KillRecords ?? new List<KillRecord>())
                foreach (var w in r.Witnesses)
                    if (w.State == "pending") yield return (r, w);
        }

        internal static bool IsPendingWitness(Human h) =>
            h != null && Pending().Any(p => p.witness.Id == h.humanID);

        // A pending witness keeps quiet for good. False when they were not one.
        internal static bool Silence(Human h, string how)
        {
            if (h == null) return false;
            bool any = false;
            foreach (var (record, witness) in Pending().ToList())
            {
                if (witness.Id != h.humanID) continue;
                witness.State = "silenced";
                witness.How = how;
                any = true;
                Plugin.Logger.LogInfo($"[Witness] {witness.Name} silenced ({how}) about {record.VictimName}");
            }
            return any;
        }

        private static void Report(KillRecord record, WitnessRecord witness)
        {
            witness.State = "reported";
            HeatSystem.Change(HitmanConfig.HeatPerWitness.Value, $"witness {witness.Name} reported the death of {record.VictimName}");
            Text.Notify($"{witness.Name} contou à polícia o que viu. (heat +{HitmanConfig.HeatPerWitness.Value})",
                        $"{witness.Name} told the police what they saw. (heat +{HitmanConfig.HeatPerWitness.Value})");
            RefreshLastKillSeen();
            // The police now know there is a body.
            if (!record.BodyFound) BodyFound(record, witness.Name, null);
        }

        // A witness who refused the player's money goes to the police at once.
        internal static void ReportNow(Human h)
        {
            if (h == null) return;
            foreach (var (record, witness) in Pending().ToList())
                if (witness.Id == h.humanID) Report(record, witness);
        }

        internal static void Tick(float now)
        {
            if (!Enabled) return;
            foreach (var (record, witness) in Pending().ToList())
            {
                Human h = null;
                if (CityData.Instance == null || !CityData.Instance.GetHuman(witness.Id, out h, false) || h == null || h.isDead)
                {
                    witness.State = "silenced";
                    witness.How = "dead";
                    Plugin.Logger.LogInfo($"[Witness] {witness.Name} is dead and will never talk.");
                    continue;
                }
                if (now < witness.ReportAt) continue;
                if (h.ai != null && h.ai.ko)
                {
                    witness.ReportAt = now + 0.25f; // asleep or knocked out: talks once awake
                    continue;
                }
                Report(record, witness);
            }
        }

        // The interrogation's "were you there?" question: what the police could know about the last kill.
        internal static bool LastKillKnown()
        {
            var last = H.KillRecords?.LastOrDefault();
            if (last == null) return H.LastKillSeen;
            return last.Camera || last.Witnesses.Any(w => w.State == "reported");
        }

        private static void RefreshLastKillSeen() => H.LastKillSeen = LastKillKnown();

        // ---- the corrupt cop's packages (1.2.2) ----
        // "now": heat the evidence already brought; "later": heat it would still bring (witnesses who have not talked
        // yet, footage of bodies nobody has found). An erased statement is State "erased": it no longer counts anywhere.

        private static IEnumerable<KillRecord> Records => H.KillRecords ?? new List<KillRecord>();

        internal static (int now, int later) WitnessValue()
        {
            int now = 0, later = 0, per = HitmanConfig.HeatPerWitness.Value;
            foreach (var r in Records)
                foreach (var w in r.Witnesses)
                {
                    if (w.State == "reported") now += per;
                    else if (w.State == "pending") later += per;
                }
            return (now, later);
        }

        internal static (int now, int later) CameraValue()
        {
            int now = 0, later = 0, per = HitmanConfig.HeatCamera.Value;
            foreach (var r in Records)
            {
                if (!r.Camera) continue;
                if (r.BodyFound) now += per;
                else later += per;
            }
            return (now, later);
        }

        // Everything on file that has not brought heat yet.
        internal static int FutureValue() =>
            WitnessValue().later + Records.Where(r => !r.BodyFound).Sum(r => r.BodyPoints);

        // Returns the heat to take off now.
        internal static int EraseWitnesses(string how)
        {
            int removed = 0, per = HitmanConfig.HeatPerWitness.Value;
            foreach (var r in Records)
                foreach (var w in r.Witnesses)
                {
                    if (w.State == "reported") removed += per;
                    else if (w.State != "pending") continue;
                    w.State = w.State == "reported" ? "erased" : "silenced";
                    w.How = how;
                }
            RefreshLastKillSeen();
            Plugin.Logger.LogInfo($"[Evidence] Witness statements erased ({how}): heat -{removed}");
            return removed;
        }

        internal static int EraseCameras(string how)
        {
            int removed = 0, per = HitmanConfig.HeatCamera.Value;
            foreach (var r in Records)
            {
                if (!r.Camera) continue;
                r.Camera = false;
                if (r.BodyFound) removed += per;
                else r.BodyPoints = Math.Max(0, r.BodyPoints - per);
            }
            RefreshLastKillSeen();
            Plugin.Logger.LogInfo($"[Evidence] Camera footage erased ({how}): heat -{removed}");
            return removed;
        }

        // The player's name off the file: nothing recorded so far will ever bring heat again.
        internal static void EraseAll(string how)
        {
            EraseWitnesses(how);
            EraseCameras(how);
            foreach (var r in Records)
                if (!r.BodyFound) r.BodyPoints = 0;
            RefreshLastKillSeen();
            Plugin.Logger.LogInfo($"[Evidence] The whole file erased ({how}).");
        }

        // ---- dev menu ----

        internal static string Summary()
        {
            var records = H.KillRecords ?? new List<KillRecord>();
            int pending = Pending().Count();
            int hidden = records.Count(r => !r.BodyFound);
            return Text.L($"testemunhas pendentes {pending}, corpos não achados {hidden}", $"pending witnesses {pending}, bodies not found {hidden}");
        }

        internal static void DevListPending()
        {
            var list = Pending().ToList();
            if (list.Count == 0)
            {
                Text.Notify("[Menu] Nenhuma testemunha pendente.", "[Menu] No pending witness.");
                return;
            }
            foreach (var (record, w) in list)
                Text.Notify($"[Menu] {w.Name} viu a morte de {record.VictimName}; denuncia às {Text.Clock(w.ReportAt)}.",
                            $"[Menu] {w.Name} saw {record.VictimName} die; reports at {Text.Clock(w.ReportAt)}.");
        }

        internal static void DevReportAll()
        {
            float now = Text.Now;
            foreach (var (_, w) in Pending()) w.ReportAt = now;
            Tick(now);
        }

        internal static int DevSilenceAll()
        {
            int n = 0;
            foreach (var (_, w) in Pending().ToList())
            {
                w.State = "silenced";
                w.How = "dev menu";
                n++;
            }
            return n;
        }

        internal static void DevFindLastBody()
        {
            var record = H.KillRecords?.LastOrDefault(r => !r.BodyFound);
            if (record == null) Text.Notify("[Menu] Nenhum corpo escondido.", "[Menu] No hidden body.");
            else BodyFound(record, "dev menu", null);
        }

        internal static void DevShowLast()
        {
            var r = H.KillRecords?.LastOrDefault();
            if (r == null)
            {
                Text.Notify("[Menu] Nenhuma morte registrada.", "[Menu] No kill recorded.");
                return;
            }
            Text.Notify(
                $"[Menu] {r.VictimName} em {r.Place}, {Text.Clock(r.Time)}: corpo {(r.BodyFound ? $"achado por {r.FoundBy}" : "não achado")}, câmera {(r.Camera ? "sim" : "não")}, " +
                $"testemunhas {string.Join(", ", r.Witnesses.Select(w => $"{w.Name} ({w.State})"))}.",
                $"[Menu] {r.VictimName} at {r.Place}, {Text.Clock(r.Time)}: body {(r.BodyFound ? $"found by {r.FoundBy}" : "not found")}, camera {(r.Camera ? "yes" : "no")}, " +
                $"witnesses {string.Join(", ", r.Witnesses.Select(w => $"{w.Name} ({w.State})"))}.");
        }
    }
}
