using System;
using System.Collections.Generic;
using HarmonyLib;
using SOD.Common;

namespace SODHitman
{
    // Phone calls the game's own way (found after the 1.2.0 test, 20/10/2026, in the game code):
    //  - Every call the game makes to the player (the killer's cover-up offer, the side jobs' payphones) is
    //    CallSource(CallType.player, greeting) with no caller and specificReceiver = true. On pick-up the game opens a
    //    dialog with the player's phone and runs the greeting, whose response is the voice on the line. The mod used
    //    CallType.dds, which has the caller speak a DDS tree where he stands, miles away: the player heard nothing.
    //    The greeting is HitmanPhoneLine (content/Phone), whose one block is set to the line before each call.
    //  - When the player dials a fake number nobody answers: PhoneCall.recevierNS stays null and the game runs the
    //    greeting with the player as the listener, which DialogController.ExecuteDialog cannot handle (it threw a
    //    NullReferenceException on the cop's and the Fixer's numbers). Side job numbers work because the job's client
    //    takes the call. Here the citizen behind the number (the cop, the Fixer) takes it the same way, just before the
    //    game handles the pick-up, so the call runs as a call to that citizen.
    //    KeypadController creates a call to a fake number with no telephone at the other end (PhoneCall.to is not the
    //    number), passing the CallSource registered for it: the call is recognised by that source's greeting.
    internal static class PhoneCalls
    {
        internal const string LineDialogName = "HitmanPhoneLine";

        private static DialogPreset line;
        private static readonly Dictionary<string, Func<Human>> Answerers = new();

        internal static DialogPreset LinePreset
        {
            get
            {
                if (line != null) return line;
                var all = Toolbox.Instance != null ? Toolbox.Instance.allDialog : null;
                if (all == null) return null;
                for (int i = 0; i < all.Count; i++)
                    if (all[i] != null && all[i].name == LineDialogName) return line = all[i];
                return null;
            }
        }

        // The player is called on `to` from `from`, and the voice says `text` when they pick up.
        internal static TelephoneController.PhoneCall CallPlayer(Telephone from, Telephone to, string text)
        {
            var player = Player.Instance;
            if (from == null || to == null || player == null || TelephoneController.Instance == null) return null;
            Lib.DdsStrings["dds.blocks", Heat.CorruptCop.CallLineBlockId] = text;
            var preset = LinePreset;
            TelephoneController.CallSource source;
            if (preset != null)
                source = new TelephoneController.CallSource(TelephoneController.CallType.player, preset, InteractionController.ConversationType.normal);
            else
            {
                Plugin.Logger.LogWarning($"[Phone] {LineDialogName} is not loaded; falling back to a DDS tree call (the line goes on screen).");
                source = new TelephoneController.CallSource(TelephoneController.CallType.dds, Heat.CorruptCop.CallTreeId);
            }
            var call = TelephoneController.Instance.CreateNewCall(from, to, null, player, source, HitmanConfig.CopRingHours.Value, true);
            if (call != null && preset == null) OnScreen[call.Pointer] = text;
            return call;
        }

        // Fallback calls: the DDS tree is not heard, so the line is shown when the player picks up.
        private static readonly Dictionary<IntPtr, string> OnScreen = new();

        // The citizen who takes the calls to a fake number, by the greeting registered for that number.
        internal static void SetAnswerer(DialogPreset greeting, Func<Human> who)
        {
            if (greeting != null && greeting.name != null) Answerers[greeting.name] = who;
        }

        // The open conversation is over the phone (InteractionController.SetDialog's isRemote). The options of a phone
        // conversation and of a face-to-face one are separate DialogPresets (telephoneCallOption true/false): in the
        // 7th test the cop's conversation started remote and was rebuilt as a face-to-face one with him a second
        // later, which dropped the phone options. Each menu option exists in both forms and shows the one that fits.
        internal static bool DialogIsRemote() => InteractionController.Instance != null && InteractionController.Instance.isRemote;

        // The greeting of a call's source: identifies the mod's fake numbers.
        internal static string GreetingOf(TelephoneController.PhoneCall call)
        {
            var source = call != null ? call.source : null;
            var greeting = source != null ? source.dialogGreeting : null;
            return greeting != null ? greeting.name : null;
        }

        [HarmonyPatch(typeof(TelephoneController.PhoneCall), nameof(TelephoneController.PhoneCall.SetCallState))]
        internal static class PhoneCall_SetCallState
        {
            static void Prefix(TelephoneController.PhoneCall __instance, TelephoneController.CallState newState)
            {
                if (newState != TelephoneController.CallState.started || __instance == null || __instance.recevierNS != null) return;
                var greeting = Answerers.Count > 0 ? GreetingOf(__instance) : null;
                if (greeting == null || !Answerers.TryGetValue(greeting, out var who)) return;
                Human human = null;
                try { human = who(); }
                catch { }
                if (human == null || human.isDead) return;
                __instance.recevierNS = human;
                __instance.receiver = human.humanID;
                Plugin.Logger.LogInfo($"[Phone] {Dev.Describe.Who(human)} takes the player's call ({greeting}).");
            }

            static void Postfix(TelephoneController.PhoneCall __instance, TelephoneController.CallState newState)
            {
                if (OnScreen.Count == 0 || __instance == null || !OnScreen.TryGetValue(__instance.Pointer, out var text)) return;
                if (newState == TelephoneController.CallState.started) Text.Notify(text, text);
                else if (newState == TelephoneController.CallState.ended || newState == TelephoneController.CallState.denied) OnScreen.Remove(__instance.Pointer);
            }
        }
    }
}
