using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SODHitman.Contracts;
using static SODHitman.Dev.Describe;
using Il2CppGeneric = Il2CppSystem.Collections.Generic;

namespace SODHitman.Dev
{
    // Writes what the game has loaded (presets, citizens, side jobs) as JSON, so the mod's data can be built on
    // the real vanilla names. Fields are copied into managed objects by hand; nothing IL2CPP reaches the serializer.
    internal static class PresetDumper
    {
        private static bool autoDumped;

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        };

        internal static string DumpDir => Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "dump");

        internal static void OnCityReady()
        {
            if (!HitmanConfig.DevAutoDump.Value || autoDumped) return;
            autoDumped = true;
            DumpAll("city loaded");
        }

        internal static void DumpAll(string reason)
        {
            Directory.CreateDirectory(DumpDir);
            Plugin.Logger.LogInfo($"[Dump] Writing to {DumpDir} ({reason})");
            Write("presets_jobs", DumpJobPresets);
            Write("presets_intros", DumpIntros);
            Write("presets_handins", DumpHandIns);
            Write("presets_revenge", DumpRevengeObjectives);
            Write("presets_motives", DumpMotives);
            Write("presets_dialogs", DumpJobDialogs);
            Write("presets_names", DumpPresetNames);
            Write("citizens", DumpCitizens);
            Write("jobs_active", DumpActiveJobs);
        }

        internal static void DumpJobs()
        {
            Directory.CreateDirectory(DumpDir);
            Write("jobs_active", DumpActiveJobs);
        }

        private static void Write(string file, Func<object> build)
        {
            try
            {
                var data = build();
                if (data == null)
                {
                    Plugin.Logger.LogWarning($"[Dump] {file}: nothing to write (not loaded yet?)");
                    return;
                }
                File.WriteAllText(Path.Combine(DumpDir, file + ".json"), JsonSerializer.Serialize(data, Options), new UTF8Encoding(false));
                Plugin.Logger.LogInfo($"[Dump] {file}.json written");
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"[Dump] {file} failed: {e}");
            }
        }

        private static IEnumerable<T> Presets<T>() where T : UnityEngine.Object
        {
            var all = AssetLoader.Instance?.allPresets;
            if (all == null) yield break;
            for (int i = 0; i < all.Count; i++)
            {
                var so = all[i];
                if (so == null) continue;
                var cast = so.TryCast<T>();
                if (cast != null) yield return cast;
            }
        }

        // ---- presets ----

        private static object DumpJobPresets()
        {
            var registered = new HashSet<string>(Names(Toolbox.Instance?.allSideJobs));
            return Presets<JobPreset>().Select(p => (object)new
            {
                p.name,
                p.presetName,
                registeredInToolbox = registered.Contains(p.name),
                p.disabled,
                p.caseName,
                p.subClass,
                jobPosting = N(p.jobPosting),
                physicalRewardLocation = p.physicalRewardLocation.ToString(),
                p.allowSyncDiskRewards,
                p.allowBlackMarketSyncDiskRewards,
                p.generateHidingLocation,
                p.activePerCitizen,
                p.maxJobs,
                p.immediatePostCountThreshold,
                difficultyTag = p.difficultyTag.ToString(),
                changePosterDialogCompliancy = p.changePosterDialogCompliancy.ToString(),
                changePerpDialogCompliancy = p.changePerpDialogCompliancy.ToString(),
                p.penaltyForPurpAndPosterSameBuilding,
                purpetratorMotives = Names(p.purpetratorMotives),
                startingScenarios = Each(p.startingScenarios).Select(s => new { s.name, s.dds, leads = Each(s.leads).Select(Lead).ToList() }).ToList(),
                compatibleIntros = Each(p.compatibleIntros).Select(c => new { preset = N(c.preset), c.frequency }).ToList(),
                compatibleHandIns = Each(p.compatibleHandIns).Select(c => new { preset = N(c.preset), c.frequency }).ToList(),
                p.leadPoolData,
                informationAcquisitionLeads = Each(p.informationAcquisitionLeads).Select(Lead).ToList(),
                createFactsOnInformationAcquisition = Each(p.createFactsOnInformationAcquisition).Select(f => new
                {
                    factPreset = N(f.factPreset),
                    from = f.from.ToString(),
                    to = f.to.ToString(),
                }).ToList(),
                revengeObjectives = Names(p.revengeObjectives),
                spawnItems = Each(p.spawnItems).Select(SpawnItem).ToList(),
                resolveQuestions = Each(p.resolveQuestions).Select(Question).ToList(),
                additional = Each(p.additional).Select(Block).ToList(),
                dialogReferences = Each(p.dialogReferences).Select(d => new { d.name, dialog = N(d.dialog) }).ToList(),
            }).ToList();
        }

        private static object DumpIntros() =>
            Presets<SideMissionIntroPreset>().Select(p => (object)new
            {
                p.name,
                p.presetName,
                p.rewardModifier,
                blocks = Each(p.blocks).Select(Block).ToList(),
            }).ToList();

        private static object DumpHandIns() =>
            Presets<SideMissionHandInPreset>().Select(p => (object)new
            {
                p.name,
                p.presetName,
                p.rewardModifier,
                p.postersDoor,
                p.cityHall,
                blocks = Each(p.blocks).Select(Block).ToList(),
            }).ToList();

        private static object DumpRevengeObjectives() =>
            Presets<RevengeObjective>().Select(p => (object)new
            {
                p.name,
                p.presetName,
                p.disabled,
                p.baseChance,
                p.answerMethod,
                tag = p.tag.ToString(),
                icon = p.icon.ToString(),
                p.d0Name,
                p.d1Name,
                p.idTargetName,
                p.resolveQuestionName,
                p.resolveQuestionNameAlternate,
                passedNumberRange = V2(p.passedNumberRange),
                rewardMultiplier = V2(p.rewardMultiplier),
                specialConditions = Strings(p.specialConditions),
                p.useHEXACO,
                p.useTraits,
            }).ToList();

        private static object DumpMotives() =>
            Presets<MotivePreset>().Select(p => (object)new
            {
                p.name,
                p.presetName,
                p.allowHomelessPurps,
                p.allowJoblessPurps,
                p.purpMustLiveAtDifferentAddressToPoster,
                p.allowEnforcers,
                p.disallowEchelonHome,
                p.usePurpJobs,
                purpJobs = Names(p.purpJobs),
                purpTraitModifiers = Each(p.purpTraitModifiers).Select(Rule).ToList(),
                p.allowHomelessPosters,
                p.allowJoblessPosters,
                p.usePosterConnections,
                acceptableConnections = Strings(p.acceptableConnections),
                p.usePosterTraits,
                posterTraitModifiers = Each(p.posterTraitModifiers).Select(Rule).ToList(),
                p.purpIsExemptFromPostingOtherJobs,
                p.purpIsExemptFromPurpingOtherJobs,
                p.posterIsExemptFromPostingOtherJobs,
                p.posterIsExemptFromPurpingOtherJobs,
            }).ToList();

        // Side job dialogs (vanilla "Job_*" and the mod's "Hitman*"), to see which responses drive each intro step.
        private static object DumpJobDialogs() =>
            Presets<DialogPreset>()
                .Where(p => p.name != null && (p.name.StartsWith("Job_") || p.name.StartsWith(ContractRegistry.PresetPrefix)))
                .Select(p => (object)new
                {
                    p.name,
                    p.msgID,
                    p.isJobDetails,
                    p.telephoneCallOption,
                    p.defaultOption,
                    specialCase = p.specialCase.ToString(),
                    p.ignoreActiveJobRequirement,
                    p.removeAfterSaying,
                    p.useSuccessTest,
                    p.baseChance,
                    responses = Each(p.responses).Select(r => new
                    {
                        r.dictionaryString,
                        r.ddsMessageID,
                        r.isSuccessful,
                        r.chance,
                        r.useParsing,
                        r.endsDialog,
                        r.jobHandIn,
                        r.startCombat,
                        r.flee,
                        r.giveUpSelf,
                        r.onlyIfEnfocerOnDuty,
                        r.onlyIfNotEnforcerOnDuty,
                        applyDiscovery = Strings(r.applyDiscovery),
                        tieKeys = Strings(r.tieKeys),
                    }).ToList(),
                    followUpDialogSuccess = Names(p.followUpDialogSuccess),
                    followUpDialogFail = Names(p.followUpDialogFail),
                }).ToList();

        // Every loaded ScriptableObject name, grouped by type: the vocabulary for "REF:Type|Name" in the mod's JSON.
        private static object DumpPresetNames()
        {
            var all = AssetLoader.Instance?.allPresets;
            if (all == null) return null;
            var byType = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
            for (int i = 0; i < all.Count; i++)
            {
                var so = all[i];
                if (so == null) continue;
                var type = so.GetIl2CppType().Name;
                if (!byType.TryGetValue(type, out var list)) byType[type] = list = new List<string>();
                list.Add(so.name);
            }
            foreach (var list in byType.Values) list.Sort(StringComparer.Ordinal);
            return new
            {
                counts = byType.ToDictionary(kv => kv.Key, kv => kv.Value.Count),
                occupations = Names(Toolbox.Instance?.allJobs),
                criminalOccupations = Names(Toolbox.Instance?.allCriminalJobs),
                names = byType,
            };
        }

        // ---- city ----

        private sealed class OccupationStats
        {
            public int count { get; set; }
            public int enforcers { get; set; }
            public float minSalary { get; set; }
            public float maxSalary { get; set; }
            public float minPaygrade { get; set; }
            public float maxPaygrade { get; set; }
            public string exampleTitle { get; set; }
            public string exampleSalaryString { get; set; }
        }

        private static object DumpCitizens()
        {
            var citizens = CityData.Instance?.citizenDirectory;
            if (citizens == null || citizens.Count == 0) return null;

            var byOccupation = new SortedDictionary<string, OccupationStats>(StringComparer.Ordinal);
            var sample = new List<object>();
            int employed = 0;
            foreach (var c in Each(citizens))
            {
                if (c == null) continue;
                var job = c.job;
                bool hasEmployer = job != null && job.employer != null;
                if (hasEmployer) employed++;

                var key = job != null && job.preset != null ? job.preset.name : "(none)";
                if (!byOccupation.TryGetValue(key, out var s))
                {
                    byOccupation[key] = s = new OccupationStats
                    {
                        exampleTitle = job?.name,
                        exampleSalaryString = job?.salaryString,
                        minSalary = job?.salary ?? 0, maxSalary = job?.salary ?? 0,
                        minPaygrade = job?.paygrade ?? 0, maxPaygrade = job?.paygrade ?? 0,
                    };
                }
                s.count++;
                if (c.isEnforcer) s.enforcers++;
                if (job != null)
                {
                    s.minSalary = Math.Min(s.minSalary, job.salary);
                    s.maxSalary = Math.Max(s.maxSalary, job.salary);
                    s.minPaygrade = Math.Min(s.minPaygrade, job.paygrade);
                    s.maxPaygrade = Math.Max(s.maxPaygrade, job.paygrade);
                }

                if (sample.Count < 40)
                {
                    sample.Add(new
                    {
                        who = Who(c),
                        occupation = key,
                        title = job?.name,
                        salary = job?.salary,
                        salaryString = job?.salaryString,
                        paygrade = job?.paygrade,
                        hasEmployer,
                        c.isEnforcer,
                        hasPartner = c.partner != null,
                        hasHome = c.home != null,
                    });
                }
            }
            return new { total = citizens.Count, employed, occupations = byOccupation, sample };
        }

        private static object DumpActiveJobs()
        {
            if (SideJobController.Instance == null) return null;
            return new
            {
                gameTime = SessionData.Instance != null ? SessionData.Instance.gameTime : 0f,
                decimalClock = SessionData.Instance != null ? SessionData.Instance.decimalClock : 0f,
                contracts = ContractStore.All.ToList(),
                jobs = ContractRegistry.AllJobs().Select(JobDetail).ToList(),
            };
        }

        internal static object JobDetail(SideJob j)
        {
            var purpJob = j.purp != null ? j.purp.job : null;
            return new
            {
                j.jobID,
                preset = N(j.preset),
                type = j.GetIl2CppType().Name,
                state = j.state.ToString(),
                j.accepted,
                j.failed,
                j.phase,
                j.phaseChange,
                j.reward,
                j.rewardSyncDisk,
                poster = Who(j.poster),
                purp = Who(j.purp),
                purpDead = j.purp != null && j.purp.isDead,
                purpOccupation = purpJob != null ? N(purpJob.preset) : null,
                purpSalary = purpJob != null ? purpJob.salary : (float?)null,
                motive = N(j.motive),
                intro = j.chosenIntro != null ? N(j.chosenIntro.preset) : null,
                handIn = j.chosenHandIn != null ? N(j.chosenHandIn.preset) : null,
                introString = j.intro,
                handInString = j.handIn,
                currentBlock = j.currentBlock != null ? $"{j.currentBlock.name} ({j.currentBlock.elementType})" : null,
                post = Where(j.post),
                j.startingScenario,
                j.knowHandInLocation,
                j.gooseChasePhone,
                j.gooseChaseFromPhone,
                j.gooseChaseCallTime,
                j.gooseChaseCallTriggered,
                chosenGooseChasePhone = Where(j.chosenGooseChasePhone),
                j.meetingPoint,
                j.meetingConsumableIndex,
                chosenMeetingPoint = Where(j.chosenMeetingPoint),
                j.secretLocationFurniture,
                j.jobInfoDialogMsg,
                j.fakeNumberStr,
                activeJobItems = Items(j.activeJobItems),
                objectives = j.thisCase != null
                    ? Each(j.thisCase.currentActiveObjectives).Select(o => new { o.name, o.progress, o.isComplete, o.isCancelled }).ToList()
                    : null,
                resolveQuestions = Each(Contracts.ContractRegistry.Questions(j)).Select(Question).ToList(),
                caseStatus = j.thisCase != null ? j.thisCase.caseStatus.ToString() : null,
                handInValid = j.thisCase != null && j.thisCase.handInValid,
            };
        }

        // ---- pieces ----

        private static Dictionary<string, string> Items(Il2CppGeneric.Dictionary<JobPreset.JobTag, Interactable> items)
        {
            var result = new Dictionary<string, string>();
            if (items == null) return result;
            foreach (var kv in items) result[kv.Key.ToString()] = Where(kv.Value);
            return result;
        }

        private static object Lead(JobPreset.StartingLead l) => new
        {
            leadEvidence = l.leadEvidence.ToString(),
            keys = Strings(l.keys),
            l.useKeyFromLeadPool,
            l.autoPin,
            addDialogOptions = Names(l.addDialogOptions),
            factsReveal = Strings(l.factsReveal),
            mergeKeys = Strings(l.mergeKeys),
        };

        private static object SpawnItem(JobPreset.StartingSpawnItem s) => new
        {
            s.name,
            spawnItem = N(s.spawnItem),
            itemTag = s.itemTag.ToString(),
            where = s.where.ToString(),
            belongsTo = s.belongsTo.ToString(),
            writer = s.writer.ToString(),
            receiver = s.receiver.ToString(),
            s.chance,
            s.findExisting,
            s.compatibleWithAllMotives,
            compatibleWithMotives = Names(s.compatibleWithMotives),
            s.useIf,
            ifTag = s.ifTag.ToString(),
            s.vmailThread,
            s.security,
            s.priority,
            ownershipRule = s.ownershipRule.ToString(),
        };

        private static object Block(SideMissionIntroPreset.SideMissionObjectiveBlock b) => new
        {
            b.name,
            elementType = b.elementType.ToString(),
            b.dialogReference,
            tagReference = b.tagReference.ToString(),
            b.objectiveDelay,
            b.enableUpdateWhileTalking,
            spawnItems = Each(b.spawnItems).Select(SpawnItem).ToList(),
            validItems = Names(b.validItems),
            validFurniture = Names(b.validFurniture),
            disableOnDifficulties = Strings(b.disableOnDifficulties),
            onlyCompatibleWithIntros = Names(b.onlyCompativleWithIntros),
            onlyCompatibleWithHandIns = Names(b.onlyCompatibleWithHandIns),
            triggerFailIfItemDestroyed = Strings(b.triggerFailIfItemDestroyed),
        };

        private static object Rule(MotivePreset.ModifierRule r) => new
        {
            rule = r.rule.ToString(),
            traits = Names(r.traitList),
            r.mustPassForApplication,
            r.score,
        };

        // Preset fields plus the live state, which only means something on a running job.
        internal static object Question(Case.ResolveQuestion q) => q == null ? null : new
        {
            q.name,
            inputType = q.inputType.ToString(),
            automaticAnswers = Strings(q.automaticAnswers),
            tag = q.tag.ToString(),
            target = q.target.ToString(),
            location = q.location.ToString(),
            q.revengeObjective,
            rewardRange = V2(q.rewardRange),
            penaltyRange = V2(q.penaltyRange),
            q.isOptional,
            q.displayObjective,
            q.displayOnlyAtPhase,
            q.displayAtPhase,
            q.useAlternateName,
            useName = q.useName.ToString(),
            onlyCompatibleWithIntros = Names(q.onlyCompatibleWithIntros),
            onlyCompatibleWithHandIns = Names(q.onlyCompatibleWithHandIns),
            q.input,
            q.progress,
            q.reward,
            q.penalty,
            q.isValid,
            q.isCorrect,
            q.completedRevenge,
            q.revengeObjPassed,
            q.revengeObjTarget,
            q.revengeObjLoc,
            correctAnswers = Strings(q.correctAnswers),
        };
    }
}
