using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using HarmonyLib;
using SODHitman.Contracts;
using static SODHitman.Dev.Describe;

namespace SODHitman.Heat
{
    internal sealed class HeatState
    {
        public int Points { get; set; }
        public int Level { get; set; }
        public int Kills { get; set; }
        public int ContractKills { get; set; }
        // Victims already scored: weapon mods may call Human.Murder again for them when a save loads.
        public List<int> CountedVictims { get; set; } = new();
        public int ArticlePending { get; set; }
        public int ArticleLevelPublished { get; set; }
        public float NextInvestigation { get; set; } = -1f;
        public float NextHunter { get; set; } = -1f;
        public List<int> HunterIds { get; set; } = new();

        // Corrupt cop (spec 11.1)
        public int CopId { get; set; } = -1;
        public string CopName { get; set; }
        public int CopNumber { get; set; } = -1;
        public bool CopContacted { get; set; }
        public bool CopAnnounceReplacement { get; set; }
        public string PreviousCopName { get; set; }
        public float CopNextCall { get; set; } = -1f;
        public float CopReplaceAt { get; set; } = -1f;
        public int PendingPoints { get; set; }
        // 1.2.2: the package paid for by the envelope (CorruptCop.Package); -1 = a save from before, PendingPoints applies.
        public int PendingKind { get; set; } = -1;
        public int PendingCost { get; set; }
        public int PendingDropNumber { get; set; } = -1;
        public float PendingDeadline { get; set; } = -1f;

        // What the police could know about the last kill, for the interrogation questions.
        public float LastKillTime { get; set; } = -1f;
        public bool LastKillSeen { get; set; }
        public string LastVictimName { get; set; }
        public bool LastVictimNeighbour { get; set; }

        // v0.7: what the police can find out about each kill (bodies, witnesses, cameras), newest last.
        public List<KillRecord> KillRecords { get; set; } = new();
        // Career (v0.9): points from contract ratings and the killer's record.
        public int Reputation { get; set; }
        public int ContractsDone { get; set; }
        public int SilentRatings { get; set; }
        public int Earned { get; set; }
        // The Fixer (v0.9).
        public int FixerId { get; set; } = -1;
        public string FixerName { get; set; }
        public int FixerNumber { get; set; } = -1;
        public bool FixerContacted { get; set; }
        public float FixerNextCall { get; set; } = -1f;
        public float FixerNextJob { get; set; } = -1f;
        public float FixerReplaceAt { get; set; } = -1f;
        // Killers sent by an angry client (bribe discovered); they chase whatever the heat level.
        public List<int> PersonalHunterIds { get; set; } = new();
        // Bribed targets who left town (kept removed after a load).
        public List<int> LeftTownIds { get; set; } = new();
        // v0.8: poisoned citizens and when they die.
        public List<Weapons.PoisonRecord> Poisoned { get; set; } = new();
        // v1.1 consequences: killers on their way, clients who owe the player, targets laying low.
        public List<Contracts.PendingHunter> PendingHunters { get; set; } = new();
        public List<Contracts.Grudge> Grudges { get; set; } = new();
        public List<Contracts.ScaredTarget> Scared { get; set; } = new();
        // v1.2 campaign "O Último Contrato" (Contracts/Campaign.cs).
        public int CampaignStage { get; set; }
        public int CampaignJobId { get; set; } = -1;
        public float CampaignCallAt { get; set; } = -1f;
        public bool CampaignBetrayed { get; set; }
        public string CampaignEnding { get; set; }

        // Interrogation visit waiting for the player to be at home (game time it became due, -1 = none).
        public float VisitDue { get; set; } = -1f;
        public bool VisitForced { get; set; }

        // A visit in progress is not saved: after a load the visit simply happens again.
        [JsonIgnore] public float VisitCallTime { get; set; } = -1f;
        [JsonIgnore] public int InterrogatorId { get; set; } = -1;
        [JsonIgnore] public int InterrogationStep { get; set; }
        [JsonIgnore] public int InterrogationScore { get; set; }
        [JsonIgnore] public float InterrogationStart { get; set; } = -1f;
        [JsonIgnore] public bool InterrogationHasQ3 { get; set; }
    }

