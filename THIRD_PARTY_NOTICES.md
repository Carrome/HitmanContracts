# Third-party notices

Hitman Contracts reuses and adapts code from two MIT-licensed Shadows of Doubt mods, through the author's own
TranquilizerPistol mod, which is now part of Hitman Contracts (`src/SOD.Hitman/Weapons`).
Their copyright notices and license text are reproduced below, as the MIT License requires.

## Guns — https://github.com/ReveredRedHood/SpyraxiShadowsOfDoubtMods

Reused/adapted: aiming and firing flow, camera aim zoom, Attack→Fire and Throw→Aim prompt swaps, weapon state
enums, the draw-bark suppression (`Weapons/WeaponController.cs`, `Weapons/WeaponPatches.cs`, `Weapons/WeaponStates.cs`).

```
MIT License

Copyright (c) 2023 Spyraxi (ReveredRedHood)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## SODLethalActionReborn / SOD.Common — https://github.com/Ven0maus/SOD.Common

Adapted: the `Citizen.RecieveDamage` hit-counter pattern, the `NewAIController.SetKO` postfix that clears the
counter when a citizen wakes up, and the kill sequence (`SetHealth(0)` + `Human.Murder` + dead-citizen list +
murderer/victim re-pick, and re-killing on load) (`Weapons/WeaponPatches.cs`, `Weapons/WeaponKills.cs`).

```
MIT License

Copyright (c) 2023 Venomaus

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
