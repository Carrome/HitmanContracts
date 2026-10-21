using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using SODHitman.Contracts;
using SODHitman.Heat;
using SODHitman.Weapons;
using UnityEngine;
using UiText = UnityEngine.UI.Text;
using UniverseLib;
using UniverseLib.Config;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;
using static SODHitman.Dev.Describe;

namespace SODHitman.Dev
{
    // In-game test menu (Dev.KeyMenu, Insert by default). Built with UniverseLib, which SOD.Common already ships and
    // initializes: it makes uGUI work under IL2CPP and frees the cursor while the menu is open. While open, the
    // player's mouse look and movement are off so the clicks stay on the menu.
    internal static class DevMenu
    {
        private const string UiId = "sod.hitman.devmenu";

        private static bool universeReady;
        private static UIBase ui;
        private static MenuPanel panel;
        private static UiText status;
        private static float nextStatus;
        private static bool ghost;
        private static int arsenalIndex;
        private static readonly List<Action> Refreshers = new();
        private static HitmanJobsPanel jobsPanel;
        private static BlackMarketsPanel marketsPanel;

        private static void OpenJobs() => (jobsPanel ??= new HitmanJobsPanel(ui)).Show();
        private static void OpenMarkets() => (marketsPanel ??= new BlackMarketsPanel(ui)).Show();

        internal static bool IsOpen { get; private set; }

        // Plugin load: join SOD.Common's UniverseLib setup (Init is safe to call again; the callback runs once ready).
        internal static void Init()
        {
            try
            {
                Universe.Init(0f, () => universeReady = true, null, new UniverseLibConfig
                {
                    Unhollowed_Modules_Folder = Path.Combine(Paths.BepInExRootPath, "interop"),
                    Force_Unlock_Mouse = true,
                });
            }
            catch (Exception e) { Plugin.Logger.LogError($"[Menu] UniverseLib init failed, menu disabled: {e}"); }
        }

        internal static void Toggle() => SetOpen(!IsOpen);

        internal static void SetOpen(bool open)
        {
            if (open == IsOpen) return;
            if (open)
            {
                if (!universeReady)
                {
                    Text.Notify("[Menu] O menu ainda está carregando. Tente de novo em alguns segundos.", "[Menu] The menu is still loading. Try again in a few seconds.");
                    return;
                }
                if (ui == null && !Build()) return;
            }
            IsOpen = open;
            ui.Enabled = open;
            panel?.SetActive(open);
            if (!open)
            {
                jobsPanel?.SetActive(false);
                marketsPanel?.SetActive(false);
            }
            SetPlayerControl(!open);
            if (open) Refresh();
            Plugin.Logger.LogInfo($"[Menu] {(open ? "opened" : "closed")}");
        }

        private static void SetPlayerControl(bool enabled)
        {
            var player = Player.Instance;
            if (player == null) return;
            try
            {
                player.EnablePlayerMouseLook(enabled, false);
                player.EnablePlayerMovement(enabled, true);
            }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Menu] Could not toggle player control: {e.Message}"); }
        }

