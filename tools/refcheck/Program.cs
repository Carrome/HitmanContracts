// Lists every type and member a plugin DLL references from the game's Assembly-CSharp and reports the ones
// the given interop Assembly-CSharp.dll does not define (e.g. types added by a newer game version).
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

if (args.Length < 2)
{
    Console.WriteLine("usage: refcheck <interop Assembly-CSharp.dll> <plugin.dll>...");
    return 2;
}

var game = LoadGame(args[0]);
int totalMissing = 0;
foreach (var plugin in args.Skip(1))
{
    var missing = Check(plugin, game, out int typeCount, out int memberCount);
    Console.WriteLine($"== {plugin}");
    Console.WriteLine($"   checked {typeCount} game types, {memberCount} game members; missing {missing.Count}");
    foreach (var m in missing.Distinct().OrderBy(x => x)) Console.WriteLine("   MISSING " + m);
    totalMissing += missing.Count;
}
return totalMissing == 0 ? 0 : 1;

static Dictionary<string, HashSet<string>> LoadGame(string path)
{
    using var pe = new PEReader(File.OpenRead(path));
    var md = pe.GetMetadataReader();
    var result = new Dictionary<string, HashSet<string>>();
    foreach (var h in md.TypeDefinitions)
    {
        var td = md.GetTypeDefinition(h);
        var members = new HashSet<string>();
        foreach (var m in td.GetMethods()) members.Add(md.GetString(md.GetMethodDefinition(m).Name));
        foreach (var f in td.GetFields()) members.Add(md.GetString(md.GetFieldDefinition(f).Name));
        result[FullName(md, h)] = members;
    }
    return result;
}

static string FullName(MetadataReader md, TypeDefinitionHandle h)
{
    var td = md.GetTypeDefinition(h);
    var name = md.GetString(td.Name);
    var declaring = td.GetDeclaringType();
    if (!declaring.IsNil) return FullName(md, declaring) + "/" + name;
    var ns = md.GetString(td.Namespace);
    return ns.Length > 0 ? ns + "." + name : name;
}

// Full name of a type reference, or null when it does not come from Assembly-CSharp.
static string RefName(MetadataReader md, TypeReferenceHandle h)
{
    var tr = md.GetTypeReference(h);
    var name = md.GetString(tr.Name);
    var scope = tr.ResolutionScope;
    if (scope.Kind == HandleKind.TypeReference)
    {
        var outer = RefName(md, (TypeReferenceHandle)scope);
        return outer == null ? null : outer + "/" + name;
    }
    if (scope.Kind != HandleKind.AssemblyReference) return null;
    var asm = md.GetString(md.GetAssemblyReference((AssemblyReferenceHandle)scope).Name);
    if (asm != "Assembly-CSharp") return null;
    var ns = md.GetString(tr.Namespace);
    return ns.Length > 0 ? ns + "." + name : name;
}

static List<string> Check(string path, Dictionary<string, HashSet<string>> game, out int typeCount, out int memberCount)
{
    using var pe = new PEReader(File.OpenRead(path));
    var md = pe.GetMetadataReader();
    var missing = new List<string>();
    typeCount = 0;
    memberCount = 0;

    foreach (var h in md.TypeReferences)
    {
        var name = RefName(md, h);
        if (name == null) continue;
        typeCount++;
        if (!game.ContainsKey(name)) missing.Add("type " + name);
    }

    foreach (var h in md.MemberReferences)
    {
        var mr = md.GetMemberReference(h);
        if (mr.Parent.Kind != HandleKind.TypeReference) continue;
        var owner = RefName(md, (TypeReferenceHandle)mr.Parent);
        if (owner == null) continue;
        memberCount++;
        var member = md.GetString(mr.Name);
        if (game.TryGetValue(owner, out var members) && !members.Contains(member))
            missing.Add($"member {owner}::{member}");
    }
    return missing;
}