    // Spec 11: every death the player causes adds heat; it never decays (the corrupt cop is the only way down).
    //   level 1 (Level1 pts): newspaper article
    //   level 2 (Level2 pts): enforcers sent to the player's home now and then (+ the corrupt cop calls, phase 4)
    //   level 3 (Level3 pts): wanted in every building + hired killers come after the player
    internal static class HeatSystem
    {
        internal static readonly string[] ArticleMessages =
        {
            null,
            "ae9ea068-b428-4ee9-b8c4-6cb486ea191e",
            "f00b409e-ee70-44cd-b5e5-836558cf1d1f",
            "e209af83-b30c-4ad2-903b-391a3a8df544",
        };

        private static readonly HashSet<string> Logged = new();

        private static HeatState H => ContractStore.Heat;

        internal static int LevelFor(int points) =>
            points >= HitmanConfig.HeatLevel3.Value ? 3 :
            points >= HitmanConfig.HeatLevel2.Value ? 2 :
            points >= HitmanConfig.HeatLevel1.Value ? 1 : 0;

        internal static void Change(int delta, string reason)
        {
            if (!HitmanConfig.HeatEnabled.Value || delta == 0) return;
            int oldLevel = H.Level;
            H.Points = Math.Max(0, H.Points + delta);
            H.Level = LevelFor(H.Points);
            Plugin.Logger.LogInfo($"[Heat] {(delta > 0 ? "+" : "")}{delta} ({reason}) -> {H.Points} pts, level {H.Level}");
            if (H.Level != oldLevel) OnLevelChanged(oldLevel, H.Level);
        }

        internal static bool Counted(Human victim) =>
            victim != null && H.CountedVictims != null && H.CountedVictims.Contains(victim.humanID);

        internal static void OnPlayerKill(Human victim)
        {
            if (victim == null) return;
            // v1.1: a client who kept the player's money is a contract of its own (the debt).
            bool debt = Consequences.IsGrudgeTarget(victim);
            Consequences.OnPlayerKill(victim);
            if (!HitmanConfig.HeatEnabled.Value) return;
            H.CountedVictims ??= new List<int>();
            if (H.CountedVictims.Contains(victim.humanID))
            {
                Plugin.Logger.LogInfo($"[Heat] {Who(victim)} already counted, ignoring repeated Human.Murder.");
                return;
            }
            H.CountedVictims.Add(victim.humanID);
            var player = Player.Instance;
            var loc = player != null ? player.currentGameLocation : null;

            bool contract = debt || ContractRegistry.ActiveHitmanJobs().Any(j => j.purp == victim && ContractRegistry.IsKillContract(j));
            string method = Weapons.Arsenal.CurrentMethod;
            bool natural = method == "poison";
            // A poisoned victim dies hours later, wherever they are: nobody sees the player do it then.
            if (natural) loc = victim.currentGameLocation;
            var witnesses = natural ? new List<Human>() : Witnesses(player, victim);
            bool camera = !natural && KillEvidence.CameraSawPlayer(player, loc);
            bool isPublic = !natural && loc != null && (loc.isOutside || IsPublic(loc));

            H.Kills++;
            if (contract) H.ContractKills++;
            H.LastKillTime = Text.Now;
            H.LastKillSeen = camera;
            H.LastVictimName = victim.GetCitizenName();
            H.LastVictimNeighbour = victim.home != null && player != null && player.home != null && victim.home.building == player.home.building;
            KillEvidence.OnKill(victim, contract, witnesses, camera, isPublic, loc, method);
        }

