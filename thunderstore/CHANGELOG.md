# Changelog

## 1.2.6 — The Fixer, face to face

- **On the phone** the Fixer no longer does business: he tells you where he is right now (home, work or wherever, with the building) and asks you to come over. The place also shows on screen.
- **Face to face**, "Got any work for me?" hands the job over on the spot:
  - he names the target, their job and workplace, and where to slip the case;
  - the case opens already accepted, with the client's information and the 50% deposit;
  - there is no note to find and no number to call.
- The campaign's chapters are handed over the same way.

## 1.2.5

- Fixed: the cop's and the Fixer's options never showed on the phone. The game lists a citizen's options by evidence key: face to face it uses the face and the voice, on the phone only the voice. SOD.Common ties custom dialogs to the face, so they are now tied to the voice and show in both.
- The mod's dialog options now come first, above the game's own (interrogation, bribes, the cop, the Fixer).

## 1.2.4

- Fixed: on a call to the cop or the Fixer, their options showed for a moment and were replaced by the ordinary chat lines. The game rebuilds that conversation as a face-to-face one, which hides phone options. Every option of theirs now exists in both forms, and the one that fits the open conversation shows. This also makes the face-to-face menus from 1.2.2 actually appear.
- The cop takes an envelope whenever he is not standing next to you; face to face, you pay on the spot.
- Dev menu: teleports to the outside of the Fixer's and the cop's front doors.

## 1.2.3

- Every Hitman mission now has its own name followed by **[Hitman]** ("Pizza delivery [Hitman]", "High risk [Hitman]"…), on the case and in the job list.
- Dev menu: a **Hitman missions** window lists every active mission. Each one shows its target, client, pay, bribe verdict, note location, intro, meeting and progress, with buttons to:
  - go to the note;
  - accept it at once;
  - go to the client, the target or the meeting;
  - kill the target;
  - mark it fulfilled;
  - end it.
- Dev menu: a **Black markets** section and window covering all three underground businesses (black market trader, black market sync clinic, gambling den) and any other password-locked place. Nearest first, each one shows its password, place and who runs it (flagged when it is your Fixer), with buttons to:
  - teleport outside its door;
  - go to who runs it;
  - learn the password.

## 1.2.2

- **The corrupt cop's new packages**, from the cheapest to the whole file:
  1. **Lose the witness statements:** reported statements vanish, and witnesses who haven't talked yet never will.
  2. **Wipe the camera footage and the statements:** also every recording of you, including footage of bodies nobody has found yet.
  3. **Take your name off the investigation:** heat to zero, and nothing on file brings heat again.

  Each option shows how much heat it takes off and its price.
- **Talk to the cop and the Fixer face to face:** once you know them, their options also show up in person. With the cop you pay on the spot, with no envelope.
- Fixed: calling the cop's or the Fixer's number did nothing (and threw an error).
- Fixed: their entries in the notebook showed "<invalidstringaddress>". They now read "Corrupt Cop" / "Fixer".
- When the cop calls you after the first time, he reminds you of his number instead of reading the menu.
- Ordinary citizens no longer fight with guns. Only Enforcers, fighting jobs, the game's killers and hunters draw them (`[Arsenal] CiviliansUseGuns`).
- The pistols' default accuracy is now perfect (1.0), so the first shot lands where you aim.

## 1.2.1

- Fixed: the corrupt cop, the Fixer and the campaign calls were silent. Calls to the player now work the way the game's own calls do (the voice on the line speaks the greeting).
- Fixed: calling the cop's or the Fixer's number threw an error and nothing happened. The citizen behind the number now takes the call himself.
- Fixed: the cop's packages, the Fixer's options, the "hand over the Fixer" option and the interrogation answers never showed up. SOD.Common only offers custom dialogs listed as default options.
- Fixed: the Enforcers who come for the interrogation went into the apartment. They are now sent to the hallway outside your door.
- Fixed: salaries were read in thousands. Pay now really depends on the target's job, and well-paid targets can accept a bribe (the limit was Cr3 before).
- The cop's and the Fixer's numbers go into your notebook the first time they call, answered or not.

## 1.2.0 — The Last Contract

- **A story campaign**, told through phone calls:
  - Once you are a Professional, the Fixer calls about a big client.
  - Three jobs follow, each better paid (×1.5, on top of the Fixer bonus): a test, a witness, a target in uniform.
  - Then your police contact calls with bad news: the Fixer sold you out, and a killer is on the way.
- **The ending is your choice:**
  - kill the Fixer, and keep the client's money he was holding;
  - or call the cop and hand the Fixer over, and your heat is wiped clean.
- If a chapter's job falls through, ask the Fixer again. If the Fixer dies before the end, the story is over.
- A new Fixer takes over afterwards, as before.
- New `[Campaign]` config section and a dev menu section.

## 1.1.0 — Consequences

- **Traitor clients:** now and then (10%, never on your first job) the client betrays you at the hand-in. They either:
  - **keep the rest of the money**: it is yours again if the client dies, a contract of its own;
  - **call the police** on you: +15 heat, Enforcers come to where you are;
  - **send a killer** to tie up loose ends, hours later.
