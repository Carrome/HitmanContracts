using System;
using SOD.Common;
using SOD.Common.Helpers.DialogObjects;

namespace SODHitman.Contracts
{
    // Spec 5: talk to the target, reveal the contract and ask for double the payment.
    //   accept  when 2 x payment <= SalaryShare x salary / SalaryPeriods (no job = refuse)
    //   accept  -> the target pays at once, the contract ends without the client's payment (decided 19/10/2026)
    //   refuse  -> fighters (Payment.FighterJobs) attack, everyone else flees
    // Built with SOD.Common's dialog API; one attempt per contract.
    internal static class Bribe
    {
        private enum Outcome { None, Accept, Fight, Flee, NotTarget }

        private static DialogObject dialog;
        private static DialogResponse refuseResponse;

        // The offer being made right now (set when the option is shown / chosen, read by the text getters).
        private static SideJob offerJob;
        private static Outcome outcome;
        private static bool offerJobless;

        internal static bool WouldAccept(float salary, bool employed, int payment, out float limit)
        {
            limit = HitmanConfig.BribeSalaryShare.Value * salary / HitmanConfig.BribeSalaryPeriods.Value;
            return employed && salary > 0f && 2 * payment <= limit;
        }

        internal static void Register()
        {
            if (!HitmanConfig.BribeEnabled.Value) return;

            refuseResponse = new DialogResponse(RefuseLine, r =>
            {
                r.isSuccessful = false;
                r.endsDialog = true;
            });

            dialog = Lib.Dialogs.Builder("HitmanBribe")
                .SetText("There's a contract on your head. Pay me double and I disappear.")
                .ModifyDialogOptions(p => p.ranking = PhoneDialogs.Ranking) // on top of the game's options
                .SetDialogLogic(new Logic())
                .AddResponse(new DialogResponse(AcceptLine, r =>
                {
                    r.isSuccessful = true;
                    r.endsDialog = true;
                }))
                .AddResponse(refuseResponse)
                .CreateAndRegister();

            Plugin.Logger.LogInfo("[Bribe] Dialog option registered.");
        }

        // ---- lines (the game language is only known once a city is loaded, so text is set when shown) ----

        private static int PaymentOf(SideJob job)
        {
            var state = ContractStore.Get(job);
            if (state.Payment < 0) Payment.Apply(job, "bribe");
            return state.Payment >= 0 ? state.Payment : Payment.Calculate(job, out _);
        }

        private static string PlayerLine(SideJob job)
        {
            int p = PaymentOf(job);
            return Text.L($"Alguém me pagou Cr{p} pra te apagar. Me paga Cr{2 * p} e eu esqueço que você existe.",
                          $"Somebody paid me Cr{p} to take you out. Pay me Cr{2 * p} and I forget you exist.");
        }

        private static string AcceptLine() =>
            Text.L("...Tá bom. Toma o dinheiro. Você nunca me viu.",
                   "...Fine. Take the money. You never saw me.");

        private static string RefuseLine() => outcome switch
        {
            Outcome.Fight => Text.L("Você escolheu a pessoa errada pra ameaçar.",
                                    "You picked the wrong person to threaten."),
            Outcome.Flee when offerJobless => Text.L("Eu não tenho esse dinheiro! Socorro! Alguém chame a polícia!",
                                                     "I don't have that kind of money! Help! Somebody call the police!"),
            Outcome.Flee => Text.L("Socorro! Alguém chame a polícia!",
                                   "Help! Somebody call the police!"),
            _ => Text.L("Do que você tá falando? Me deixa em paz.",
                        "What are you talking about? Leave me alone."),
        };

        // ---- contract lookup ----

        private static bool Eligible(SideJob job)
        {
            if (!job.accepted || !ContractRegistry.IsKillContract(job)) return false;
            if (!ContractStore.TryGet(job, out var state)) return false;
            return state.InfoAcquired && !state.KillRecorded && !state.BribeAttempted && job.purp != null && !job.purp.isDead;
        }

