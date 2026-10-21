using System;
using System.Linq;
using HarmonyLib;
using SOD.Common;
using SOD.Common.Helpers.DialogObjects;
using SODHitman.Heat;
using static SODHitman.Dev.Describe;

namespace SODHitman.Contracts
{
    // v1.2 of the roadmap: the campaign "O Último Contrato" (The Last Contract), told through phone calls.
    //  stage 0  not started. Once the player is a Professional (StartReputation) and knows the Fixer, he calls about
    //           a big client.
    //  1 to 3   three contracts for that client, asked for on the Fixer's phone ("About the big client"):
    //           1 the test (HitmanCall), 2 the witness (HitmanPayphone), 3 the uniform (HitmanBadge), each paying
    //           CampaignPayMultiplier more. After 1 and 2 the Fixer calls with the next one.
    //  4        after the third, the corrupt cop calls: the Fixer sold the player out, and a killer is on the way.
    //           The player kills the Fixer, or calls the cop and hands him over. The cop stays loyal (decided 20/10/2026).
    //  5        over: CampaignEnding "killed", "arrested", or "lost" when the Fixer died before the end.
    internal static class Campaign
    {
        private static readonly string[] ChapterPresets = { "HitmanCall", "HitmanPayphone", "HitmanBadge" };
        internal const string Requester = "campaign";

        private static HeatState H => ContractStore.Heat;
        internal static bool Enabled => HitmanConfig.CampaignEnabled.Value;

        // ---- registration (plugin load): the cop's "hand over the Fixer" option, on the phone and face to face ----

        internal static void Register()
        {
            if (!Enabled || !HitmanConfig.CopEnabled.Value) return;
            foreach (bool phone in new[] { true, false })
            {
                var logic = new HandOverLogic(phone);
                logic.Reply = new DialogResponse(() => "...", r =>
                {
                    r.isSuccessful = true;
                    r.endsDialog = true;
                });
                logic.Self = Lib.Dialogs.Builder("HitmanCopHandOverFixer" + (phone ? "" : "P"))
                    .SetText("...")
                    .ModifyDialogOptions(p => PhoneDialogs.MenuOption(p, phone))
                    .SetDialogLogic(logic)
                    .AddResponse(logic.Reply)
                    .CreateAndRegister();
                PhoneDialogs.Add(logic.Self);
            }
        }

        private static string FixerName => H.FixerName ?? "Fixer";

        // ---- lines ----

        private static string StartLine() =>
            Text.L($"Aqui é {FixerName}. Apareceu um cliente grande, gente com dinheiro de verdade. São três serviços, e cada um paga bem mais que o normal. Vem falar comigo e pergunta pelo cliente grande.",
                   $"This is {FixerName}. A big client showed up, people with real money. Three jobs, and each pays a lot more than usual. Come see me and ask about the big client.");

        private static string NextLine(int stage) => stage == 2
            ? Text.L($"{FixerName} de novo. O cliente gostou do que viu. Tem o segundo serviço. Vem me ver.", $"{FixerName} again. The client liked what they saw. There's a second job. Come see me.")
            : Text.L($"{FixerName}. Falta um, o último e o maior. Vem me ver.", $"{FixerName}. One left, the last and the biggest. Come see me.");

        private static string CopWarningLine() =>
            Text.L($"Aqui é {H.CopName}. Escuta bem: o seu intermediário, {FixerName}, vendeu o seu nome. O cliente grande está apagando quem sabe demais, e já mandaram alguém atrás de você. Me liga se quiser que eu prenda {FixerName}. Ou resolve do seu jeito.",
                   $"This is {H.CopName}. Listen: your middleman, {FixerName}, sold your name. The big client is erasing everyone who knows too much, and someone's already after you. Call me if you want me to lock {FixerName} up. Or deal with it your way.");

        private static string ChapterBriefing(int stage) => stage switch
        {
            1 => Text.L("O cliente quer ver se você é bom antes de abrir a carteira. Um serviço simples.",
                        "The client wants to see if you're any good before opening the wallet. A simple job."),
            2 => Text.L("Agora tem alguém que viu o que não devia. Limpo, sem testemunha.",
                        "Now there's someone who saw what they shouldn't have. Clean, no witnesses."),
            _ => Text.L("Esse é o grande. O alvo usa uniforme e sabe se defender. Depois desse, o cliente some e você fica rico.",
                        "This is the big one. The target wears a uniform and can fight back. After this one, the client disappears and you're rich."),
        };

        // ---- the Fixer's phone option (Fixer.CampaignLogic) ----

