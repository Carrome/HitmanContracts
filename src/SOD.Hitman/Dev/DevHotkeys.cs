using System;
using System.Collections.Generic;
using HarmonyLib;
using SODHitman.Contracts;
using UnityEngine;
using static SODHitman.Dev.Describe;

namespace SODHitman.Dev
{
    internal static class DevHotkeys
    {
        private sealed class Binding
        {
            public Hotkey Key;
            public string Label;
            public Action Run;
            public bool WasDown;
        }

        private static readonly List<Binding> Bindings = new();

        internal static void Init()
        {
            Add(HitmanConfig.KeyMenu.Value, "test menu", DevMenu.Toggle);
            Add(HitmanConfig.KeyDump.Value, "write dump", () => PresetDumper.DumpAll("hotkey"));
            Add(HitmanConfig.KeyForcePost.Value, "force post " + HitmanConfig.DevForcePostPreset.Value, ForcePost);
            Add(HitmanConfig.KeyListJobs.Value, "list jobs", ListJobs);
            Add(HitmanConfig.KeyKillTarget.Value, "kill target (lethal damage)", () => KillTarget(viaMurder: false));
            Add(HitmanConfig.KeyMurderTarget.Value, "kill target (Human.Murder)", () => KillTarget(viaMurder: true));
            Add(HitmanConfig.KeyHeatUp.Value, "heat +10", () => ChangeHeat(10));
            Add(HitmanConfig.KeyHeatDown.Value, "heat -10", () => ChangeHeat(-10));
            Add(HitmanConfig.KeyGiveWeapons.Value, "give both pistols", () =>
            {
                if (!CityReady()) return;
                Weapons.WeaponShops.GiveBoth();
                Text.Notify("[Dev] As duas pistolas foram para o inventário.", "[Dev] Both pistols are in your inventory.");
            });
            Add(HitmanConfig.KeyCopCall.Value, "corrupt cop calls now", () =>
            {
                if (!CityReady()) return;
                Heat.CorruptCop.CallNow();
                Text.Notify("[Dev] O policial corrupto está ligando.", "[Dev] The corrupt cop is calling.");
            });
            Add(HitmanConfig.KeyInterrogate.Value, "enforcers come to interrogate", () =>
            {
                if (!CityReady()) return;
                Heat.Interrogation.ForceVisit();
                Text.Notify("[Dev] Enforcers vão vir te interrogar (fique em casa).", "[Dev] Enforcers are coming to interrogate you (stay at home).");
            });
        }

        internal static void ChangeHeat(int delta)
        {
            if (!CityReady()) return;
            Heat.HeatSystem.Change(delta, "dev hotkey");
            ShowHeatOnScreen();
        }

        internal static void ShowHeatOnScreen()
        {
            var h = ContractStore.Heat;
            if (Heat.KillEvidence.Enabled)
                Text.Notify($"[Dev] Evidências: {Heat.KillEvidence.Summary()}. Reputação {h.Reputation}.",
                            $"[Dev] Evidence: {Heat.KillEvidence.Summary()}. Reputation {h.Reputation}.");
            Text.Notify(
                $"[Dev] Heat {h.Points} pts (nível {h.Level}). Mortes {h.Kills} (contrato {h.ContractKills}). Caçadores {h.HunterIds.Count}." +
                (h.NextInvestigation >= 0 ? $" Próx. investigação {Text.Clock(h.NextInvestigation)}." : ""),
                $"[Dev] Heat {h.Points} pts (level {h.Level}). Kills {h.Kills} (contract {h.ContractKills}). Hunters {h.HunterIds.Count}." +
                (h.NextInvestigation >= 0 ? $" Next investigation {Text.Clock(h.NextInvestigation)}." : ""));
            if (h.CopId >= 0)
                Text.Notify(
                    $"[Dev] Policial: {h.CopName} #{h.CopId}, número {h.CopNumber}, contato {(h.CopContacted ? "feito" : "não")}." +
                    (h.PendingDropNumber >= 0 ? $" Envelope Cr{h.PendingCost} até {Text.Clock(h.PendingDeadline)}." : "") +
                    $" Ponto Cr{Heat.CorruptCop.PricePerPoint()}.",
                    $"[Dev] Cop: {h.CopName} #{h.CopId}, number {h.CopNumber}, contacted {(h.CopContacted ? "yes" : "no")}." +
                    (h.PendingDropNumber >= 0 ? $" Envelope Cr{h.PendingCost} by {Text.Clock(h.PendingDeadline)}." : "") +
                    $" Point Cr{Heat.CorruptCop.PricePerPoint()}.");
        }

        private static void Add(string keyText, string label, Action run)
        {
            var key = Hotkey.Parse(keyText);
            if (key == null)
            {
                Plugin.Logger.LogWarning($"[Dev] Invalid hotkey '{keyText}' for {label}, disabled.");
                return;
            }
            Bindings.Add(new Binding { Key = key, Label = label, Run = run });
            Plugin.Logger.LogInfo($"[Dev] {key.Text} = {label}");
        }

        internal static void Update()
        {
            if (Bindings.Count == 0 || !Native.IsGameForeground()) return;
            foreach (var b in Bindings)
            {
                bool down = b.Key.IsPressed();
                if (down && !b.WasDown)
                {
                    try { b.Run(); }
                    catch (Exception e) { Plugin.Logger.LogError($"[Dev] {b.Label} failed: {e}"); }
                }
                b.WasDown = down;
            }
        }

        internal static bool CityReady()
        {
            if (SessionData.Instance != null && SessionData.Instance.startedGame && SideJobController.Instance != null) return true;
            Plugin.Logger.LogWarning("[Dev] No city loaded yet.");
            return false;
        }