        // The contract whose target is this citizen, else (when offering to everyone) any eligible contract.
        private static SideJob ContractFor(Citizen saysTo, out bool isTarget)
        {
            SideJob any = null;
            foreach (var job in ContractRegistry.ActiveHitmanJobs())
            {
                if (!Eligible(job)) continue;
                if (job.purp == saysTo)
                {
                    isTarget = true;
                    return job;
                }
                any ??= job;
            }
            isTarget = false;
            return HitmanConfig.BribeShowToEveryone.Value ? any : null;
        }

        private sealed class Logic : IDialogLogic
        {
            public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (saysTo == null || saysTo.isDead || saysTo.isPlayer) return false;
                var job = ContractFor(saysTo, out _);
                if (job == null) return false;
                offerJob = job;
                Lib.DdsStrings["dds.blocks", dialog.BlockId] = PlayerLine(job);
                return true;
            }

            public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption option, Citizen saysTo, NewNode where, Actor saidBy)
            {
                var job = ContractFor(saysTo, out bool isTarget);
                offerJob = job;
                offerJobless = false;
                if (job == null || !isTarget)
                {
                    outcome = Outcome.NotTarget;
                    SetRefuseFlags(fight: false, flee: false);
                    Plugin.Logger.LogInfo($"[Bribe] Offered to {Dev.Describe.Who(saysTo)}, not a target.");
                    return DialogController.ForceSuccess.fail;
                }

                var occupation = saysTo.job;
                bool employed = occupation != null && occupation.employer != null;
                float salary = Payment.AnnualSalary(occupation);
                string occupationName = occupation != null && occupation.preset != null ? occupation.preset.name : "(none)";
                int payment = PaymentOf(job);
                bool accept = WouldAccept(salary, employed, payment, out float limit);
                offerJobless = !employed;

                outcome = accept ? Outcome.Accept : Payment.IsFighter(occupationName) ? Outcome.Fight : Outcome.Flee;
                SetRefuseFlags(fight: outcome == Outcome.Fight, flee: outcome == Outcome.Flee);
                Plugin.Logger.LogInfo($"[Bribe] Job #{job.jobID}: offer Cr{2 * payment} to {Dev.Describe.Who(saysTo)} ({occupationName}, salary {salary:0}, employed {employed}), " +
                                      $"limit {limit:0} -> {outcome}");
                return accept ? DialogController.ForceSuccess.success : DialogController.ForceSuccess.fail;
            }

