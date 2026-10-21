using System.Collections.Generic;
using UnityEngine;
using Il2CppGeneric = Il2CppSystem.Collections.Generic;

namespace SODHitman.Dev
{
    // Null-safe, log-friendly views of game objects, shared by the dump, the probes and the hotkeys.
    internal static class Describe
    {
        internal static string N(Object o) => o == null ? null : o.name;

        internal static string Who(Human h)
        {
            if (h == null) return "null";
            if (h.isPlayer) return "PLAYER";
            return $"{h.citizenName}#{h.humanID}";
        }

        internal static string Job(SideJob j) =>
            j == null ? "null" : $"#{j.jobID} {N(j.preset)} state={j.state} accepted={j.accepted} failed={j.failed} phase={j.phase} poster={Who(j.poster)} purp={Who(j.purp)}";

        internal static string Where(Interactable i)
        {
            if (i == null) return null;
            string name;
            try { name = i.GetName(); } catch { name = "?"; }
            var node = i.node;
            var loc = node != null ? N(node.gameLocation) : null;
            var building = node != null ? N(node.building) : null;
            return $"{name} [{N(i.preset)}] @ {loc} / {building} {i.wPos}";
        }

        // Short place name for on-screen messages: "location (building)".
        internal static string Place(Interactable i)
        {
            var node = i != null ? i.node : null;
            if (node == null) return "?";
            var loc = N(node.gameLocation);
            var building = N(node.building);
            return building != null && building != loc ? $"{loc} ({building})" : loc ?? building ?? "?";
        }

        internal static IEnumerable<T> Each<T>(Il2CppGeneric.List<T> list)
        {
            if (list == null) yield break;
            for (int i = 0; i < list.Count; i++) yield return list[i];
        }

        internal static List<string> Names<T>(Il2CppGeneric.List<T> list) where T : Object
        {
            var result = new List<string>();
            foreach (var o in Each(list)) result.Add(N(o));
            return result;
        }

        internal static List<string> Strings<T>(Il2CppGeneric.List<T> list)
        {
            var result = new List<string>();
            foreach (var o in Each(list)) result.Add(o == null ? null : o.ToString());
            return result;
        }

        internal static object V2(Vector2 v) => new { v.x, v.y };
    }
}
