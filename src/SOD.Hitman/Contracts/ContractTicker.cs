using System;

namespace SODHitman.Contracts
{
    // Runs once per game minute (SOD.Common Lib.Time.OnMinuteChanged).
    internal static class ContractTicker
    {
        private static int errors;

        internal static void Tick()
        {
            if (SideJobController.Instance == null || SessionData.Instance == null || !SessionData.Instance.startedGame) return;
            try
            {
                KillTracker.Sweep();
                MeetingClock.Tick();
                foreach (var job in ContractRegistry.ActiveHitmanJobs())
                    Payment.Apply(job, "tick");
                Heat.HeatSystem.Tick();
                Heat.CorruptCop.Tick();
                Bribe.Tick(Text.Now);
                Weapons.Arsenal.Tick(Text.Now);
                Fixer.Tick();
                Consequences.Tick(Text.Now);
                Campaign.Tick(Text.Now);
            }
            catch (Exception e)
            {
                if (errors++ < 5) Plugin.Logger.LogError($"[Tick] {e}");
            }
        }
    }
}
