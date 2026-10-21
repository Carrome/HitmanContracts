using System;
using SOD.Common;

namespace SODHitman
{
    // In-code strings follow the game language (BrazilianPortuguese or anything else -> English).
    internal static class Text
    {
        internal static bool IsPortuguese
        {
            get
            {
                var lang = Game.Instance != null ? Game.Instance.language : null;
                return lang != null && lang.Contains("Portuguese");
            }
        }

        internal static string L(string pt, string en) => IsPortuguese ? pt : en;

        internal static void Notify(string pt, string en)
        {
            Plugin.Logger.LogInfo("[Notify] " + en);
            if (!HitmanConfig.ShowNotifications.Value) return;
            try { Lib.GameMessage.Broadcast(L(pt, en)); }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Notify] Broadcast failed: {e.Message}"); }
        }

        // 24h clock for an absolute game time (SessionData.gameTime, in hours).
        internal static string Clock(float gameTime)
        {
            try { return SessionData.Instance.GameTimeToClock24String(gameTime, false); }
            catch
            {
                var hourOfDay = ((gameTime % 24f) + 24f) % 24f;
                int h = (int)hourOfDay, m = (int)Math.Round((hourOfDay - h) * 60f);
                if (m == 60) { h = (h + 1) % 24; m = 0; }
                return $"{h:00}:{m:00}";
            }
        }

        internal static float Now => SessionData.Instance != null ? SessionData.Instance.gameTime : 0f;
    }
}
