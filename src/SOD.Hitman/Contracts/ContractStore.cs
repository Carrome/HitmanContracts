using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SOD.Common;
using SOD.Common.Helpers;
using SODHitman.Heat;

namespace SODHitman.Contracts
{
    // What the mod adds on top of the game's own (saved) SideJob, one entry per Hitman job.
    internal sealed class ContractState
    {
        public int JobId { get; set; }
        public int Payment { get; set; } = -1;
        public string PaymentBreakdown { get; set; }
        public float MeetTime { get; set; } = -1f;
        public float Deadline { get; set; } = -1f;
        public bool InfoAcquired { get; set; }
        public bool KillRecorded { get; set; }
        public bool FailedLate { get; set; }
        public bool BribeAttempted { get; set; }
        public string BribeOutcome { get; set; }
        public int DepositPaid { get; set; }
        // v0.7
        public int KillsAtAccept { get; set; } = -1;
        public string Rating { get; set; }
        public bool BribedAway { get; set; }
        public float BribeDiscoverAt { get; set; } = -1f;
        public int TargetId { get; set; } = -1;
        public string TargetName { get; set; }
        // v1.1: how the client betrayed the player at the hand-in (nopay, police, hunter), "none" when they didn't.
        public string Betrayal { get; set; }
    }

    internal sealed class ModSaveData
    {
        public List<ContractState> Contracts { get; set; } = new();
        public HeatState Heat { get; set; } = new();
        public List<int> WeaponKills { get; set; } = new();
    }

    // The mod's per-save data, kept in a JSON file next to the save through SOD.Common's save events.
    internal static class ContractStore
    {
        private static readonly Dictionary<int, ContractState> States = new();

        // One file per save. Uses the save-store API that exists in both SOD.Common 2.0.x (the one that matches
        // this game build) and 2.1.x, unlike GetSaveGameDataPath (2.1.x only).
        private static string PathFor(SaveGameArgs args) =>
            Lib.SaveGame.GetSavestoreDirectoryPath(typeof(Plugin).Assembly, $"hitman_{Lib.SaveGame.GetUniqueString(args.FilePath)}.json");

        internal static HeatState Heat { get; private set; } = new();

        // From the game's "before load" until the mod has read its own file and re-applied the kills. In between,
        // the game re-runs Human.Murder for every saved corpse; those are not new kills.
        internal static bool Loading { get; set; }

        // Citizens the player shot dead, re-applied on load (see Weapons.WeaponKills).
        internal static List<int> WeaponKills { get; private set; } = new();

        internal static ContractState Get(SideJob job)
        {
            if (!States.TryGetValue(job.jobID, out var state))
                States[job.jobID] = state = new ContractState { JobId = job.jobID };
            return state;
        }

        internal static bool TryGet(SideJob job, out ContractState state) => States.TryGetValue(job.jobID, out state);

        internal static IReadOnlyCollection<ContractState> All => States.Values;

        internal static void Reset()
        {
            States.Clear();
            Heat = new HeatState();
            WeaponKills = new List<int>();
            CorruptCop.ResetSession();
            Postings.ResetSession();
            Fixer.ResetSession();
            Consequences.ResetSession();
            StoryCall.ResetSession();
        }

        internal static void Save(SaveGameArgs args)
        {
            try
            {
                var path = PathFor(args);
                var data = new ModSaveData { Contracts = States.Values.ToList(), Heat = Heat, WeaponKills = WeaponKills };
                File.WriteAllText(path, JsonSerializer.Serialize(data));
                Plugin.Logger.LogInfo($"[Store] Saved {States.Count} contract(s), heat {Heat.Points} to {path}");
            }
            catch (Exception e) { Plugin.Logger.LogError($"[Store] Save failed: {e}"); }
        }

        internal static void Load(SaveGameArgs args)
        {
            Reset();
            try
            {
                var path = PathFor(args);
                if (!File.Exists(path))
                {
                    Plugin.Logger.LogInfo("[Store] No mod data for this save.");
                    return;
                }
                var data = JsonSerializer.Deserialize<ModSaveData>(File.ReadAllText(path)) ?? new ModSaveData();
                foreach (var s in data.Contracts ?? new List<ContractState>())
                    States[s.JobId] = s;
                Heat = data.Heat ?? new HeatState();
                WeaponKills = data.WeaponKills ?? new List<int>();
                Plugin.Logger.LogInfo($"[Store] Loaded {States.Count} contract(s), heat {Heat.Points} (level {Heat.Level}) from {path}");
            }
            catch (Exception e) { Plugin.Logger.LogError($"[Store] Load failed: {e}"); }
        }
    }
}