- **Revenge:** once a victim's body is found, someone close to them (partner, housemate, friend) may hire a killer. The chance doubles when the police have evidence. Deaths that pass for natural causes or suicide never bring revenge.
- **Scared targets:** a target who refuses a bribe and runs, or sees you kill someone, locks themselves at home for 12 game hours.
- New `[Consequences]` config section; dev menu section with forced betrayals, scare and revenge.

## 1.0.0 — The contract catalog

- **Four new assassination contracts:**

  | Contract | How it starts | Tier |
  |---|---|---|
  | Problem solver | The client names the target on the phone | Professional |
  | Quick job | A payphone rings at a set time with the details | Professional |
  | Broken heart | Meet a client whose partner is cheating on them | Veteran |
  | High risk | Meet the client; the target is an Enforcer, a guard or a debt collector | Legend |

- Each contract carries its own kill objective, so the contracts load in any order.
- The deposit and the "details updated" message now also work for contracts without a meeting.
- `[Career] ContractTiers` lists the real catalog. The 0.9.0 placeholder default is migrated.
- Dev menu: one "Post" button per contract.
- Fixed: the dev menu showed broken accents in Portuguese.

## 0.9.0 — Career and the Fixer

- **Reputation tiers** (Beginner, Professional, Veteran, Legend) earned through job ratings. Higher tiers unlock contracts and pay more.
- **The Fixer**, an underworld middleman with a real criminal job. He calls once you have a name, and on his number you can:
  - get exclusive, better-paid contracts;
  - check your record;
  - buy gear: syringe, poison, a suicide note, rope, a sniper rifle.
- **Intimidation:** a non-lethal job (a beating, no killing) that runs alongside the kill contracts, for less money.
- New `[Career]` and `[Fixer]` config sections.

## 0.8.0 — Arsenal and methods

- **The game's own firearms** (revolver, shotgun, sniper and battle rifles) fire like the lethal pistol. They are **loud**: everyone within earshot becomes a witness. Rifles zoom further, and weapons dealers sell them.
- **Blades** (knives, katana): a hit that knocks someone down kills.
- **Garrote** (rope, wire): one silent kill from behind.
- **Poison** (syringe, poison bottle) on someone asleep or knocked out: they die hours later of "natural causes", with almost no heat and no crime scene.
- **Forged suicide:** poison or garrote plus a suicide note in the victim's home passes for suicide.
- New `[Arsenal]` config section.

## 0.7.0 — Stealth and evidence

- **Heat comes from what the police find out:**
  - A death counts when the body is **found**, and a found body becomes a crime scene. Hidden bodies add nothing.
  - **Witnesses** go to the police 1–2 game hours later. Bribe them, put them to sleep with a dart, or deal with them before they talk.
  - **Cameras** only count if they were on and saw you, so switching security off first pays.
- **Job ratings** at the hand-in: Silent Assassin (+25%), Professional (+10%), Sloppy. Ratings build reputation.
- **Bribed targets leave town** and the client still pays. Later clients may find out and send a killer after you.
- **The interrogation** asks a third question when the police hold evidence against you.
- `[Stealth]` config section; `Stealth.Enabled = false` restores the old instant heat.

## 0.6.0

- Developer tools are now hidden and off: no hotkeys, test menu or log probes unless `devmode.txt` sits in the plugin folder (or `DevMode = true` is added to the cfg). Their settings moved to `sod.hitman.dev.cfg`.
- The client pays half up front at the meeting (`[Payment] DepositShare`); the rest comes with the hand-in.
- Also packaged for Nexus Mods (manual install archive with INSTALL.txt).
- README rewritten (English and Portuguese).

## 0.5.7 (test build)

- New in-game test menu (Insert, `[Dev] KeyMenu`), built with the UniverseLib that SOD.Common already ships. It has:
  - every dev hotkey;
  - teleports to the contract note, client, target, meeting point, corrupt cop and envelope payphone;
  - money, health, clear wanted, time speed and clock skips;
  - the game's own debug switches (invincible, invisible, inaudible, pause AI, NPCs ignore attacks, keys to the city), and ghost mode;
  - wake darted citizens, make everyone forget you, mark a contract fulfilled without killing, show the case questions.
- Mouse look, movement and the pistols are off while the menu is open.

## 0.5.6 (test build)

- Fixed: tranquilized citizens slept for days and kept the player hunted, with the "seen" eye on screen for good. The game counts a forced knock-out in game hours, and its damage reaction starts a pursuit even at zero damage. Darts now skip that reaction. The citizen sleeps 3 minutes of real, unpaused play, and forgets the player: no pursuit, combat or witness entry, before or after waking.
- Citizens left asleep for days by older darts wake up when the save loads.

## 0.5.5 (test build)