        // The game's own witness set, plus citizens close by who can see the player right now
        // (the set may not have caught up yet when a shot kills instantly). At most MaxWitnesses, nearest first.
        internal static List<Human> Witnesses(Player player, Human victim)
        {
            var result = new List<Human>();
            if (player == null) return result;
            var ids = new HashSet<int>();
            var position = player.transform.position;
            void Take(Human h)
            {
                if (h == null || h.isDead || h.isPlayer || h == victim || !ids.Add(h.humanID)) return;
                result.Add(h);
            }

            var set = player.witnessesToIllegalActivity;
            if (set != null)
                foreach (var actor in set)
                    Take(actor != null ? actor.TryCast<Human>() : null);

            // v0.8: a loud gunshot is heard: everyone close enough is a witness, seeing or not.
            if (Weapons.Arsenal.RecentLoudShot)
            {
                float radius = HitmanConfig.ArsenalHearingRadius.Value;
                var shot = Weapons.Arsenal.LastLoudShotPosition;
                foreach (var c in Each(CityData.Instance != null ? CityData.Instance.citizenDirectory : null))
                    if (c != null && !c.isDead && c != victim && (c.ai == null || !c.ai.ko) && (c.transform.position - shot).sqrMagnitude <= radius * radius)
                        Take(c);
            }

            var citizens = CityData.Instance != null ? CityData.Instance.citizenDirectory : null;
            foreach (var c in Each(citizens))
            {
                if (c == null || c.isDead || c == victim || ids.Contains(c.humanID)) continue;
                if ((c.transform.position - position).sqrMagnitude > 25f * 25f) continue;
                try
                {
                    if (SOD.Common.Extensions.GameExtensions.Sees(c, player)) Take(c);
                }
                catch { }
            }
            return result
                .OrderBy(h => (h.transform.position - position).sqrMagnitude)
                .Take(HitmanConfig.HeatMaxWitnesses.Value)
                .ToList();
        }

        private static bool IsPublic(NewGameLocation loc)
        {
            try { return loc.IsPublicallyOpen(true); }
            catch { return false; }
        }

        private static void OnLevelChanged(int from, int to)
        {
            float now = Text.Now;
            if (to > from)
            {
                if (HitmanConfig.HeatNewspaper.Value && to > H.ArticleLevelPublished) H.ArticlePending = to;
                if (to >= 2 && H.NextInvestigation < 0f) H.NextInvestigation = now + 1f;
                if (to >= 3 && H.NextHunter < 0f) H.NextHunter = now + 2f;
                switch (to)
                {
                    case 1:
                        Text.Notify("Os jornais estão falando de um matador de aluguel na cidade. (Heat nível 1)",
                                    "The papers are talking about a contract killer in town. (Heat level 1)");
                        break;
                    case 2:
                        Text.Notify("Os Enforcers estão investigando as mortes. Cuidado com o seu apartamento. (Heat nível 2)",
                                    "Enforcers are investigating the deaths. Watch your apartment. (Heat level 2)");
                        break;
                    case 3:
                        Text.Notify("Há um mandado contra você, e alguém pôs um preço na sua cabeça. (Heat nível 3)",
                                    "There is a warrant out for you, and someone put a price on your head. (Heat level 3)");
                        break;
                }
                CorruptCopHooks.OnHeatLevelUp(to);
            }
            else
            {
                Text.Notify($"Heat reduzido para o nível {to}.", $"Heat down to level {to}.");
                if (from >= 3 && to < 3)
                {
                    CancelHunters();
                    H.NextHunter = -1f;
                }
                if (to < 2)
                {
                    H.NextInvestigation = -1f;
                    if (!H.VisitForced) H.VisitDue = -1f;
                }
            }
        }