        private static SideJob CampaignJob() =>
            H.CampaignJobId < 0 ? null : ContractRegistry.AllJobs().FirstOrDefault(j => j.jobID == H.CampaignJobId);

        // The option's text, or null when it is not offered.
        internal static string FixerOptionText()
        {
            if (!Enabled || H.CampaignStage < 1 || H.CampaignStage > 3 || H.CampaignJobId >= 0) return null;
            return Text.L("Sobre o cliente grande…", "About the big client…");
        }

        internal static string OnFixerOption()
        {
            int stage = H.CampaignStage;
            var names = new System.Collections.Generic.List<string> { ChapterPresets[stage - 1] };
            names.AddRange(Career.UnlockedKillPresets());
            names.Add("HitmanPizza");
            foreach (var name in names.Distinct())
            {
                // Handed over on the spot, like the Fixer's other jobs (decided 20/10/2026).
                var job = Postings.Request(name, Requester, Fixer.HandOver, out var failure);
                if (job == null)
                {
                    Plugin.Logger.LogInfo($"[Campaign] Chapter {stage}: '{name}' not posted: {failure}");
                    continue;
                }
                H.CampaignJobId = job.jobID;
                Plugin.Logger.LogInfo($"[Campaign] Chapter {stage}: job #{job.jobID} ({name})");
                Text.Notify($"“O Último Contrato”, capítulo {stage}.", $"“The Last Contract”, chapter {stage}.");
                return ChapterBriefing(stage) + " " + Fixer.Brief(job);
            }
            return Text.L("O cliente ainda não mandou o nome. Volta mais tarde.", "The client hasn't sent the name yet. Come back later.");
        }

