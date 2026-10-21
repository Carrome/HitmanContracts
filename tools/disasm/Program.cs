// Disassembles game methods (x64) with calls, string literals and metadata globals resolved, so a crash inside a game
// method can be traced to the exact field it reads. Also prints instance field offsets of the given types.
//
// usage: disasm <game folder> [Type::Method | Outer+Nested::Method | fields:Type] ...
using Iced.Intel;
using LibCpp2IL;
using LibCpp2IL.Metadata;

if (args.Length < 2)
{
    Console.WriteLine("usage: disasm <game folder> Type::Method | fields:Type ...");
    return 2;
}

// Offset of the vtable inside Il2CppClass for this Unity/IL2CPP version (x64), seen in the game's own virtual calls.
const ulong VtableBase = 0x138;

var gameDir = args[0];
var dataDir = Directory.GetDirectories(gameDir, "*_Data").First();
var version = LibCpp2IlMain.DetermineUnityVersion(Path.Combine(gameDir, "UnityPlayer.dll"), dataDir);
if (!LibCpp2IlMain.LoadFromFile(Path.Combine(gameDir, "GameAssembly.dll"),
        Path.Combine(dataDir, "il2cpp_data", "Metadata", "global-metadata.dat"), version))
{
    Console.WriteLine("LibCpp2IL could not load the game.");
    return 2;
}

var bin = LibCpp2IlMain.Binary!;
var raw = bin.GetRawBinaryContent();
var starts = LibCpp2IlMain.MethodsByPtr.Keys.Where(k => k != 0).OrderBy(k => k).ToArray();
var types = LibCpp2IlMain.TheMetadata!.typeDefs;