        internal static void Tick()
        {
            if (!HitmanConfig.HeatEnabled.Value) return;
            var player = Player.Instance;
            if (player == null) return;
            float now = Text.Now;
            KillEvidence.Tick(now);
            Interrogation.Tick(player, now);
            TickPersonalHunters(player);
            if (H.Level == 0) return;

            if (H.Level >= 2 && HitmanConfig.HeatInvestigations.Value && H.NextInvestigation >= 0f && now >= H.NextInvestigation)
            {
                H.NextInvestigation = now + (H.Level >= 3 ? HitmanConfig.HeatInvestigationHoursL3.Value : HitmanConfig.HeatInvestigationHoursL2.Value);
                var home = player.home;
                // Level 2: an interrogation at home (decided 19/10/2026). Level 3 (warrant): the game's own response.
                if (H.Level == 2 && HitmanConfig.InterrogationEnabled.Value) Interrogation.Schedule(now);
                else if (home != null && GameplayController.Instance != null)
                {
                    GameplayController.Instance.CallEnforcers(home, false, false, 0f);
                    Plugin.Logger.LogInfo($"[Heat] Enforcers sent to the player's home {N(home)}; next visit {Text.Clock(H.NextInvestigation)}");
                    Text.Notify("Vizinhos viram Enforcers rondando o seu apartamento.",
                                "Neighbours saw Enforcers poking around your apartment.");
                }
                else Plugin.Logger.LogInfo("[Heat] Investigation skipped: the player has no home.");
            }

            if (H.Level >= 3 && HitmanConfig.HeatWarrant.Value)
            {
                var building = player.currentBuilding;
                if (building != null && StatusController.Instance != null)
                {
                    StatusController.Instance.SetWantedInBuilding(building, HitmanConfig.HeatWantedValue.Value);
                    if (Logged.Add("wanted:" + N(building)))
                        Plugin.Logger.LogInfo($"[Heat] Wanted in {N(building)}: wantedInBuilding={building.wantedInBuilding} at gameTime {now:0.00}");
                }
            }

            if (H.Level >= 3 && HitmanConfig.HeatHunters.Value) TickHunters(player, now);
        }

        // ---- hunters ----

        private static void TickHunters(Player player, float now)
        {
            H.HunterIds.RemoveAll(id => !TryHuman(id, out var h) || h.isDead);

            if (H.NextHunter >= 0f && now >= H.NextHunter)
            {
                H.NextHunter = now + HitmanConfig.HeatHunterHours.Value;
                if (H.HunterIds.Count < HitmanConfig.HeatMaxHunters.Value)
                {
                    var hunter = PickHunter();
                    if (hunter != null)
                    {
                        H.HunterIds.Add(hunter.humanID);
                        Plugin.Logger.LogInfo($"[Heat] Hunter {Who(hunter)} ({N(hunter.job?.preset)}) sent after the player from {N(hunter.currentGameLocation)}");
                        Text.Notify("Você sente que está sendo seguido.", "You feel like you're being followed.");
                    }
                    else Plugin.Logger.LogInfo("[Heat] No hunter available.");
                }
            }

            // Keep each hunter on the player's trail.
            foreach (var id in H.HunterIds)
            {
                if (!TryHuman(id, out var hunter) || hunter.ai == null) continue;
                if (!hunter.ai.persuit) hunter.ai.SetPersue(player, false, 2, true, 1000f);
            }
        }

        private static Human PickHunter()
        {
            var citizens = CityData.Instance != null ? CityData.Instance.citizenDirectory : null;
            if (citizens == null) return null;
            var targets = ContractRegistry.ActiveHitmanJobs().Select(j => j.purp).Where(p => p != null).ToList();

            var killers = new List<Human>();
            var fighters = new List<Human>();
            foreach (var c in Each(citizens))
            {
                if (c == null || c.isDead || c.isPlayer || c.isEnforcer || c.ai == null) continue;
                if (H.HunterIds.Contains(c.humanID) || (H.PersonalHunterIds != null && H.PersonalHunterIds.Contains(c.humanID)) || targets.Any(t => t == c)) continue;
                var occupation = c.job != null && c.job.preset != null ? c.job.preset.name : null;
                if (occupation == "HiredKiller") killers.Add(c);
                else if (Payment.IsFighter(occupation) && occupation != "Enforcer" && occupation != "EnforcerCaptain") fighters.Add(c);
            }
            var pool = killers.Count > 0 ? killers : fighters;
            return pool.Count == 0 ? null : pool[new Random().Next(pool.Count)];
        }

