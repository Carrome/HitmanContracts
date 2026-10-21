// For each "Type::Method" given, prints the native address of every overload and how many game methods share it.
// IL2CPP folds methods with identical machine code (empty methods, trivial getters...) into one function: a Harmony
// patch on a folded method runs for ALL of them, with the wrong 'this', and crashes the game. Only patch methods
// whose address is not shared.
//
// usage: foldcheck <game folder> Type::Method [Outer+Nested::Method] ...
using LibCpp2IL;
using LibCpp2IL.Metadata;

if (args.Length < 2)
{
    Console.WriteLine("usage: foldcheck <game folder> Type::Method ...");
    return 2;
}

var gameDir = args[0];
var dataDir = Directory.GetDirectories(gameDir, "*_Data").First();
var version = LibCpp2IlMain.DetermineUnityVersion(Path.Combine(gameDir, "UnityPlayer.dll"), dataDir);
Console.WriteLine($"Unity {version}, loading GameAssembly...");
if (!LibCpp2IlMain.LoadFromFile(Path.Combine(gameDir, "GameAssembly.dll"),
        Path.Combine(dataDir, "il2cpp_data", "Metadata", "global-metadata.dat"), version))
{
    Console.WriteLine("LibCpp2IL could not load the game.");
    return 2;
}

var types = LibCpp2IlMain.TheMetadata!.typeDefs;
int unsafeCount = 0;
foreach (var target in args.Skip(1))
{
    var parts = target.Split("::");
    var typePath = parts[0].Split('+');
    var methodName = parts[1];

    var type = types.FirstOrDefault(t => Matches(t, typePath));
    if (type == null)
    {
        Console.WriteLine($"?? {target}: type not found");
        continue;
    }
    var overloads = (type.Methods ?? Array.Empty<Il2CppMethodDefinition>()).Where(m => m.Name == methodName).ToList();
    if (overloads.Count == 0) Console.WriteLine($"?? {target}: method not found");
    foreach (var m in overloads)
    {
        var ps = m.Parameters;
        var sig = ps == null ? "" : string.Join(", ", ps.Select(p => p.Type.ToString()));
        var ptr = m.MethodPointer;
        int shared = ptr != 0 && LibCpp2IlMain.MethodsByPtr.TryGetValue(ptr, out var list) ? list.Count : 0;
        var verdict = ptr == 0 ? "NO CODE (abstract/extern)" : shared > 1 ? "FOLDED - DO NOT PATCH" : "ok";
        if (ptr == 0 || shared > 1) unsafeCount++;
        Console.WriteLine($"{verdict,-24} {target}({sig}) @0x{ptr:X} shared by {shared}");
        if (shared > 1)
            foreach (var other in LibCpp2IlMain.MethodsByPtr[ptr].Take(6))
                Console.WriteLine($"      also: {other.DeclaringType?.FullName}::{other.Name}");
    }
}
return unsafeCount == 0 ? 0 : 1;

static bool Matches(Il2CppTypeDefinition t, string[] path)
{
    if (t.Name != path[^1]) return false;
    var outer = t.DeclaringType;
    for (int i = path.Length - 2; i >= 0; i--)
    {
        if (outer == null || outer.Name != path[i]) return false;
        outer = outer.DeclaringType;
    }
    return path.Length > 1 || t.DeclaringType == null;
}
