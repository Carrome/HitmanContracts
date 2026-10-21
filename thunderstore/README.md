# Hitman Contracts

Become a contract killer in Shadows of Doubt.

Contracts on the job boards, a career, a Fixer, stealth and evidence, a full arsenal, consequences and a story campaign, all built on the game's own systems.

Assassination contracts show up on the side job boards next to the normal jobs. Call the number, get the target, do the job your way, and hand in the case. The police only know what they find out: a body found, a witness who talks, a camera that was on. Too much heat brings the newspapers, an Enforcer at your door with questions, a warrant and hired hunters. A corrupt cop makes paperwork disappear for a price, and a Fixer brings better work as your name grows.

*Português abaixo.*

## Features

### Contracts

Five assassination contracts, each with its own way in, plus one non-lethal job:

| Contract | How it starts | Tier |
|---|---|---|
| **Pizza delivery** | Call the note; meet the client at a set time and place | Beginner |
| **Intimidation** (non-lethal) | Meet the client; beat the target up, no killing | Beginner |
| **Problem solver** | Call the note; the client names the target on the phone | Professional |
| **Quick job** | Call the note; a payphone rings at a set time with the details | Professional |
| **Broken heart** | Meet a client whose partner is cheating on them | Veteran |
| **High risk** | Meet the client; the target is an Enforcer, a guard or a debt collector | Legend |

- **Only the target's death counts.** Knocked out or tranquilized does not.
- **Half the payment comes up front** once you have the details. The rest comes at the hand-in, under the client's door.
- **Pay depends on the target's job:** dangerous jobs pay more. Your career tier adds on top.

### Stealth and evidence

- **A death counts for heat only once the body is found.** A found body becomes a crime scene. Hidden bodies add nothing.
- **Witnesses go to the police 1–2 game hours later.** Before they do, you can:
  - bribe them ("You didn't see anything", Cr200);
  - put them to sleep with a dart;
  - deal with them some other way.
- **Cameras only count if they were on and saw you.** Switch security off first.
- **Every job is rated at the hand-in:**
  - **Silent Assassin:** only the target, no witness talked, no camera, body never found. Pays +25%.
  - **Professional:** pays +10%.
  - **Sloppy.**

### Arsenal and methods

- **Two silenced pistols:**
  - **lethal:** the game's Lazarus 5;
  - **tranquilizer:** the target sleeps 3 real minutes and forgets who shot them.

  Aim with Secondary, fire with Primary. Infinite ammo.
- **The game's firearms** (revolver, shotgun, sniper and battle rifles) kill too, but they are **loud**: everyone within earshot becomes a witness. Rifles zoom further.
- **Blades** (knives, katana): a hit that knocks someone down kills.
- **Garrote** (rope, wire): one silent kill from behind.
- **Poison** (syringe, poison bottle) on someone asleep or knocked out: they die hours later of "natural causes", with almost no heat.
- **Forged suicide:** poison or garrote, plus a suicide note in the victim's home.

### Career and the Fixer

- **Reputation tiers:** Beginner → Professional → Veteran → Legend.
  - Ratings build reputation: Silent Assassin +3, Professional +2, Sloppy +1.
  - Higher tiers unlock contracts and pay more (up to ×2).
- **The Fixer**, an underworld middleman, calls once you have a name. On his number you can:
  - get exclusive, better-paid contracts;
  - check your record;
  - buy gear: syringe, poison, a suicide note, rope, a sniper rifle.

### The Last Contract (campaign)

- Once you are a Professional, the Fixer calls about a big client: three jobs that pay far more than usual.
- After the third, your police contact warns you that the Fixer sold you out, and a killer is already on the way.
- The ending is your choice: kill the Fixer, or hand him over to the cop.

### Consequences

- **Traitor clients:** sometimes the client betrays you at the hand-in. They keep the rest of the money (kill them to collect it), call the police on you, or send a killer to tie up loose ends.
- **Revenge:** once a body is found, someone close to the victim may hire a killer to come after you.
- **Scared targets:** a target who refuses your bribe, or sees you kill, locks themselves at home for a while.

### Heat, the police and the corrupt cop

- **Heat** comes from deaths the police know about: more with witnesses, cameras or in public.
  - **Level 1:** the papers write about a contract killer.
  - **Level 2:** an Enforcer comes to your home and asks questions (a third one if they hold evidence). Good answers lower your heat; bad ones, or not being home, raise it.
  - **Level 3:** you are wanted everywhere, and hunters come for you.
- **The corrupt cop** calls at level 2 and leaves his number. Call him, or talk to him face to face, to buy a package:
  1. lose the witness statements (cheapest);
  2. wipe the camera footage and the statements;
  3. take your name off the investigation (heat to zero, the most expensive).

  On the phone he names a payphone where you leave the envelope; in person you pay on the spot.
- **Bribe a target:** offer to let them go for double the contract.
  - **If they accept**, they leave town and the client still pays. The client may find out later.
  - **If they refuse**, they fight or run for the police.

## Requirements

- BepInEx 6 IL2CPP (be.755+).
- **SOD.Common 2.0.7.** Newer releases do not load on the September 2024 game build.
- Asset Bundle Loader, DDSLoader and CommunityCaseLoader.

**Not compatible with the "Guns" mod**: the pistols include their own copy of its shooting code.

## Configuration

Everything is in `BepInEx/config/sod.hitman.cfg`:

| Section | What it covers |
|---|---|
| `Contract`, `Payment` | Meeting delay and tolerance, pay formula, deposit share |
| `Stealth` | Evidence-based heat (off = the old instant heat), witnesses, rating bonuses |
| `Heat`, `Interrogation` | Points, thresholds (30/60/90), each effect, the questions |
| `CorruptCop`, `Bribe` | The cop's calls and prices, bribe rules |
| `Weapons`, `Tranquilizer`, `Lethal`, `Arsenal` | Pistols, firearms, blades, garrote, poison, forged suicide |
| `Career`, `Fixer` | Tiers, contract unlocks, the Fixer's bonus and shop |
| `Consequences` | Traitor clients, revenge, scared targets |
| `Campaign` | The Last Contract: when it starts, pay, reward |

## Developer mode (hidden)

A developer mode exists for testing and debugging. It adds hotkeys, an in-game test menu (**Insert**) and detailed logging. To turn it on, do either of these:

- create an empty file named **`devmode.txt`** in `BepInEx/plugins/SOD.Hitman/` (or the mod's folder in your mod manager profile);
- add the line `DevMode = true` anywhere in `BepInEx/config/sod.hitman.cfg`.

The test menu can post any contract, teleport you to the client or target, change heat, reputation, money and time, and toggle the game's own debug switches. Its keys are in `BepInEx/config/sod.hitman.dev.cfg`, created the first time. Delete `devmode.txt` to go back to normal play.

## Languages

English and Brazilian Portuguese (follows the game language).

## Credits

- Built on SOD.Common (Venomaus), Asset Bundle Loader, DDSLoader and CommunityCaseLoader (Piepieonline).
- The pistols adapt MIT-licensed code from **Guns** by Spyraxi (ReveredRedHood) and **SODLethalActionReborn** by Venomaus (see `THIRD_PARTY_NOTICES.md`).

---

# Hitman Contracts (Português)

Vire um matador de aluguel em Shadows of Doubt. Os contratos aparecem nos quadros de side jobs. Ligue para o número, descubra o alvo, faça o serviço do seu jeito e entregue o caso. A polícia só sabe o que descobre: um corpo achado, uma testemunha que fala, uma câmera que estava ligada.

- **Contratos:** cinco de assassinato e um não letal.

  | Contrato | Como começa | Nível |
  |---|---|---|
  | Pizza | Encontro marcado | Iniciante |
  | Intimidação | Encontro marcado; surrar, sem matar | Iniciante |
  | Resolvedor de problemas | O cliente diz o alvo pelo telefone | Profissional |
  | Trabalho rápido | Um orelhão toca numa hora marcada | Profissional |
  | Coração partido | Encontro; o cônjuge do cliente o trai | Veterano |
  | Alto risco | Encontro; o alvo é Enforcer, segurança ou cobrador | Lenda |

  Metade do pagamento vem adiantada; o resto, na entrega.
- **Furtividade:**
  - A morte só gera heat quando o corpo é achado.
  - As testemunhas vão à polícia 1–2 horas depois. Antes disso, dá para subornar (Cr200), fazer dormir ou dar um jeito.
  - Câmeras só contam se estavam ligadas.
  - Cada serviço recebe uma nota: Assassino Silencioso (+25%), Profissional (+10%) ou Desleixado.
- **Arsenal:**
  - pistola letal e tranquilizante (3 minutos reais de sono);
  - armas de fogo do jogo (barulhentas);
  - facas e katana;
  - garrote por trás;
  - veneno em quem está dormindo (morte "natural" horas depois);
  - suicídio forjado com bilhete.
- **Carreira:** reputação em quatro níveis, que libera contratos e paga até ×2. O **Fixer** liga quando você tem nome: contratos exclusivos, sua ficha e uma loja de equipamento.
- **Campanha “O Último Contrato”:**
  - Quando você vira Profissional, o Fixer liga com um cliente grande: três serviços que pagam bem mais.
  - Depois, o policial avisa que o Fixer te vendeu.
  - Você escolhe o final: matar o Fixer ou entregá-lo ao policial.
- **Consequências:**
  - Às vezes o cliente trai você: fica com o resto do dinheiro (mate-o para cobrar), chama a polícia ou manda um matador.
  - Alguém próximo de uma vítima pode contratar um matador.
  - O alvo assustado se tranca em casa.
- **Heat:**
  - Nível 1: jornais.
  - Nível 2: um Enforcer vem à sua casa te interrogar.
  - Nível 3: mandado e caçadores.

  O **policial corrupto** (por telefone ou pessoalmente) vende três pacotes:
  1. sumir com os depoimentos;
  2. apagar as gravações e os depoimentos;
  3. tirar seu nome da investigação.

**Requisitos:** BepInEx 6 IL2CPP, **SOD.Common 2.0.7** (não atualize), Asset Bundle Loader, DDSLoader e CommunityCaseLoader. Incompatível com o mod "Guns".

**Modo de desenvolvedor (oculto):** crie um arquivo vazio **`devmode.txt`** em `BepInEx/plugins/SOD.Hitman/`, ou acrescente `DevMode = true` ao `sod.hitman.cfg`.
- Liga as teclas de teste, o menu de teste (**Insert**) e o log detalhado.
- As opções ficam em `sod.hitman.dev.cfg`.
- Apague o `devmode.txt` para voltar ao normal.