        internal static void DevCancelHunters()
        {
            CancelHunters();
            CancelPersonalHunters();
        }

        // Killers paid by a client or the Fixer give up (the campaign's ending).
        internal static void CancelPersonalHunters()
        {
            foreach (var id in H.PersonalHunterIds ?? new List<int>())
                if (TryHuman(id, out var hunter) && hunter.ai != null) hunter.ai.CancelPersue();
            H.PersonalHunterIds?.Clear();
        }

        // A killer hired against the player by a client (v0.7), independent of the heat level.
        internal static void SendPersonalHunter(string reason)
        {
            H.PersonalHunterIds ??= new List<int>();
            var hunter = PickHunter();
            if (hunter == null)
            {
                Plugin.Logger.LogInfo($"[Heat] No hunter available for: {reason}");
                return;
            }
            H.PersonalHunterIds.Add(hunter.humanID);
            Plugin.Logger.LogInfo($"[Heat] Personal hunter {Who(hunter)} sent: {reason}");
            Text.Notify("Você sente que está sendo seguido.", "You feel like you're being followed.");
        }

        private static void TickPersonalHunters(Player player)
        {
            var ids = H.PersonalHunterIds;
            if (ids == null || ids.Count == 0) return;
            ids.RemoveAll(id => !TryHuman(id, out var h) || h.isDead);
            foreach (var id in ids)
            {
                if (!TryHuman(id, out var hunter) || hunter.ai == null) continue;
                if (!hunter.ai.persuit) hunter.ai.SetPersue(player, false, 2, true, 1000f);
            }
        }

        private static void CancelHunters()
        {
            foreach (var id in H.HunterIds)
                if (TryHuman(id, out var hunter) && hunter.ai != null) hunter.ai.CancelPersue();
            if (H.HunterIds.Count > 0) Plugin.Logger.LogInfo($"[Heat] Called off {H.HunterIds.Count} hunter(s).");
            H.HunterIds.Clear();
        }

        private static bool TryHuman(int id, out Human human)
        {
            human = null;
            return CityData.Instance != null && CityData.Instance.GetHuman(id, out human, false) && human != null;
        }

        // ---- game hooks ----

        // Not while loading: the game re-runs Human.Murder for every saved corpse, the player's victims included.
        [HarmonyPatch(typeof(Human), nameof(Human.Murder))]
        internal static class Human_Murder
        {
            static void Postfix(Human __instance, Human killer)
            {
                if (ContractStore.Loading) return;
                if (__instance != null && __instance.isDead && killer != null && killer.isPlayer) OnPlayerKill(__instance);
            }
        }

        // A new level puts its article on the front page of the next newspaper.
        [HarmonyPatch(typeof(NewspaperController), nameof(NewspaperController.GenerateNewNewspaper))]
        internal static class NewspaperController_GenerateNewNewspaper
        {
            static void Postfix(NewspaperController __instance)
            {
                int level = H.ArticlePending;
                if (level <= 0 || level >= ArticleMessages.Length || !HitmanConfig.HeatNewspaper.Value) return;
                var state = __instance.currentState;
                if (state == null) return;

                var msg = ArticleMessages[level];
                bool known = Toolbox.Instance != null && Toolbox.Instance.allDDSMessages != null && Toolbox.Instance.allDDSMessages.ContainsKey(msg);
                Plugin.Logger.LogInfo($"[Heat] Newspaper: main article {state.mainArticle} (context {state.mainContext}) -> level {level} article {msg} (loaded={known})");
                if (!known) return;

                state.mainArticle = msg;
                H.ArticleLevelPublished = level;
                H.ArticlePending = 0;
                try { __instance.UpdateText(false); }
                catch (Exception e) { Plugin.Logger.LogWarning($"[Heat] Newspaper UpdateText failed: {e.Message}"); }
            }
        }
    }

    internal static class CorruptCopHooks
    {
        internal static void OnHeatLevelUp(int level) => CorruptCop.OnHeatLevelUp(level);
    }
}
