using System;
using System.Runtime.InteropServices;

namespace SODHitman
{
    // Keyboard state read directly from Windows, independent of Rewired and Unity (from SOD.InputSoftlockFix).
    internal static class Native
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private static readonly uint OwnPid = (uint)Environment.ProcessId;

        public static bool IsDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

        public static bool CtrlDown => IsDown(0x11) || IsDown(0xA2) || IsDown(0xA3);
        public static bool ShiftDown => IsDown(0x10) || IsDown(0xA0) || IsDown(0xA1);
        public static bool AltDown => IsDown(0x12) || IsDown(0xA4) || IsDown(0xA5);

        public static bool IsGameForeground()
        {
            var hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return false;
            GetWindowThreadProcessId(hwnd, out var pid);
            return pid == OwnPid;
        }
    }

    internal sealed class Hotkey
    {
        public readonly bool Ctrl, Shift, Alt;
        public readonly int Vk;
        public readonly string Text;

        private Hotkey(bool ctrl, bool shift, bool alt, int vk, string text)
        {
            Ctrl = ctrl; Shift = shift; Alt = alt; Vk = vk; Text = text;
        }

        // Requested modifiers must be held; the others are ignored
        // (requiring them to be released made the hotkey fail with "stuck" keys or AltGr).
        public bool IsPressed() =>
            Native.IsDown(Vk) && (!Ctrl || Native.CtrlDown) && (!Shift || Native.ShiftDown) && (!Alt || Native.AltDown);

        // Format: [Ctrl+][Shift+][Alt+]Key. Returns null when the key is not recognised.
        public static Hotkey Parse(string s)
        {
            bool ctrl = false, shift = false, alt = false;
            int vk = 0;
            foreach (var raw in (s ?? "").Split('+'))
            {
                var p = raw.Trim().ToUpperInvariant();
                if (p.Length == 0) continue;
                switch (p)
                {
                    case "CTRL": case "CONTROL": ctrl = true; continue;
                    case "SHIFT": shift = true; continue;
                    case "ALT": alt = true; continue;
                }
                vk = KeyToVk(p);
            }
            return vk == 0 ? null : new Hotkey(ctrl, shift, alt, vk, s.Trim());
        }

        private static int KeyToVk(string p)
        {
            if (p.Length == 1 && ((p[0] >= 'A' && p[0] <= 'Z') || (p[0] >= '0' && p[0] <= '9'))) return p[0];
            if (p.Length >= 2 && p[0] == 'F' && int.TryParse(p.Substring(1), out var f) && f >= 1 && f <= 24) return 0x70 + f - 1;
            return p switch
            {
                "HOME" => 0x24,
                "END" => 0x23,
                "INSERT" => 0x2D,
                "DELETE" => 0x2E,
                "PAGEUP" => 0x21,
                "PAGEDOWN" => 0x22,
                "PAUSE" => 0x13,
                "BACKSPACE" => 0x08,
                _ => 0
            };
        }
    }
}