foreach (var target in args.Skip(1))
{
    if (target.StartsWith("fields:"))
    {
        var t = Find(target[7..].Split('+'));
        if (t == null) { Console.WriteLine($"?? {target}"); continue; }
        Console.WriteLine($"== fields of {t.FullName} (valueType={t.IsValueType})");
        foreach (var f in t.FieldInfos ?? Array.Empty<LibCpp2IL.Reflection.Il2CppFieldReflectionData>())
            if ((f.Attributes & System.Reflection.FieldAttributes.Static) == 0)
                Console.WriteLine($"  0x{f.FieldOffset:X3}  {f.Field.Name} : {f.Field.FieldType}");
        continue;
    }

    if (target.StartsWith("xref:"))
    {
        // Every game method that calls (or jumps to) the given method.
        var xp = target[5..].Split("::");
        var xt = Find(xp[0].Split('+'));
        var wanted = (xt?.Methods ?? Array.Empty<Il2CppMethodDefinition>()).Where(m => m.Name == xp[1] && m.MethodPointer != 0)
            .Select(m => m.MethodPointer).ToHashSet();
        if (wanted.Count == 0) { Console.WriteLine($"?? {target}"); continue; }
        // Virtual methods are also called through the vtable: call [klass + VtableBase + slot*16]. Those sites can be
        // any class's method in the same slot, so they are listed apart as candidates.
        var vdisp = (xt?.Methods ?? Array.Empty<Il2CppMethodDefinition>()).Where(m => m.Name == xp[1] && m.slot != 0xFFFF)
            .Select(m => VtableBase + m.slot * 16UL).ToHashSet();
        Console.WriteLine($"== callers of {target}" + (vdisp.Count > 0 ? $" (virtual: vtable displacement {string.Join(",", vdisp.Select(d => $"0x{d:X}"))})" : ""));
        for (int i = 0; i < starts.Length; i++)
        {
            var s = starts[i];
            var e = i + 1 < starts.Length ? starts[i + 1] : s + 0x1000;
            if (e - s > 0x20000 || !bin.TryMapVirtualAddressToRaw(s, out var o)) continue;
            var buf = new byte[e - s];
            Array.Copy(raw, o, buf, 0, buf.Length);
            var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(buf));
            dec.IP = s;
            while (dec.IP < e)
            {
                dec.Decode(out var ins);
                bool direct = ins.FlowControl is FlowControl.Call or FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64
                    && wanted.Contains(ins.NearBranchTarget);
                bool viaVtable = ins.FlowControl is FlowControl.IndirectCall && ins.Op0Kind == OpKind.Memory
                    && ins.MemoryIndex == Register.None && ins.MemoryBase != Register.RIP && vdisp.Contains(ins.MemoryDisplacement64);
                if (direct || viaVtable)
                {
                    var who = LibCpp2IlMain.MethodsByPtr[s];
                    Console.WriteLine($"  {ins.IP:X} {(direct ? "call  " : "vcall?")} in {who[0].DeclaringType?.FullName}::{who[0].Name}" + (who.Count > 1 ? $" (+{who.Count - 1} folded)" : ""));
                }
            }
        }
        continue;
    }

    var parts = target.Split("::");
    var type = Find(parts[0].Split('+'));
    if (type == null) { Console.WriteLine($"?? {target}: type not found"); continue; }
    foreach (var m in (type.Methods ?? Array.Empty<Il2CppMethodDefinition>()).Where(m => m.Name == parts[1]))
    {
        var start = m.MethodPointer;
        var sig = string.Join(", ", (m.Parameters ?? Array.Empty<LibCpp2IL.Reflection.Il2CppParameterReflectionData>()).Select(p => $"{p.Type} {p.ParameterName}"));
        Console.WriteLine($"== {type.FullName}::{m.Name}({sig}) @0x{start:X}");
        if (start == 0) continue;
        var idx = Array.BinarySearch(starts, start);
        var end = idx >= 0 && idx + 1 < starts.Length ? starts[idx + 1] : start + 0x2000;
        if (end - start > 0x40000) end = start + 0x40000;
        var off = bin.MapVirtualAddressToRaw(start);
        var code = new byte[end - start];
        Array.Copy(raw, off, code, 0, code.Length);

        var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(code));
        decoder.IP = start;
        var fmt = new IntelFormatter();
        fmt.Options.FirstOperandCharIndex = 8;
        var output = new StringOutput();
        while (decoder.IP < end)
        {
            decoder.Decode(out var ins);
            fmt.Format(ins, output);
            var text = output.ToStringAndReset();
            var note = "";
            if (ins.FlowControl is FlowControl.Call or FlowControl.UnconditionalBranch && ins.Op0Kind == OpKind.NearBranch64)
                note = Name(ins.NearBranchTarget);
            else if (ins.IsIPRelativeMemoryOperand)
                note = Global(ins.IPRelativeMemoryAddress);
            Console.WriteLine($"  {ins.IP:X}  {text,-60} {note}");
            if (ins.Mnemonic == Mnemonic.Int3) break;
        }
    }
}
return 0;


string Name(ulong addr)
{
    if (LibCpp2IlMain.MethodsByPtr.TryGetValue(addr, out var list) && list.Count > 0)
        return $"-> {list[0].DeclaringType?.FullName}::{list[0].Name}" + (list.Count > 1 ? $" (+{list.Count - 1} folded)" : "");
    if (bin.TryGetExportedFunctionName(addr, out var exp)) return "-> " + exp;
    return "";
}

string Global(ulong addr)
{
    try
    {
        var lit = LibCpp2IlMain.GetLiteralByAddress(addr);
        if (lit != null) return $"\"{lit}\"";
        var g = LibCpp2IlMain.GetAnyGlobalByAddress(addr);
        if (g != null) return $"[{g.Type}] {g}";
    }
    catch { }
    return "";
}

Il2CppTypeDefinition? Find(string[] path) => types.FirstOrDefault(t =>
{
    if (t.Name != path[^1]) return false;
    var outer = t.DeclaringType;
    for (int i = path.Length - 2; i >= 0; i--)
    {
        if (outer == null || outer.Name != path[i]) return false;
        outer = outer.DeclaringType;
    }
    return path.Length > 1 || t.DeclaringType == null;
});
