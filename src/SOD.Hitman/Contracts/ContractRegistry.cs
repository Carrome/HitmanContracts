using System.Collections.Generic;

namespace SODHitman.Contracts
{
    // Finds the mod's side jobs among the game's. Every Hitman JobPreset name starts with "Hitman".
    internal static class ContractRegistry
    {
        internal const string PresetPrefix = "Hitman";

        internal static bool IsHitmanPreset(JobPreset preset) =>
            preset != null && preset.name != null && preset.name.StartsWith(PresetPrefix);

        internal static bool IsHitmanJob(SideJob job) => job != null && IsHitmanPreset(job.preset);

        // The tag after every Hitman mission's name (decided 20/10/2026), also in content/Contracts/*/missions.postings.csv.
        internal const string Tag = "[Hitman]";

        internal static string DisplayName(JobPreset preset) => (preset != null ? preset.name : null) switch
        {
            "HitmanPizza" => Text.L("Entrega de pizza", "Pizza delivery"),
            "HitmanCall" => Text.L("Resolvedor de problemas", "Problem solver"),
            "HitmanPayphone" => Text.L("Trabalho rápido", "Quick job"),
            "HitmanPassion" => Text.L("Coração partido", "Broken heart"),
            "HitmanBadge" => Text.L("Alto risco", "High risk"),
            "HitmanIntimidate" => Text.L("Recado", "Message"),
            var other => other ?? "?",
        } + " " + Tag;

        // Snapshot of every side job the game tracks (posted, accepted and ended), safe to iterate while jobs change.
        internal static List<SideJob> AllJobs()
        {
            var result = new List<SideJob>();
            var dict = SideJobController.Instance?.allJobsDictionary;
            if (dict == null) return result;
            foreach (var kv in dict)
                if (kv.Value != null) result.Add(kv.Value);
            return result;
        }

        internal static bool IsActive(SideJob job) => job.state == SideJob.JobState.posted && !job.failed;

        internal static List<SideJob> ActiveHitmanJobs()
        {
            var result = new List<SideJob>();
            foreach (var job in AllJobs())
                if (IsHitmanJob(job) && IsActive(job)) result.Add(job);
            return result;
        }

        internal const string KillObjectivePrefix = "HitmanKill";

        // A contract to kill (its preset's revenge objective is HitmanKill, or HitmanKill<Contract> for the contracts
        // that carry their own copy). The others, like intimidation, are non-lethal jobs the game completes through
        // its own revenge objectives (beat up, vandalism).
        internal static bool IsKillPreset(JobPreset preset)
        {
            if (!IsHitmanPreset(preset) || preset.revengeObjectives == null) return false;
            foreach (var o in Dev.Describe.Each(preset.revengeObjectives))
                if (o != null && o.name != null && o.name.StartsWith(KillObjectivePrefix)) return true;
            return false;
        }

        internal static bool IsKillContract(SideJob job) => job != null && IsKillPreset(job.preset);

        // The kill contract presets the game knows (one tracking entry per preset).
        internal static List<string> KillPresetNames()
        {
            var result = new List<string>();
            var sjc = SideJobController.Instance;
            if (sjc == null) return result;
            foreach (var t in Dev.Describe.Each(sjc.jobTracking))
                if (t != null && IsKillPreset(t.preset)) result.Add(t.preset.name);
            return result;
        }

        // The resolve questions in use. SideJob.AcceptJob (game code) copies the job's list into the new case
        // (thisCase.resolveQuestions) and clears the job's own, so after accepting only the case has them.
        internal static Il2CppSystem.Collections.Generic.List<Case.ResolveQuestion> Questions(SideJob job)
        {
            var onCase = job.thisCase != null ? job.thisCase.resolveQuestions : null;
            return onCase != null && onCase.Count > 0 ? onCase : job.resolveQuestions;
        }

        // The resolve question completed by the target's death (the job's only revenge-objective question).
        internal static Case.ResolveQuestion KillQuestion(SideJob job)
        {
            var questions = Questions(job);
            if (questions == null) return null;
            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                if (q != null && q.inputType == Case.InputType.revengeObjective) return q;
            }
            return null;
        }

        internal static bool UsesMeetingIntro(SideJob job)
        {
            var intro = job.chosenIntro != null ? job.chosenIntro.preset : null;
            return intro != null && intro.name != null && intro.name.Contains("MeetUp");
        }

        // The accepted job the dev tools act on: a Hitman job if there is one, else the newest accepted job.
        internal static SideJob CurrentAcceptedJob()
        {
            SideJob best = null;
            foreach (var job in AllJobs())
            {
                if (!job.accepted || !IsActive(job)) continue;
                if (best == null
                    || (IsHitmanJob(job) && !IsHitmanJob(best))
                    || (IsHitmanJob(job) == IsHitmanJob(best) && job.jobID > best.jobID))
                    best = job;
            }
            return best;
        }
    }
}