            public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
            {
                var job = offerJob;
                var result = outcome;
                outcome = Outcome.None;
                if (job == null || result == Outcome.NotTarget || result == Outcome.None) return;

                var state = ContractStore.Get(job);
                state.BribeAttempted = true;
                state.BribeOutcome = result.ToString();

                if (result == Outcome.Accept)
                {
                    int money = 2 * PaymentOf(job);
                    GameplayController.Instance.AddMoney(money, true, "Hitman bribe");
                    if (HitmanConfig.BribeTargetLeavesTown.Value)
                    {
                        LeaveTown(job, state, saysTo);
                        Text.Notify($"Suborno aceito: Cr{money}. {saysTo.GetCitizenName()} vai sumir da cidade. O cliente acha que o serviço foi feito: entregue o caso para receber dele também.",
                                    $"Bribe accepted: Cr{money}. {saysTo.GetCitizenName()} is leaving town. The client thinks the job is done: hand in the case to get paid by them too.");
                        return;
                    }
                    Plugin.Logger.LogInfo($"[Bribe] Job #{job.jobID}: accepted, paid Cr{money}, ending the contract (consequence: none, spec 8). game success={success}");
                    Text.Notify($"Suborno aceito: Cr{money}. O contrato foi encerrado.",
                                $"Bribe accepted: Cr{money}. The contract is closed.");
                    try { job.End(); }
                    catch (Exception e) { Plugin.Logger.LogError($"[Bribe] End() failed on job #{job.jobID}: {e}"); }
                    return;
                }

                var ai = saysTo != null ? saysTo.ai : null;
                Plugin.Logger.LogInfo($"[Bribe] Job #{job.jobID}: refused ({result}), game success={success}. AI after dialog: " +
                                      $"inCombat={ai?.inCombat} persuit={ai?.persuit} inFleeState={ai?.inFleeState}");
                Text.Notify(
                    result == Outcome.Fight ? "O alvo recusou e partiu para cima de você!" : "O alvo recusou e está fugindo. A polícia pode ser chamada.",
                    result == Outcome.Fight ? "The target refused and is coming at you!" : "The target refused and is running. The police may be called.");

                // v1.1: a target who ran lays low at home once the fright is over.
                if (result == Outcome.Flee) Consequences.Scare(saysTo, "refused a bribe");

                if (result == Outcome.Fight && ai != null && !ai.inCombat && !ai.persuit && HitmanConfig.BribeForceCombatFallback.Value)
                {
                    Plugin.Logger.LogInfo($"[Bribe] Job #{job.jobID}: dialog did not start combat, forcing pursuit.");
                    ai.SetPersue(Player.Instance, false, 2, true);
                }
            }
        }

        // ---- v0.7: the bribed target leaves town ----
        // The target vanishes from the city (Human.RemoveFromWorld) and the contract counts as done, so the client
        // pays at the hand-in too. Except on the first mission (decided 19/10/2026: no consequence there), the client
        // may find out BribeDiscoveryChance of the time, 1 to 3 days later, and send a killer after the player.

        private const string FirstMission = "HitmanPizza";

        private static void LeaveTown(SideJob job, ContractState state, Citizen target)
        {
            state.BribedAway = true;
            KillTracker.CompleteKill(job, null, "bribe: the target left town");
            var heat = ContractStore.Heat;
            heat.LeftTownIds ??= new System.Collections.Generic.List<int>();
            if (!heat.LeftTownIds.Contains(target.humanID)) heat.LeftTownIds.Add(target.humanID);
            try { target.RemoveFromWorld(true); }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Bribe] RemoveFromWorld failed for {Dev.Describe.Who(target)}: {e.Message}"); }

            bool risky = job.preset == null || job.preset.name != FirstMission;
            if (risky && new Random().NextDouble() < HitmanConfig.BribeDiscoveryChance.Value)
                state.BribeDiscoverAt = Text.Now + 24f + (float)new Random().NextDouble() * 48f;
            Plugin.Logger.LogInfo($"[Bribe] Job #{job.jobID}: {Dev.Describe.Who(target)} left town; client finds out at " +
                                  $"{(state.BribeDiscoverAt >= 0 ? Text.Clock(state.BribeDiscoverAt) : "never")}");
        }

        // Every game minute: clients who find out the target is alive.
        internal static void Tick(float now)
        {
            foreach (var state in ContractStore.All)
            {
                if (state.BribeDiscoverAt < 0f || now < state.BribeDiscoverAt) continue;
                state.BribeDiscoverAt = -1f;
                Plugin.Logger.LogInfo($"[Bribe] The client of job #{state.JobId} found out {state.TargetName} is alive.");
                Text.Notify($"O cliente descobriu que {state.TargetName ?? "o alvo"} está vivo. Alguém foi pago para te pegar.",
                            $"The client found out {state.TargetName ?? "the target"} is alive. Someone has been paid to get you.");
                Heat.HeatSystem.SendPersonalHunter($"client of job #{state.JobId} found out about the bribe");
            }
        }

        // The game's save may bring a citizen who left town back; keep them gone.
        internal static void ReapplyOnLoad()
        {
            var ids = ContractStore.Heat.LeftTownIds;
            if (ids == null || ids.Count == 0 || CityData.Instance == null) return;
            foreach (var id in ids)
                if (CityData.Instance.GetHuman(id, out var h, false) && h != null && !h.isDead)
                {
                    try { h.RemoveFromWorld(true); } catch { }
                }
            Plugin.Logger.LogInfo($"[Bribe] {ids.Count} bribed target(s) kept out of town after load.");
        }

        private static void SetRefuseFlags(bool fight, bool flee)
        {
            var info = refuseResponse?.ResponseInfo;
            if (info == null) return;
            info.startCombat = fight;
            info.flee = flee;
        }
    }
}