        // F8: one more contract of Dev.ForcePostPreset now (Contracts.Postings does the work).
        internal static void ForcePost()
        {
            if (!CityReady()) return;
            var target = HitmanConfig.DevForcePostPreset.Value;
            var job = Postings.Request(target, "dev", AnnouncePosted, out var failure);
            if (job == null)
            {
                Plugin.Logger.LogWarning($"[Dev] Force post '{target}' failed: {failure}");
                Text.Notify($"[Dev] Não deu para postar '{target}': {failure}. Tente de novo.",
                            $"[Dev] Could not post '{target}': {failure}. Try again.");
                return;
            }
            if (job.state != SideJob.JobState.posted)
                Text.Notify($"[Dev] Contrato #{job.jobID} criado. O anúncio vai para um quadro em instantes.",
                            $"[Dev] Contract #{job.jobID} created. Its note goes up on a board in a moment.");
        }

        private static void AnnouncePosted(SideJob job) =>
            Text.Notify($"[Dev] Contrato #{job.jobID} postado em {Place(job.post)}.",
                        $"[Dev] Contract #{job.jobID} posted at {Place(job.post)}.");

        internal static void ListJobs()
        {
            if (!CityReady()) return;
            var jobs = ContractRegistry.AllJobs();
            int posted = 0;
            foreach (var j in jobs)
            {
                if (j.state != SideJob.JobState.posted) continue;
                posted++;
                var tag = ContractRegistry.IsHitmanJob(j) ? "[HITMAN] " : j.accepted ? "[ACCEPTED] " : "";
                Plugin.Logger.LogInfo($"[Dev] {tag}{Job(j)} reward={j.reward} post={Where(j.post)}");
                if (ContractStore.TryGet(j, out var s))
                    Plugin.Logger.LogInfo($"[Dev]     contract: payment={s.Payment} meet={(s.MeetTime >= 0 ? Text.Clock(s.MeetTime) : "-")} " +
                                          $"deadline={(s.Deadline >= 0 ? Text.Clock(s.Deadline) : "-")} info={s.InfoAcquired} kill={s.KillRecorded} late={s.FailedLate} " +
                                          $"bribe={(s.BribeAttempted ? s.BribeOutcome : "-")} | {s.PaymentBreakdown}");
            }
            Plugin.Logger.LogInfo($"[Dev] {posted} posted of {jobs.Count} tracked jobs. gameTime={SessionData.Instance.gameTime:0.00}");
            PresetDumper.DumpJobs();
            ShowContractsOnScreen(jobs);
            ShowHeatOnScreen();
        }

        // Tester aid: where each contract is posted, who the target is, the price and the bribe verdict.
        private static void ShowContractsOnScreen(List<SideJob> jobs)
        {
            int shown = 0;
            foreach (var j in jobs)
            {
                if (!ContractRegistry.IsHitmanJob(j) || !ContractRegistry.IsActive(j)) continue;
                shown++;
                var occupation = j.purp != null ? j.purp.job : null;
                float salary = Payment.AnnualSalary(occupation);
                bool employed = occupation != null && occupation.employer != null;
                int payment = ContractStore.Get(j).Payment;
                if (payment < 0) payment = Payment.Calculate(j, out _);
                bool accepts = Bribe.WouldAccept(salary, employed, payment, out _);
                var occupationName = occupation != null ? N(occupation.preset) : "-";
                Text.Notify(
                    $"[Dev] Contrato #{j.jobID}{(j.accepted ? " (aceito)" : "")}: anúncio em {Place(j.post)}. Alvo {Who(j.purp)} ({occupationName}). Cr{payment}. Suborno: {(accepts ? "aceitaria" : "recusaria")}.",
                    $"[Dev] Contract #{j.jobID}{(j.accepted ? " (accepted)" : "")}: posted at {Place(j.post)}. Target {Who(j.purp)} ({occupationName}). Cr{payment}. Bribe: {(accepts ? "would accept" : "would refuse")}.");
            }
            if (shown == 0)
                Text.Notify("[Dev] Nenhum contrato ativo. Use F8 para postar um.", "[Dev] No active contract. Press F8 to post one.");
        }

        internal static void KillTarget(bool viaMurder)
        {
            if (!CityReady()) return;
            var job = ContractRegistry.CurrentAcceptedJob();
            if (job == null)
            {
                Plugin.Logger.LogWarning("[Dev] No accepted job to act on.");
                return;
            }
            var purp = job.purp;
            if (purp == null || purp.isDead)
            {
                Plugin.Logger.LogWarning($"[Dev] Target missing or already dead: {Job(job)}");
                return;
            }

            // Both go through WeaponKills: the game's own kill paths (enableKill, Murder without a Murder object)
            // throw when the player is the killer.
            Plugin.Logger.LogInfo($"[Dev] Killing {Who(purp)} via {(viaMurder ? "Human.Murder" : "damage + kill, as a lethal shot")} for {Job(job)}");
            var player = Player.Instance;
            if (viaMurder)
                Weapons.WeaponKills.MurderByPlayer(purp, null, 0f);
            else
            {
                purp.RecieveDamage(99999f, player, purp.transform.position, Vector3.forward, null, null,
                    forceRagdoll: true, enableKill: false);
                Weapons.WeaponKills.Kill(purp, null, "dev hotkey");
            }
            Plugin.Logger.LogInfo($"[Dev] After: isDead={purp.isDead} isStunned={purp.isStunned} health={purp.currentHealth} | {Job(job)}");
        }
    }
}
