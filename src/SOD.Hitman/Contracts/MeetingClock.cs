using System;
using System.Collections.Generic;
using HarmonyLib;

namespace SODHitman.Contracts
{
    // Vanilla meetings have no time: the client just sits at the place with an item. Contracts add one (spec 4):
    // the time is set when the client first names it on the phone, and the contract fails when the player has not
    // talked to the client by meeting time + LateToleranceMinutes.
    // DDS text can show it through |sidejob.custom_hitman_meettime| and |sidejob.custom_hitman_deadline|.
    internal static class MeetingClock
    {
        private const string VarPrefix = "custom_hitman_";
        private static readonly HashSet<string> Logged = new();

        internal static ContractState EnsureMeeting(SideJob job, string source)
        {
            var state = ContractStore.Get(job);
            if (state.MeetTime >= 0f) return state;

            float now = Text.Now;
            float meet = now + HitmanConfig.MeetDelayMinutes.Value / 60f;
            meet = (float)(Math.Ceiling(meet * 12.0) / 12.0); // next 5 minutes
            state.MeetTime = meet;
            state.Deadline = meet + HitmanConfig.LateToleranceMinutes.Value / 60f;

            var place = Dev.Describe.Where(job.chosenMeetingPoint);
            Plugin.Logger.LogInfo($"[Meet] Job #{job.jobID} ({source}): now {now:0.000} ({Text.Clock(now)}), meeting {Text.Clock(state.MeetTime)}, " +
                                  $"deadline {Text.Clock(state.Deadline)}, place={place ?? "not set yet"}, phase={job.phase}");
            Text.Notify(
                $"Encontro com o cliente às {Text.Clock(state.MeetTime)}. Tolerância até {Text.Clock(state.Deadline)}.",
                $"Meeting the client at {Text.Clock(state.MeetTime)}. You have until {Text.Clock(state.Deadline)}.");
            return state;
        }

        internal static void Tick()
        {
            float now = Text.Now;
            foreach (var job in ContractRegistry.ActiveHitmanJobs())
            {
                if (!job.accepted || !ContractRegistry.UsesMeetingIntro(job)) continue;

                // Fallback when neither the call hook nor the dialog text set the time: the job sits on its meeting step.
                if (!ContractStore.TryGet(job, out var state) || state.MeetTime < 0f)
                {
                    var block = job.currentBlock;
                    if (block == null || block.elementType != SideMissionIntroPreset.SideMissionElementType.setMeeting) continue;
                    state = EnsureMeeting(job, "meeting step");
                }

                // The intro has no step left: the meeting happened even if no info hook fired.
                if (!state.InfoAcquired && job.currentBlock == null)
                {
                    state.InfoAcquired = true;
                    Plugin.Logger.LogInfo($"[Meet] Job #{job.jobID}: intro finished, counting the meeting as done.");
                    Payment.PayDeposit(job, state);
                }

                if (state.InfoAcquired || state.FailedLate || state.KillRecorded) continue;
                if (!HitmanConfig.EnforceMeetingDeadline.Value || now <= state.Deadline) continue;

                state.FailedLate = true;
                Plugin.Logger.LogInfo($"[Meet] Job #{job.jobID}: late (now {Text.Clock(now)}, deadline {Text.Clock(state.Deadline)}), failing. {Dev.Describe.Job(job)}");
                Text.Notify("Você não apareceu no encontro. O cliente cancelou o contrato.",
                            "You missed the meeting. The client called off the contract.");
                job.TriggerFail(JobGuards.OwnFailPrefix + " missed the meeting");
            }
        }

        private static SideJob FindJob(Il2CppSystem.Object inputObject, Il2CppSystem.Object baseObject)
        {
            var job = inputObject != null ? inputObject.TryCast<SideJob>() : null;
            if (job == null && baseObject != null) job = baseObject.TryCast<SideJob>();
            if (ContractRegistry.IsHitmanJob(job)) return job;

            var invoked = SideJobController.Instance != null ? SideJobController.Instance.invokedSideJob : null;
            if (ContractRegistry.IsHitmanJob(invoked)) return invoked;

            var current = ContractRegistry.CurrentAcceptedJob();
            return ContractRegistry.IsHitmanJob(current) ? current : null;
        }

        [HarmonyPatch(typeof(Strings), nameof(Strings.GetContainedValue))]
        internal static class Strings_GetContainedValue
        {
            static bool Prefix(ref string __result, Il2CppSystem.Object baseObject, string withinScope, string newValue, Il2CppSystem.Object inputObject)
            {
                if (newValue == null || !newValue.StartsWith(VarPrefix)) return true;

                var job = FindJob(inputObject, baseObject);
                if (Logged.Add($"{withinScope}.{newValue}:{job != null}"))
                    Plugin.Logger.LogInfo($"[Meet] Text variable {withinScope}.{newValue} requested, job={(job != null ? "#" + job.jobID : "not found")}");
                if (job == null)
                {
                    __result = "??:??";
                    return false;
                }

                var state = EnsureMeeting(job, "dialog text");
                switch (newValue)
                {
                    case VarPrefix + "meettime":
                        __result = Text.Clock(state.MeetTime);
                        return false;
                    case VarPrefix + "deadline":
                        __result = Text.Clock(state.Deadline);
                        return false;
                    default:
                        return true;
                }
            }
        }

        // The game updates the job's Details tab silently after the meeting/call; tell the player (spec 4).
        private static void OnInfo(SideJob job, string dialogName)
        {
            if (!ContractRegistry.IsHitmanJob(job)) return;
            var state = ContractStore.Get(job);
            Plugin.Logger.LogInfo($"[Meet] Job #{job.jobID}: OnAcquireJobInfo({dialogName}), meeting set={state.MeetTime >= 0f}, infoBefore={state.InfoAcquired}");

            // The phone call that sets a meeting is not the meeting itself. Without a meeting the client's own call
            // (HitmanCall) or the payphone call (HitmanPayphone) is the info; the game's opening call (Job_*) is not.
            if (dialogName != null && (ContractRegistry.UsesMeetingIntro(job) ? dialogName.EndsWith("CallDialog") : dialogName.StartsWith("Job_"))) return;
            if (state.InfoAcquired) return;
            state.InfoAcquired = true;
            Text.Notify("Detalhes do serviço atualizados. Confira a aba Detalhes do caso.",
                        "Job details updated. Check the case's Details tab.");
            Payment.PayDeposit(job, state);
        }

        // The player called the posting's number: that call is where the client names the meeting.
        [HarmonyPatch(typeof(SideJob), nameof(SideJob.OnPlayerCall))]
        internal static class SideJob_OnPlayerCall
        {
            static void Postfix(SideJob __instance)
            {
                if (ContractRegistry.IsHitmanJob(__instance) && ContractRegistry.UsesMeetingIntro(__instance))
                    EnsureMeeting(__instance, "player called");
            }
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.OnAcquireJobInfo), new[] { typeof(DialogPreset) })]
        internal static class SideJob_OnAcquireJobInfo_Dialog
        {
            static void Postfix(SideJob __instance, DialogPreset dialog) => OnInfo(__instance, Dev.Describe.N(dialog));
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.OnAcquireJobInfo), new[] { typeof(string) })]
        internal static class SideJob_OnAcquireJobInfo_Message
        {
            static void Postfix(SideJob __instance, string infoDialogMessage) => OnInfo(__instance, infoDialogMessage);
        }
    }
}