        private static bool Build()
        {
            try
            {
                ui = UniversalUI.RegisterUI(UiId, OnUiUpdate);
                panel = new MenuPanel(ui);
                return true;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"[Menu] Could not build the menu: {e}");
                Text.Notify("[Menu] Não consegui montar o menu (veja o log).", "[Menu] Could not build the menu (see the log).");
                ui = null;
                return false;
            }
        }

        private static void OnUiUpdate()
        {
            if (!IsOpen || Time.unscaledTime < nextStatus) return;
            nextStatus = Time.unscaledTime + 0.5f;
            Refresh();
        }

        private static void Refresh()
        {
            if (status != null)
            {
                try { status.text = StatusText(); }
                catch (Exception e) { status.text = "status: " + e.Message; }
            }
            foreach (var r in Refreshers)
            {
                try { r(); } catch { }
            }
        }

        private static string StatusText()
        {
            if (!DevHotkeys.CityReady()) return Text.L("Nenhuma cidade carregada.", "No city loaded.");
            var h = ContractStore.Heat;
            var job = DevActions.Contract();
            var s = SessionData.Instance;
            var lines = new List<string>
            {
                Text.L($"<b>Hora</b> {Text.Clock(s.gameTime)}  ({s.currentTimeSpeed})   <b>Dinheiro</b> Cr{GameplayController.Instance.money}",
                       $"<b>Time</b> {Text.Clock(s.gameTime)}  ({s.currentTimeSpeed})   <b>Money</b> Cr{GameplayController.Instance.money}"),
                Text.L($"<b>Heat</b> {h.Points} pts, nível {h.Level} (níveis {HitmanConfig.HeatLevel1.Value}/{HitmanConfig.HeatLevel2.Value}/{HitmanConfig.HeatLevel3.Value})   mortes {h.Kills} (contrato {h.ContractKills})   caçadores {h.HunterIds.Count}",
                       $"<b>Heat</b> {h.Points} pts, level {h.Level} (levels {HitmanConfig.HeatLevel1.Value}/{HitmanConfig.HeatLevel2.Value}/{HitmanConfig.HeatLevel3.Value})   kills {h.Kills} (contract {h.ContractKills})   hunters {h.HunterIds.Count}"),
                Text.L($"<b>Interrogatório</b> {Interrogation.Status()}", $"<b>Interrogation</b> {Interrogation.Status()}"),
                Text.L($"<b>Evidências</b> {KillEvidence.Summary()}   <b>Carreira</b> {Career.TierName(Career.Tier)} ({h.Reputation})",
                       $"<b>Evidence</b> {KillEvidence.Summary()}   <b>Career</b> {Career.TierName(Career.Tier)} ({h.Reputation})"),
                Text.L($"<b>Fixer</b> {Fixer.Status()}", $"<b>Fixer</b> {Fixer.Status()}"),
                Text.L($"<b>Consequências</b> {Consequences.Status()}   <b>Campanha</b> {Campaign.Status()}",
                       $"<b>Consequences</b> {Consequences.Status()}   <b>Campaign</b> {Campaign.Status()}"),
                job == null
                    ? Text.L("<b>Contrato</b> nenhum", "<b>Contract</b> none")
                    : Text.L($"<b>Contrato</b> #{job.jobID} {(job.accepted ? "aceito" : "no quadro")} — alvo {Who(job.purp)}, cliente {Who(job.poster)}, Cr{ContractStore.Get(job).Payment}",
                             $"<b>Contract</b> #{job.jobID} {(job.accepted ? "accepted" : "on a board")} — target {Who(job.purp)}, client {Who(job.poster)}, Cr{ContractStore.Get(job).Payment}"),
                h.CopId >= 0
                    ? Text.L($"<b>Policial</b> {h.CopName}, contato {(h.CopContacted ? "feito" : "não")}" + (h.PendingDropNumber >= 0 ? $", envelope Cr{h.PendingCost} até {Text.Clock(h.PendingDeadline)}" : ""),
                             $"<b>Cop</b> {h.CopName}, contacted {(h.CopContacted ? "yes" : "no")}" + (h.PendingDropNumber >= 0 ? $", envelope Cr{h.PendingCost} by {Text.Clock(h.PendingDeadline)}" : ""))
                    : Text.L("<b>Policial</b> ainda não escolhido", "<b>Cop</b> not picked yet"),
                Text.L($"<b>Dormindo por dardo</b> {Tranquilizer.Sleeping}", $"<b>Asleep from darts</b> {Tranquilizer.Sleeping}"),
            };
            return string.Join("\n", lines);
        }

        // ---- layout ----

        private static void BuildContent(GameObject root)
        {
            Refreshers.Clear();
            status = UIFactory.CreateLabel(root, "Status", "", TextAnchor.UpperLeft, new Color(0.9f, 0.9f, 0.9f), true, 13);
            UIFactory.SetLayoutElement(status.gameObject, minHeight: 150, flexibleWidth: 9999);

            var scroll = UIFactory.CreateScrollView(root, "Options", out var content, out _, new Color(0.07f, 0.07f, 0.07f));
            UIFactory.SetLayoutElement(scroll, flexibleHeight: 9999, flexibleWidth: 9999);

            Section(content, Text.L("Contratos", "Contracts"));
            Row(content, (Text.L("» Missões Hitman (janela com todas)", "» Hitman missions (window with all of them)"), OpenJobs));
            Row(content,
                (Text.L("Postar contrato (F8)", "Post contract (F8)"), DevHotkeys.ForcePost),
                (Text.L("Listar contratos (F9)", "List contracts (F9)"), DevHotkeys.ListJobs));
            // One button per catalog contract (posts it even above the player's tier).
            Row(content,
                (Text.L("Postar: Pizza", "Post: Pizza"), () => DevActions.PostPreset("HitmanPizza")),
                (Text.L("Postar: Ligação", "Post: Call"), () => DevActions.PostPreset("HitmanCall")),
                (Text.L("Postar: Orelhão", "Post: Payphone"), () => DevActions.PostPreset("HitmanPayphone")));
            Row(content,
                (Text.L("Postar: Passional", "Post: Passion"), () => DevActions.PostPreset("HitmanPassion")),
                (Text.L("Postar: Distintivo", "Post: Badge"), () => DevActions.PostPreset("HitmanBadge")),
                (Text.L("Postar: Intimidação", "Post: Intimidation"), () => DevActions.PostPreset("HitmanIntimidate")));
            Row(content,
                (Text.L("Ir ao anúncio", "Go to the note"), DevActions.TeleportToPosting),
                (Text.L("Ir ao ponto de encontro", "Go to the meeting"), DevActions.TeleportToMeeting));
            Row(content,
                (Text.L("Ir ao cliente", "Go to the client"), DevActions.TeleportToClient),
                (Text.L("Ir ao alvo", "Go to the target"), DevActions.TeleportToTarget));
            Row(content,
                (Text.L("Matar alvo: tiro (F10)", "Kill target: shot (F10)"), () => DevHotkeys.KillTarget(viaMurder: false)),
                (Text.L("Matar alvo: direto (F11)", "Kill target: direct (F11)"), () => DevHotkeys.KillTarget(viaMurder: true)));
            Row(content,
                (Text.L("Marcar cumprido (alvo vivo)", "Mark fulfilled (target alive)"), DevActions.ForceFulfilled),
                (Text.L("Ver perguntas do caso", "Show case questions"), DevActions.ShowQuestions));

            Section(content, "Heat");
            Row(content,
                ("+10 (Shift+F6)", () => DevHotkeys.ChangeHeat(10)),
                ("−10 (Ctrl+F6)", () => DevHotkeys.ChangeHeat(-10)),
                (Text.L("Zerar", "Reset"), () => DevActions.SetHeat(0)));
            Row(content,
                (Text.L("Nível 1", "Level 1"), () => DevActions.SetHeatLevel(1)),
                (Text.L("Nível 2", "Level 2"), () => DevActions.SetHeatLevel(2)),
                (Text.L("Nível 3", "Level 3"), () => DevActions.SetHeatLevel(3)));
            Row(content,
                (Text.L("Interrogatório agora (F12)", "Interrogation now (F12)"), () =>
                {
                    Interrogation.ForceVisit();
                    Text.Notify("[Menu] Enforcers vão vir te interrogar (fique em casa).", "[Menu] Enforcers are coming to interrogate you (stay at home).");
                }),
                (Text.L("Cancelar interrogatório", "Cancel interrogation"), Interrogation.DevCancel));
            Row(content,
                (Text.L("Enforcers em casa (jogo)", "Enforcers home (game)"), DevActions.VanillaEnforcers),
                (Text.L("Dispensar caçadores", "Call off hunters"), HeatSystem.DevCancelHunters));

            Section(content, Text.L("Evidências", "Evidence"));
            Row(content,
                (Text.L("Ver último crime", "Show last crime"), KillEvidence.DevShowLast),
                (Text.L("Testemunhas pendentes", "Pending witnesses"), KillEvidence.DevListPending));
            Row(content,
                (Text.L("Testemunhas denunciam já", "Witnesses report now"), KillEvidence.DevReportAll),
                (Text.L("Silenciar testemunhas", "Silence witnesses"), () =>
                {
                    int n = KillEvidence.DevSilenceAll();
                    Text.Notify($"[Menu] {n} testemunha(s) silenciada(s).", $"[Menu] {n} witness(es) silenced.");
                }),
                (Text.L("Achar corpo escondido", "Find hidden body"), KillEvidence.DevFindLastBody));

            Section(content, Text.L("Carreira e Fixer", "Career and Fixer"));
            Row(content,
                (Text.L("+5 de reputação", "+5 reputation"), () => Career.AddReputation(5, "dev menu")),
                (Text.L("Ficha do assassino", "Killer's record"), () => Text.Notify("[Menu] " + Career.Sheet(), "[Menu] " + Career.Sheet())));
            Row(content,
                (Text.L("Fixer liga agora", "Fixer calls now"), () =>
                {
                    Fixer.CallNow();
                    Text.Notify("[Menu] O Fixer está ligando para a sua casa.", "[Menu] The Fixer is calling your home.");
                }),
                (Text.L("Número do Fixer no caderno", "Fixer's number in notebook"), Fixer.DevMarkContacted),
                (Text.L("Ir ao Fixer", "Go to the Fixer"), () => DevActions.TeleportToHuman(Fixer.DevFixer(), "Fixer")));
            Row(content,
                (Text.L("Ir à porta da casa do Fixer (lado de fora)", "Go to the Fixer's front door (outside)"),
                    () => DevActions.TeleportOutsideHome(Fixer.DevFixer(), "Fixer")));

            Section(content, Text.L("Consequências (1.1)", "Consequences (1.1)"));
            Row(content,
                (Text.L("Próxima entrega: não paga", "Next hand-in: no pay"), () => Consequences.DevForceNext(Consequences.NoPay)),
                (Text.L("Próxima entrega: polícia", "Next hand-in: police"), () => Consequences.DevForceNext(Consequences.Police)),
                (Text.L("Próxima entrega: caçador", "Next hand-in: killer"), () => Consequences.DevForceNext(Consequences.Hunter)));
            Row(content,
                (Text.L("Assustar o alvo atual", "Scare the current target"), Consequences.DevScareTarget),
                (Text.L("Vingança agora", "Revenge now"), Consequences.DevRevengeNow),
                (Text.L("Dívidas e caçadores", "Debts and killers"), Consequences.DevList));

            Section(content, Text.L("Campanha “O Último Contrato” (1.2)", "Campaign “The Last Contract” (1.2)"));
            Row(content,
                (Text.L("Começar (o Fixer liga)", "Start (the Fixer calls)"), Campaign.DevStart),
                (Text.L("Pular capítulo", "Skip chapter"), Campaign.DevSkipChapter),
                (Text.L("Traição agora (o policial liga)", "Betrayal now (the cop calls)"), Campaign.DevBetrayalNow));

            Section(content, Text.L("Mercados negros (mercado, clínica clandestina, cassino ilegal)", "Black markets (trader, sync clinic, gambling den)"));
            Row(content, (Text.L("» Lista dos três tipos (janela: senha, porta, quem atende)", "» List of all three kinds (window: password, door, who runs it)"), OpenMarkets));
            Row(content,
                (Text.L("Ir ao mais próximo (porta)", "Go to the nearest (door)"), () =>
                {
                    var nearest = BlackMarkets.All().FirstOrDefault();
                    if (nearest == null) Text.Notify("[Menu] Esta cidade não tem mercado negro, clínica clandestina nem cassino ilegal.", "[Menu] This city has no black market, sync clinic or gambling den.");
                    else BlackMarkets.TeleportOutside(nearest);
                }),
                (Text.L("Aprender todas as senhas", "Learn every password"), () =>
                {
                    var all = BlackMarkets.All();
                    foreach (var m in all) GameplayController.Instance.SetPlayerKnowsPassword(m);
                    Text.Notify($"[Menu] Você agora sabe a senha de {all.Count} lugar(es) (mercados, clínicas e cassinos).", $"[Menu] You now know the password of {all.Count} place(s) (markets, clinics and dens).");
                }));

            Section(content, Text.L("Policial corrupto", "Corrupt cop"));
            Row(content,
                (Text.L("Ligar agora (F4)", "Call now (F4)"), () =>
                {
                    CorruptCop.CallNow();
                    Text.Notify("[Menu] O policial corrupto está ligando.", "[Menu] The corrupt cop is calling.");
                }),
                (Text.L("Número no caderno", "Number in notebook"), CorruptCop.DevMarkContacted));
            Row(content,
                (Text.L("Ir ao policial", "Go to the cop"), DevActions.TeleportToCop),
                (Text.L("Ir ao orelhão do envelope", "Go to the envelope payphone"), DevActions.TeleportToDrop),
                (Text.L("Pagar envelope já", "Pay envelope now"), DevActions.PayEnvelope));
            Row(content,
                (Text.L("Ir à porta da casa do policial (lado de fora)", "Go to the cop's front door (outside)"),
                    () => DevActions.TeleportOutsideHome(CorruptCop.DevCop(), Text.L("policial", "cop"))));

            Section(content, Text.L("Armas", "Weapons"));
            Row(content,
                (Text.L("Dar as duas pistolas (F3)", "Give both pistols (F3)"), () =>
                {
                    WeaponShops.GiveBoth();
                    Text.Notify("[Menu] As duas pistolas foram para o inventário.", "[Menu] Both pistols are in your inventory.");
                }),
                (Text.L("Acordar tranquilizados", "Wake darted citizens"), () =>
                {
                    int n = Tranquilizer.WakeAllNow();
                    Text.Notify($"[Menu] Acordados: {n}.", $"[Menu] Woken: {n}.");
                }));
            Row(content,
                (Text.L("Dar um item do arsenal", "Give an arsenal item"), () =>
                {
                    var items = System.Linq.Enumerable.ToList(Arsenal.Items);
                    if (items.Count == 0) { Text.Notify("[Menu] Nenhum item do arsenal neste jogo.", "[Menu] No arsenal item in this game."); return; }
                    var next = items[arsenalIndex++ % items.Count];
                    WeaponShops.Give(next);
                    Text.Notify($"[Menu] Recebido: {next.name}. Clique de novo para o próximo.", $"[Menu] Given: {next.name}. Click again for the next one.");
                }),
                (Text.L("Alvo morre de veneno já", "Target dies of poison now"), () =>
                {
                    var job = DevActions.Contract();
                    if (job?.purp == null || job.purp.isDead) { Text.Notify("[Menu] Sem alvo vivo.", "[Menu] No living target."); return; }
                    WeaponKills.DieNaturally(job.purp, "poison");
                }));
            Row(content,
                (Text.L("Todos esquecem você", "Everyone forgets you"), () =>
                {
                    int n = Tranquilizer.ForgetEveryone();
                    Text.Notify($"[Menu] {n} cidadãos esqueceram você.", $"[Menu] {n} citizens forgot you.");
                }));

            Section(content, Text.L("Jogador", "Player"));
            Row(content,
                (Text.L("Vida cheia", "Full health"), DevActions.Heal),
                ("+Cr1.000", () => DevActions.AddMoney(1000)),
                ("+Cr10.000", () => DevActions.AddMoney(10000)));
            Row(content,
                (Text.L("Limpar procurado", "Clear wanted"), DevActions.ClearWanted),
                (Text.L("Ir para casa", "Go home"), DevActions.TeleportHome),
                (Text.L("Onde estou?", "Where am I?"), DevActions.WhereAmI));
            ToggleRow(content,
                (Text.L("Invencível", "Invincible"), () => Game.Instance.invinciblePlayer, v => Game.Instance.invinciblePlayer = v),
                (Text.L("Invisível", "Invisible"), () => Game.Instance.invisiblePlayer, v => Game.Instance.invisiblePlayer = v));
            ToggleRow(content,
                (Text.L("Silencioso", "Inaudible"), () => Game.Instance.inaudiblePlayer, v => Game.Instance.inaudiblePlayer = v),
                (Text.L("Sem dano de queda", "No fall damage"), () => Game.Instance.disableFallDamage, v => Game.Instance.disableFallDamage = v));
            ToggleRow(content,
                (Text.L("Modo fantasma (atravessa paredes)", "Ghost mode (noclip)"), () => ghost, v =>
                {
                    ghost = v;
                    Player.Instance.EnableGhostMovement(v, v, 0f);
                }),
                (Text.L("Sem invasão de propriedade", "No trespassing"), () => Game.Instance.disableTrespass, v => Game.Instance.disableTrespass = v));

            Section(content, Text.L("Cidade e NPCs", "City and NPCs"));
            ToggleRow(content,
                (Text.L("Pausar IA", "Pause AI"), () => Game.Instance.pauseAI, v => Game.Instance.pauseAI = v),
                (Text.L("NPCs não reagem a ataques", "NPCs ignore attacks"), () => Game.Instance.noReactOnAttack, v => Game.Instance.noReactOnAttack = v));
            ToggleRow(content,
                (Text.L("Chaves da cidade (portas)", "Keys to the city (doors)"), () => Game.Instance.keysToTheCity, v => Game.Instance.keysToTheCity = v));

            Section(content, Text.L("Tempo", "Time"));
            Row(content,
                (Text.L("Normal", "Normal"), () => DevActions.TimeSpeed(SessionData.TimeSpeed.normal)),
                (Text.L("Rápido", "Fast"), () => DevActions.TimeSpeed(SessionData.TimeSpeed.fast)),
                (Text.L("Muito rápido", "Very fast"), () => DevActions.TimeSpeed(SessionData.TimeSpeed.veryFast)));
            Row(content,
                ("+1 h", () => DevActions.SkipHours(1f)),
                ("+6 h", () => DevActions.SkipHours(6f)),
                ("+24 h", () => DevActions.SkipHours(24f)));

            Section(content, Text.L("Ferramentas", "Tools"));
            Row(content,
                (Text.L("Gravar dump (F2)", "Write dump (F2)"), () => PresetDumper.DumpAll("menu")),
                (Text.L("Fechar menu (Insert)", "Close menu (Insert)"), () => SetOpen(false)));
        }

        internal static void Section(GameObject parent, string title)
        {
            var label = UIFactory.CreateLabel(parent, "Section_" + title, $"<b>{title}</b>", TextAnchor.LowerLeft, new Color(0.95f, 0.75f, 0.35f), true, 15);
            UIFactory.SetLayoutElement(label.gameObject, minHeight: 26, flexibleWidth: 9999);
        }

        private static GameObject NewRow(GameObject parent)
        {
            var row = UIFactory.CreateHorizontalGroup(parent, "Row", true, false, true, true, 4, new Vector4(2, 2, 2, 2));
            UIFactory.SetLayoutElement(row, minHeight: 30, flexibleWidth: 9999);
            return row;
        }

        internal static void Row(GameObject parent, params (string label, Action action)[] buttons)
        {
            var row = NewRow(parent);
            foreach (var (label, action) in buttons)
            {
                var button = UIFactory.CreateButton(row, "Button", label);
                UIFactory.SetLayoutElement(button.GameObject, minHeight: 28, flexibleWidth: 9999);
                var text = label;
                button.OnClick += () => DevActions.Run(text, action);
            }
        }

        private static void ToggleRow(GameObject parent, params (string label, Func<bool> get, Action<bool> set)[] toggles)
        {
            var row = NewRow(parent);
            foreach (var (label, get, set) in toggles)
            {
                var button = UIFactory.CreateButton(row, "Toggle", label);
                UIFactory.SetLayoutElement(button.GameObject, minHeight: 28, flexibleWidth: 9999);
                var (name, read, write) = (label, get, set);
                void Show()
                {
                    bool on = false;
                    try { on = read(); } catch { }
                    button.ButtonText.text = $"{name}: {(on ? Text.L("LIGADO", "ON") : Text.L("desligado", "off"))}";
                    button.ButtonText.color = on ? new Color(0.5f, 1f, 0.5f) : new Color(0.85f, 0.85f, 0.85f);
                }
                button.OnClick += () =>
                {
                    DevActions.Run(name, () =>
                    {
                        bool now = !read();
                        write(now);
                        Text.Notify($"[Menu] {name}: {(now ? "ligado" : "desligado")}", $"[Menu] {name}: {(now ? "on" : "off")}");
                    });
                    Show();
                };
                Refreshers.Add(Show);
            }
        }

        private sealed class MenuPanel : PanelBase
        {
            public MenuPanel(UIBase owner) : base(owner) { }

            public override string Name => Text.L("Hitman — menu de teste (Insert)", "Hitman — test menu (Insert)");
            public override int MinWidth => 520;
            public override int MinHeight => 560;
            public override Vector2 DefaultAnchorMin => new(0.55f, 0.08f);
            public override Vector2 DefaultAnchorMax => new(0.97f, 0.94f);

            protected override void ConstructPanelContent() => BuildContent(ContentRoot);

            protected override void OnClosePanelClicked() => SetOpen(false);
        }
    }
}
