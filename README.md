# Hitman Contracts

A mod for **Shadows of Doubt** (BepInEx 6 IL2CPP) that turns you into a contract killer. Assassination contracts show up on the side job boards next to the normal jobs.

The mod adds:
- **Heat from evidence:** bodies found, witnesses who talk, cameras that saw you.
- **Interrogations** at your door.
- **A corrupt cop** who makes paperwork disappear for a price.
- **A Fixer** who hands out better work as your reputation grows.
- **A full arsenal.**
- **Consequences**, and a short story campaign, "The Last Contract".

The player-facing description, with the full feature list and configuration, is in [thunderstore/README.md](thunderstore/README.md). The version history is in [thunderstore/CHANGELOG.md](thunderstore/CHANGELOG.md).

## Requirements

- The September 2024 build of the game.
- BepInEx 6 IL2CPP (be.755+).
- **SOD.Common 2.0.7.** Newer releases do not load on this game build.
- Asset Bundle Loader, DDSLoader and CommunityCaseLoader (Piepieonline).

## Building

```powershell
dotnet build src/SOD.Hitman -c Release -p:GameDir="E:\Shadows of Doubt"
```

`GameDir` points at the game folder: the build references its BepInEx interop assemblies and copies the plugin and `content/` into `BepInEx/plugins/SOD.Hitman`.

```powershell
powershell -ExecutionPolicy Bypass -File pack.ps1 -GameDir "E:\Shadows of Doubt"
```

`pack.ps1` builds the release packages into `dist/`, one for Thunderstore and one zip for manual installs (Nexus). Before packing, it runs `tools/foldcheck`, which refuses any Harmony patch on a method that IL2CPP folded together with other methods. Such a patch runs for every method sharing that machine code, and crashes the game.

## Layout

| Path | What it holds |
|---|---|
| `src/SOD.Hitman` | The plugin: contracts, heat, the cop, the Fixer, weapons, the campaign, the dev menu |
| `content/` | Game data loaded by CommunityCaseLoader and DDSLoader: job presets, dialogs, DDS text (EN + PT-BR) |
| `tools/disasm` | Disassembles game methods from `GameAssembly.dll`, with calls, strings and field offsets resolved |
| `tools/foldcheck` | Finds IL2CPP-folded methods |
| `tools/refcheck` | Lists game types and members a plugin DLL references that this game build lacks |
| `thunderstore/`, `nexus/` | Package metadata |
| `*.md` (root) | Design spec and test docs (Portuguese) |

## Developer mode

Hotkeys, an in-game test menu (Insert) and detailed logging are off by default. To turn them on, put an empty `devmode.txt` next to the plugin DLL.

## Credits

- Built on SOD.Common (Venomaus) and Piepieonline's Asset Bundle Loader, DDSLoader and CommunityCaseLoader.
- The pistols adapt MIT-licensed code from **Guns** (Spyraxi / ReveredRedHood) and **SODLethalActionReborn** (Venomaus). See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