- Fixed: contracts could not be handed in. Accepting a job moves its resolve questions to the case, so the kill question was never marked done.
- Fixed: the corrupt cop said nothing on the phone and his packages did nothing. SOD.Common 2.0.7 throws on custom dialogs whose listener is not a citizen; the mod now settles its phone dialogs first. His call to the player is now a DDS conversation tree spoken over the line.
- Heat levels are shorter: 30 / 60 / 90 (were 50 / 100 / 160). Old config defaults are migrated.
- New: at heat level 2, an Enforcer comes to the player's home and asks two questions (where you were at the time of the last kill, whether you knew the victim). Credible answers lower heat; suspicious ones, walking out or never being home raise it. No search.
- Dev: F8 reports where the note was posted; F12 starts an interrogation visit.

## 0.5.4 (test build)

- Fixed: kills by the player never counted for heat or contracts. In this game build, Human.Murder without a murder case throws halfway through, leaving a victim with no death record and no body. Player kills now pass a blank murder case, which starts no investigation, so the death is recorded like any other. The game's save then keeps it.
- The lethal pistol no longer uses the game's own damage kill, which has the same problem.
- Corpses that the game restores on load no longer add heat again.
- Dev hotkey F8 now really posts a contract: it adds one board slot through the game's frequency curve for the rest of the session.
- Posted, not yet accepted contracts on a dead target leave the board as vanilla jobs do. Accepted contracts are still protected.

## 0.5.3 (test build)

- Fixed a crash at game start: two Harmony patches targeted methods that IL2CPP merges with thousands of other methods (SideJob.OnGooseChaseCallTriggered, RevengeObjective.ManualTrigger). Both removed; packaging now refuses any patch on a merged method.

## 0.5.2 (test build)

- Requires SOD.Common 2.0.7: newer SOD.Common releases (2.1.3+) patch a mod menu type this game build does not have and fail to load, which made Hitman Contracts skip loading.
- Per-save data uses the save-store API available in every SOD.Common version.
- The job posting DDS tree now names its starting message (silences a DDSLoader warning).

## 0.5.1 (test build)

- Development hotkeys moved off keys the game uses (F5, F7): dump F2, both pistols F3, corrupt cop calls F4.

## 0.5.0 (test build)

- The TranquilizerPistol mod is now part of Hitman Contracts, with a lethal pistol added: two silenced pistols the player can fire (aim with Secondary, fire with Primary).
- Lethal: the game's silenced Lazarus 5. A hit that knocks out, empties health or reaches the configured hits kills, through the game's own death routine, so contracts and heat count it.
- Tranquilizer: a new item, "Lazarus 5 Tranquilizer". Darts put the target to sleep and never kill.
- Infinite ammo, no reload (the game has no reload system).
- Both pistols are sold by weapons dealers and black market traders.
- Lethal shots seen or heard are illegal before they land, so witnesses count toward heat. Heat also counts citizens nearby who can see the player.
- Citizens shot dead are killed again on load, in case the save brings them back.
- Not compatible with the original "Guns" mod.

## 0.4.1 (test build)

- Heat counts each victim once, even if a weapon mod calls the game's death routine again when a save loads.
- The vanilla game cannot kill citizens (only knock them out); contracts need a lethal weapon mod.

## 0.4.0 (test build)

- Corrupt cop: a real enforcer, picked when the save starts, who makes paperwork disappear for money.
- He calls first when heat reaches level 2: your apartment phone, then a payphone near you if nobody answers, again every 6 game hours until you pick up. His number goes into your notebook.
- Call him to buy a package: one death (10 points), half your heat, or all of it (20% off). Price per point is half the average contract per death, doubled while there's a warrant.
- He names a payphone and a deadline; walk up to it with the money and the envelope is left, the money taken and the heat lowered.
- If he dies, another enforcer takes over 3 game days later and calls to introduce himself.

## 0.3.0 (test build)

- Heat: every death the player causes adds points (contract 10, other 20, +15 per witness, +10 with cameras, +5 in public). It never goes down on its own.
- Level 1 (50): the next newspaper runs a front-page story about a contract killer.
- Level 2 (100): enforcers are sent to the player's home now and then; a new story runs.
- Level 3 (160): wanted in every building entered, hired killers come after the player, a warrant story runs.
- Heat is saved with the game. The corrupt cop who lowers it comes in a later version.

## 0.2.0 (test build)

- Bribe: while a contract is active, a new dialog option reveals the contract and asks for double the payment. The target accepts when 2 x payment <= 40% of their salary / 12; jobless targets always refuse.
- Accepted: the target pays at once and the contract closes without the client's payment.
- Refused: targets in fighting jobs (enforcers, guards, debt collectors, weapons dealers, gambling den owners, hired killers) attack; everyone else runs away screaming for the police.
- The option shows up with anyone while a contract is active, so it does not give away who the target is; others just shrug. One attempt per contract.

## 0.1.0 (test build)

- First contract, "Freelancer for pizza delivery": phone call, meeting with the client at a set time (15 min tolerance), target named in conversation with their workplace, kill only, hand-in under the client's door.
- Payment by the target's job: (base + salary / 120) x risk x random, configurable.
- The game no longer cancels a contract when its target dies; a dead client still ends it.
- On-screen messages for the meeting time, updated job details, fulfilled and failed contracts.
- Development tools: preset/citizen/job dump, side job and death logging, hotkeys.
