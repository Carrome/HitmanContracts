using System;
using SOD.Common;
using SOD.Common.Helpers.DialogObjects;
using SODHitman.Contracts;
using static SODHitman.Dev.Describe;

namespace SODHitman.Heat
{
    // v0.7: talk a witness out of going to the police before they do (Evidence). Same pattern as the target bribe:
    // one dialog option, shown only with a witness who has not talked yet. Enforcers never take money; others accept
    // with WitnessBribeAcceptChance. A refusal sends them to the police right away, running.
    internal static class WitnessTalk
    {
        private enum Outcome { None, Accept, Refuse, NoMoney }

        private static DialogObject dialog;
        private static DialogResponse refuseResponse;
        private static Outcome outcome;
        private static readonly Random Rng = new();

        internal static int Price => Math.Max(0, HitmanConfig.WitnessBribePrice.Value);

        internal static void Register()
        {
            if (!HitmanConfig.StealthEnabled.Value) return;
            refuseResponse = new DialogResponse(RefuseLine, r =>
            {
                r.isSuccessful = false;
                r.endsDialog = true;
            });
            dialog = Lib.Dialogs.Builder("HitmanSilenceWitness")
                .SetText("You didn't see anything, right?")
                .ModifyDialogOptions(p => p.ranking = PhoneDialogs.Ranking) // on top of the game's options
                .SetDialogLogic(new Logic())
                .AddResponse(new DialogResponse(AcceptLine, r =>
                {
                    r.isSuccessful = true;
                    r.endsDialog = true;
                }))
                .AddResponse(refuseResponse)
                .CreateAndRegister();
            Plugin.Logger.LogInfo("[Witness] Dialog option registered.");
        }

        private static string PlayerLine() =>
            Text.L($"Você não viu nada, certo? Toma Cr{Price} pra ajudar a esquecer.", $"You didn't see anything, right? Here's Cr{Price} to help you forget.");

        private static string AcceptLine() =>
            Text.L("...Ver o quê? Eu estava olhando pro outro lado.", "...See what? I was looking the other way.");

        private static string RefuseLine() => outcome == Outcome.NoMoney
            ? Text.L("Você nem tem esse dinheiro. Sai daqui.", "You don't even have that money. Get lost.")
            : Text.L("Fica longe de mim! Eu vou contar tudo pra polícia!", "Stay away from me! I'm telling the police everything!");

        private sealed class Logic : IDialogLogic
        {
            public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (saysTo == null || saysTo.isDead || saysTo.isPlayer || !KillEvidence.IsPendingWitness(saysTo)) return false;
                Lib.DdsStrings["dds.blocks", dialog.BlockId] = PlayerLine();
                return true;
            }

            public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption option, Citizen saysTo, NewNode where, Actor saidBy)
            {
                var gameplay = GameplayController.Instance;
                if (gameplay == null || gameplay.money < Price) outcome = Outcome.NoMoney;
                else if (saysTo.isEnforcer || Rng.NextDouble() > HitmanConfig.WitnessBribeAcceptChance.Value) outcome = Outcome.Refuse;
                else outcome = Outcome.Accept;
                if (refuseResponse?.ResponseInfo != null) refuseResponse.ResponseInfo.flee = outcome == Outcome.Refuse;
                Plugin.Logger.LogInfo($"[Witness] Offer of Cr{Price} to {Who(saysTo)} -> {outcome}");
                return outcome == Outcome.Accept ? DialogController.ForceSuccess.success : DialogController.ForceSuccess.fail;
            }

            public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
            {
                var result = outcome;
                outcome = Outcome.None;
                if (saysTo == null) return;
                switch (result)
                {
                    case Outcome.Accept:
                        GameplayController.Instance.AddMoney(-Price, true, "Witness bribe");
                        KillEvidence.Silence(saysTo, "bribed");
                        Text.Notify($"{saysTo.GetCitizenName()} aceitou o dinheiro e não vai falar com a polícia.",
                                    $"{saysTo.GetCitizenName()} took the money and won't talk to the police.");
                        break;
                    case Outcome.Refuse:
                        KillEvidence.ReportNow(saysTo);
                        break;
                }
            }
        }
    }
}
