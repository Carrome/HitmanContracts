using System.Collections.Generic;
using HarmonyLib;
using SOD.Common.Helpers.DialogObjects;

namespace SODHitman
{
    // SOD.Common 2.0.7's prefix on DialogController.ExecuteDialog casts the listener to Citizen
    // (saysTo.isActor.Cast<Citizen>()) for every custom dialog. On the phone the listener is the player (a call to
    // the player) or a bare telephone with no actor (the player calling a number), so the cast throws, Harmony drops
    // the call and the game never runs the dialog: the corrupt cop said nothing and his packages did nothing.
    // That prefix only acts while forceSuccess is still "none", so this one, which runs first, decides the outcome
    // of the mod's phone dialogs itself whenever the listener is not a citizen.
    internal static class PhoneDialogs
    {
        private static readonly Dictionary<string, IDialogLogic> Logic = new();

        // InteractionController.RefreshDialogOptions sorts the options by DialogPreset.ranking, highest first. SOD.Common
        // gives custom dialogs ranking 1, under every game option; the mod's go on top (decided 20/10/2026, 8th test).
        internal const int Ranking = 1000;

        // A menu option of the cop or the Fixer (and the campaign's hand-over), phone or face-to-face form.
        // The game lists a citizen's options by evidence key: face to face photo + voice, on the phone voice only.
        // SOD.Common ties custom dialogs to photo, so they never showed on the phone; voice works in both.
        internal static void MenuOption(DialogPreset p, bool phone)
        {
            p.defaultOption = true; // SOD.Common only offers dialogs in the game's default options; IsDialogShown filters
            p.telephoneCallOption = phone;
            p.tiedToKey = Evidence.DataKey.voice;
            p.ranking = Ranking;
        }

        internal static void Add(DialogObject dialog)
        {
            if (dialog?.Preset != null && dialog.DialogLogic != null) Logic[dialog.Preset.name] = dialog.DialogLogic;
        }

        [HarmonyPatch(typeof(DialogController), nameof(DialogController.ExecuteDialog))]
        [HarmonyPriority(Priority.First)]
        internal static class DialogController_ExecuteDialog
        {
            static void Prefix(DialogController __instance, EvidenceWitness.DialogOption dialog, Interactable saysTo, NewNode where, Actor saidBy,
                               ref DialogController.ForceSuccess forceSuccess)
            {
                if (forceSuccess != DialogController.ForceSuccess.none || dialog == null || dialog.preset == null) return;
                if (!Logic.TryGetValue(dialog.preset.name, out var logic)) return;
                var actor = saysTo != null ? saysTo.isActor : null;
                if (actor != null && actor.TryCast<Citizen>() != null) return; // SOD.Common handles this one

                var decided = logic.ShouldDialogSucceedOverride(__instance, dialog, null, where, saidBy);
                forceSuccess = decided == DialogController.ForceSuccess.none ? DialogController.ForceSuccess.success : decided;
            }
        }
    }
}
