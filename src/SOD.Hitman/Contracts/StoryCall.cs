using System;
using System.Collections.Generic;
using HarmonyLib;
using SOD.Common;

namespace SODHitman.Contracts
{
    // A citizen calls the player's apartment and says one line: the campaign's story beats (v1.2). Same mechanism as
    // the cop's and the Fixer's calls: the shared DDS tree in content/Heat, whose only block gets the line just before.
    internal static class StoryCall
    {
        private sealed class Ringing
        {
            public TelephoneController.PhoneCall Call;
            public bool Answered;
            public Action OnAnswered, OnMissed;
        }

        private static readonly List<Ringing> Calls = new();

        internal static void ResetSession() => Calls.Clear();

        internal static bool Busy => Calls.Count > 0;

        internal static bool Start(Human caller, string line, Action onAnswered, Action onMissed = null)
        {
            var player = Player.Instance;
            if (caller == null || caller.isDead || player == null || TelephoneController.Instance == null) return false;
            var to = Heat.CorruptCop.HomePhone(player);
            if (to == null) return false;
            var from = Heat.CorruptCop.CopPhone(caller, to);
            if (from == null) return false;
            var call = PhoneCalls.CallPlayer(from, to, line);
            if (call == null) return false;
            Calls.Add(new Ringing { Call = call, OnAnswered = onAnswered, OnMissed = onMissed });
            Plugin.Logger.LogInfo($"[Story] {Dev.Describe.Who(caller)} calling the player at home {to.numberString}");
            return true;
        }

        private static void Safe(Action action)
        {
            try { action?.Invoke(); }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Story] Call callback failed: {e.Message}"); }
        }

        [HarmonyPatch(typeof(TelephoneController.PhoneCall), nameof(TelephoneController.PhoneCall.SetCallState))]
        internal static class PhoneCall_SetCallState
        {
            static void Postfix(TelephoneController.PhoneCall __instance, TelephoneController.CallState newState)
            {
                if (Calls.Count == 0 || __instance == null) return;
                var ringing = Calls.Find(c => c.Call != null && c.Call.Pointer == __instance.Pointer);
                if (ringing == null) return;
                Plugin.Logger.LogInfo($"[Story] Call state -> {newState}");
                bool byPlayer = Player.Instance != null && __instance.receiver == Player.Instance.humanID;
                if (newState == TelephoneController.CallState.started && !ringing.Answered)
                {
                    ringing.Answered = true;
                    Safe(ringing.OnAnswered);
                }
                else if (newState == TelephoneController.CallState.ended || newState == TelephoneController.CallState.denied)
                {
                    Calls.Remove(ringing);
                    if (ringing.Answered) return;
                    if (byPlayer) Safe(ringing.OnAnswered);
                    else Safe(ringing.OnMissed);
                }
            }
        }
    }
}