        // ---- chapter done ----

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.Complete))]
        internal static class SideJob_Complete
        {
            static void Postfix(SideJob __instance)
            {
                if (__instance == null || H.CampaignJobId < 0 || __instance.jobID != H.CampaignJobId) return;
                ChapterDone();
            }
        }

        private static void ChapterDone()
        {
            int done = H.CampaignStage;
            H.CampaignJobId = -1;
            H.CampaignStage = done + 1;
            H.CampaignCallAt = Text.Now + 1f;
            Plugin.Logger.LogInfo($"[Campaign] Chapter {done} done; stage {H.CampaignStage}, call at {Text.Clock(H.CampaignCallAt)}");
            if (done < 3)
                Text.Notify($"“O Último Contrato”: capítulo {done} concluído. {FixerName} vai ligar para a sua casa.",
                            $"“The Last Contract”: chapter {done} done. {FixerName} will call your home.");
            else
                Text.Notify("“O Último Contrato”: capítulo 3 concluído. Alguém vai ligar para a sua casa.",
                            "“The Last Contract”: chapter 3 done. Someone will call your home.");
        }

        // ---- the betrayal and the endings ----

        private static void Betrayal(bool copWarned)
        {
            if (H.CampaignBetrayed) return;
            H.CampaignBetrayed = true;
            HeatSystem.SendPersonalHunter($"the Fixer {FixerName} cleaning up loose ends");
            Plugin.Logger.LogInfo($"[Campaign] Betrayal revealed (cop warned: {copWarned})");
            Text.Notify(copWarned
                    ? $"“O Último Contrato”: {FixerName} te vendeu. Mate {FixerName}, ou ligue para {H.CopName} e entregue-o."
                    : $"“O Último Contrato”: {FixerName} te vendeu. Mate {FixerName} antes que te peguem.",
                copWarned
                    ? $"“The Last Contract”: {FixerName} sold you out. Kill {FixerName}, or call {H.CopName} and hand them over."
                    : $"“The Last Contract”: {FixerName} sold you out. Kill {FixerName} before they get you.");
        }

        private static void Ending(string kind)
        {
            H.CampaignStage = 5;
            H.CampaignEnding = kind;
            H.CampaignCallAt = -1f;
            H.CampaignJobId = -1;
            HeatSystem.CancelPersonalHunters();
            Plugin.Logger.LogInfo($"[Campaign] Ending: {kind}");
            switch (kind)
            {
                case "killed":
                {
                    int reward = HitmanConfig.CampaignFinalReward.Value;
                    if (reward > 0) GameplayController.Instance?.AddMoney(reward, true, "Hitman: the last contract");
                    H.Earned += reward;
                    Career.AddReputation(5, "campaign: killed the Fixer");
                    Text.Notify($"{FixerName} está morto. Sem intermediário, sem pontas soltas, e o dinheiro do cliente que estava com {FixerName} agora é seu: Cr{reward}. Fim de “O Último Contrato”.",
                                $"{FixerName} is dead. No middleman, no loose ends, and the client's money {FixerName} was holding is yours now: Cr{reward}. The end of “The Last Contract”.");
                    break;
                }
                case "arrested":
                {
                    var fixer = Fixer.DevFixer();
                    if (fixer != null)
                    {
                        H.LeftTownIds ??= new System.Collections.Generic.List<int>();
                        if (!H.LeftTownIds.Contains(fixer.humanID)) H.LeftTownIds.Add(fixer.humanID);
                        try { fixer.RemoveFromWorld(true); }
                        catch (Exception e) { Plugin.Logger.LogWarning($"[Campaign] RemoveFromWorld failed for the Fixer: {e.Message}"); }
                    }
                    // The Fixer is gone: Fixer.Tick finds a replacement after FixerReplaceDays, who introduces himself
                    // (after a load, Fixer.EnsureReady picks him at once, and he calls once that time has come).
                    H.FixerId = -1;
                    H.FixerContacted = false;
                    H.FixerNextCall = Text.Now + HitmanConfig.FixerReplaceDays.Value * 24f;
                    if (H.Points > 0) HeatSystem.Change(-H.Points, "campaign: the cop buried the player's file with the Fixer's");
                    Career.AddReputation(3, "campaign: handed the Fixer over");
                    Text.Notify($"{H.CopName}: “{FixerName} está numa cela, e a sua ficha sumiu junto com a dele.” Heat zerado. Fim de “O Último Contrato”.",
                                $"{H.CopName}: “{FixerName} is in a cell, and your file went missing along with theirs.” Heat cleared. The end of “The Last Contract”.");
                    break;
                }
                default:
                    Text.Notify($"{FixerName} morreu antes do fim. O cliente grande sumiu, e “O Último Contrato” acabou.",
                                $"{FixerName} died before the end. The big client vanished, and “The Last Contract” is over.");
                    break;
            }
        }

        private sealed class HandOverLogic : IDialogLogic
        {
            private readonly bool phone;
            internal DialogObject Self;
            internal DialogResponse Reply;

            public HandOverLogic(bool phone) => this.phone = phone;

            public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (PhoneCalls.DialogIsRemote() != phone) return false;
                bool withCop = CorruptCop.InCopCall() || (saysTo != null && H.CopId >= 0 && saysTo.humanID == H.CopId);
                if (!Enabled || H.CampaignStage != 4 || !H.CampaignBetrayed || !withCop) return false;
                Lib.DdsStrings["dds.blocks", Self.BlockId] = Text.L($"Quero entregar {FixerName}.", $"I want to hand over {FixerName}.");
                return true;
            }

            public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
            {
                Lib.DdsStrings["dds.blocks", Reply.BlockId] = Text.L(
                    $"Considere feito. Amanhã {FixerName} acorda numa cela, e a sua papelada some junto.",
                    $"Consider it done. Tomorrow {FixerName} wakes up in a cell, and your paperwork disappears with it.");
                return DialogController.ForceSuccess.success;
            }

            public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
            {
                if (H.CampaignStage == 4) Ending("arrested");
            }
        }

        // ---- every game minute ----

        internal static void Tick(float now)
        {
            if (!Enabled || H.CampaignStage >= 5) return;
            var fixer = Fixer.DevFixer();
            bool fixerAlive = fixer != null && !fixer.isDead && H.FixerId >= 0;

            if (H.CampaignStage >= 1 && !fixerAlive)
            {
                Ending(H.CampaignStage == 4 ? "killed" : "lost");
                return;
            }

            if (H.CampaignStage == 0)
            {
                if (!HitmanConfig.FixerEnabled.Value || !H.FixerContacted || !fixerAlive || H.Reputation < HitmanConfig.CampaignStartReputation.Value) return;
                if (H.CampaignCallAt < 0f)
                {
                    H.CampaignCallAt = now + 1f;
                    Plugin.Logger.LogInfo($"[Campaign] Reputation {H.Reputation}: the Fixer calls at {Text.Clock(H.CampaignCallAt)}");
                }
            }

            // A chapter's job that ended without the hand-in (failed, missed meeting, target dead by other hands).
            if (H.CampaignJobId >= 0)
            {
                var job = CampaignJob();
                if (job == null || job.failed || job.state == SideJob.JobState.ended)
                {
                    Plugin.Logger.LogInfo($"[Campaign] Chapter {H.CampaignStage} job #{H.CampaignJobId} ended without a hand-in.");
                    H.CampaignJobId = -1;
                    Text.Notify($"O serviço do cliente grande não deu certo. Fale com {FixerName} pessoalmente e peça de novo.",
                                $"The big client's job fell through. Talk to {FixerName} in person and ask again.");
                }
            }

            if (H.CampaignCallAt < 0f || now < H.CampaignCallAt || StoryCall.Busy) return;
            H.CampaignCallAt = now + HitmanConfig.CopRetryHours.Value; // until answered
            switch (H.CampaignStage)
            {
                case 0:
                    StoryCall.Start(fixer, StartLine(), () =>
                    {
                        H.CampaignStage = 1;
                        H.CampaignCallAt = -1f;
                        Plugin.Logger.LogInfo("[Campaign] Started.");
                        Text.Notify($"Nova história: “O Último Contrato”. Vá falar com {FixerName} e pergunte pelo cliente grande (ligue para o número dele para saber onde ele está).",
                                    $"New story: “The Last Contract”. Go talk to {FixerName} and ask about the big client (call his number to find out where he is).");
                    });
                    break;
                case 2:
                case 3:
                    StoryCall.Start(fixer, NextLine(H.CampaignStage), () => H.CampaignCallAt = -1f);
                    break;
                case 4:
                    var cop = CorruptCop.DevCop();
                    if (!HitmanConfig.CopEnabled.Value || cop == null || cop.isDead)
                    {
                        H.CampaignCallAt = -1f;
                        Betrayal(copWarned: false);
                    }
                    else
                        StoryCall.Start(cop, CopWarningLine(), () =>
                        {
                            H.CampaignCallAt = -1f;
                            CorruptCop.DevMarkContacted(); // his number goes into the notebook
                            Betrayal(copWarned: true);
                        });
                    break;
                default:
                    H.CampaignCallAt = -1f;
                    break;
            }
        }

        // ---- dev menu ----

        internal static string Status() => H.CampaignStage switch
        {
            0 => Text.L(H.CampaignCallAt >= 0 ? $"o Fixer liga às {Text.Clock(H.CampaignCallAt)}" : "não começou", H.CampaignCallAt >= 0 ? $"the Fixer calls at {Text.Clock(H.CampaignCallAt)}" : "not started"),
            >= 1 and <= 3 => Text.L($"capítulo {H.CampaignStage}" + (H.CampaignJobId >= 0 ? $", contrato #{H.CampaignJobId}" : ", peça ao Fixer"),
                                    $"chapter {H.CampaignStage}" + (H.CampaignJobId >= 0 ? $", job #{H.CampaignJobId}" : ", ask the Fixer")),
            4 => Text.L(H.CampaignBetrayed ? "traição: mate ou entregue o Fixer" : "o policial vai ligar", H.CampaignBetrayed ? "betrayal: kill or hand over the Fixer" : "the cop will call"),
            _ => Text.L($"fim ({H.CampaignEnding})", $"over ({H.CampaignEnding})"),
        };

        internal static void DevStart()
        {
            Fixer.DevMarkContacted();
            if (H.CampaignStage >= 5) H.CampaignStage = 0;
            H.CampaignBetrayed = false;
            H.CampaignEnding = null;
            H.CampaignStage = 0;
            H.CampaignJobId = -1;
            H.CampaignCallAt = Text.Now;
            if (H.Reputation < HitmanConfig.CampaignStartReputation.Value) Career.AddReputation(HitmanConfig.CampaignStartReputation.Value - H.Reputation, "dev menu: campaign");
            Text.Notify("[Menu] O Fixer vai ligar para a sua casa no próximo minuto (fique em casa).", "[Menu] The Fixer will call your home next minute (stay at home).");
        }

        internal static void DevSkipChapter()
        {
            if (H.CampaignStage < 1 || H.CampaignStage > 3)
            {
                Text.Notify("[Menu] Nenhum capítulo em andamento.", "[Menu] No chapter in progress.");
                return;
            }
            ChapterDone();
            H.CampaignCallAt = Text.Now;
        }

        internal static void DevBetrayalNow()
        {
            H.CampaignStage = 4;
            H.CampaignJobId = -1;
            H.CampaignBetrayed = false;
            H.CampaignCallAt = Text.Now;
            Text.Notify("[Menu] O policial vai ligar para a sua casa no próximo minuto (fique em casa).", "[Menu] The cop will call your home next minute (stay at home).");
        }
    }
}
