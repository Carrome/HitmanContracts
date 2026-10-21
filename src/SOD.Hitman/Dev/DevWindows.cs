using System;
using System.Collections.Generic;
using System.Linq;
using SODHitman.Contracts;
using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Panels;
using static SODHitman.Dev.Describe;

namespace SODHitman.Dev
{
    // Windows the test menu opens next to itself (decided 20/10/2026): a list rebuilt from the city when opened, with a
    // Refresh and a Close button on top.
    internal abstract class ListPanel : PanelBase
    {
        private GameObject list;

        protected ListPanel(UIBase owner) : base(owner) { }

        public override int MinWidth => 560;
        public override int MinHeight => 420;

        protected abstract void Fill(GameObject list);

        protected override void ConstructPanelContent()
        {
            DevMenu.Row(ContentRoot,
                (Text.L("Atualizar", "Refresh"), Rebuild),
                (Text.L("Fechar", "Close"), () => SetActive(false)));
            var scroll = UIFactory.CreateScrollView(ContentRoot, "List", out list, out _, new Color(0.07f, 0.07f, 0.07f));
            UIFactory.SetLayoutElement(scroll, flexibleHeight: 9999, flexibleWidth: 9999);
        }

        internal void Show()
        {
            SetActive(true);
            Rebuild();
        }

        internal void Rebuild()
        {
            if (list == null) return;
            var t = list.transform;
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
            try { Fill(list); }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"[Menu] {Name}: {e}");
                Info(list, Text.L("Erro ao montar a lista (veja o log).", "Could not build the list (see the log)."), 1);
            }
        }

        // A block of compact text lines (rich text).
        protected static void Info(GameObject parent, string text, int lines)
        {
            var label = UIFactory.CreateLabel(parent, "Info", text, TextAnchor.UpperLeft, new Color(0.9f, 0.9f, 0.9f), true, 13);
            UIFactory.SetLayoutElement(label.gameObject, minHeight: lines * 17 + 6, flexibleWidth: 9999);
        }

        protected static void Gap(GameObject parent)
        {
            var label = UIFactory.CreateLabel(parent, "Gap", "", TextAnchor.UpperLeft, Color.clear, false, 6);
            UIFactory.SetLayoutElement(label.gameObject, minHeight: 8, flexibleWidth: 9999);
        }

        protected static string Grey(string s) => $"<color=#9a9a9a>{s}</color>";
        protected static string YesNo(bool b) => b ? Text.L("sim", "yes") : Text.L("não", "no");
    }

    // "Missões Hitman": every Hitman mission on a board or accepted, with what a tester needs to know and do.
    internal sealed class HitmanJobsPanel : ListPanel
    {
        public HitmanJobsPanel(UIBase owner) : base(owner) { }

        public override string Name => Text.L("Missões Hitman", "Hitman missions");
        public override Vector2 DefaultAnchorMin => new(0.03f, 0.08f);
        public override Vector2 DefaultAnchorMax => new(0.53f, 0.94f);

        protected override void Fill(GameObject list)
        {
            var jobs = ContractRegistry.AllJobs()
                .Where(j => ContractRegistry.IsHitmanJob(j) && !j.failed && j.state != SideJob.JobState.ended)
                .OrderByDescending(j => j.accepted).ThenBy(j => j.jobID)
                .ToList();
            if (jobs.Count == 0)
            {
                Info(list, Text.L("Nenhuma missão Hitman ativa. Use os botões \"Postar: …\" do menu.", "No active Hitman mission. Use the menu's \"Post: …\" buttons."), 1);
                return;
            }
            foreach (var job in jobs)
            {
                var (text, lines) = Describe(job);
                Info(list, text, lines);
                var j = job;
                DevMenu.Row(list,
                    (Text.L("Ir ao bilhete", "Go to the note"), () => DevActions.TeleportToPosting(j)),
                    (j.accepted ? Text.L("Já aceito", "Already accepted") : Text.L("Aceitar já", "Accept now"), () => { DevActions.AcceptNow(j); Rebuild(); }),
                    (Text.L("Ir ao cliente", "Go to the client"), () => DevActions.TeleportToClient(j)),
                    (Text.L("Ir ao alvo", "Go to the target"), () => DevActions.TeleportToTarget(j)));
                DevMenu.Row(list,
                    (Text.L("Ir ao encontro", "Go to the meeting"), () => DevActions.TeleportToMeeting(j)),
                    (Text.L("Matar o alvo", "Kill the target"), () => { DevActions.KillTarget(j); Rebuild(); }),
                    (Text.L("Marcar cumprido", "Mark fulfilled"), () => { DevActions.ForceFulfilled(j); Rebuild(); }),
                    (Text.L("Encerrar", "End it"), () => { DevActions.EndJob(j); Rebuild(); }));
                Gap(list);
            }
        }

        private static (string text, int lines) Describe(SideJob job)
        {
            var s = ContractStore.Get(job);
            var heat = ContractStore.Heat;
            string status = job.accepted ? Text.L("aceito", "accepted")
                          : job.state == SideJob.JobState.posted ? Text.L("no quadro", "on a board")
                          : Text.L("indo para um quadro", "going up on a board");
            string campaign = heat.CampaignJobId == job.jobID ? Text.L($" · campanha, capítulo {heat.CampaignStage}", $" · campaign, chapter {heat.CampaignStage}") : "";
            string kind = ContractRegistry.IsKillContract(job) ? "" : Text.L(" · não letal", " · non-lethal");
            var lines = new List<string>
            {
                $"<b>#{job.jobID} {ContractRegistry.DisplayName(job.preset)}</b>  {Grey(status + kind + campaign)}",
            };

            var purp = job.purp;
            var occupation = purp != null ? purp.job : null;
            string occ = occupation != null && occupation.preset != null ? occupation.preset.name : Text.L("sem emprego", "jobless");
            lines.Add(Text.L($"Alvo: {Who(purp)} ({occ}){(purp != null && purp.isDead ? " — MORTO" : "")} · Cliente: {Who(job.poster)}",
                             $"Target: {Who(purp)} ({occ}){(purp != null && purp.isDead ? " — DEAD" : "")} · Client: {Who(job.poster)}"));

            string pay = s.Payment > 0 ? $"Cr{s.Payment}" : "?";
            string deposit = s.DepositPaid > 0 ? Text.L($" (sinal Cr{s.DepositPaid} pago)", $" (Cr{s.DepositPaid} deposit paid)") : "";
            string bribe = "?";
            if (s.Payment > 0 && purp != null)
            {
                bool employed = occupation != null && occupation.employer != null;
                bool accepts = Bribe.WouldAccept(Payment.AnnualSalary(occupation), employed, s.Payment, out _);
                bribe = accepts ? Text.L("aceitaria", "would accept") : Text.L("recusaria", "would refuse");
            }
            lines.Add(Text.L($"Pagamento: {pay}{deposit} · Suborno: {bribe}", $"Pay: {pay}{deposit} · Bribe: {bribe}"));
            lines.Add(Text.L($"Bilhete: {(job.post != null ? Place(job.post) : "ainda não")} · Chegada: {Intro(job)}",
                             $"Note: {(job.post != null ? Place(job.post) : "not yet")} · Intro: {Intro(job)}"));

            if (job.accepted)
            {
                string meet = s.MeetTime >= 0f
                    ? Text.L($"{Text.Clock(s.MeetTime)} em {(job.chosenMeetingPoint != null ? Place(job.chosenMeetingPoint) : "?")}", $"{Text.Clock(s.MeetTime)} at {(job.chosenMeetingPoint != null ? Place(job.chosenMeetingPoint) : "?")}")
                    : Text.L("não marcado", "not set");
                lines.Add(Grey(Text.L($"Encontro: {meet} · Detalhes recebidos: {YesNo(s.InfoAcquired)} · Morte registrada: {YesNo(s.KillRecorded)}",
                                      $"Meeting: {meet} · Details received: {YesNo(s.InfoAcquired)} · Kill recorded: {YesNo(s.KillRecorded)}")));
            }
            return (string.Join("\n", lines), lines.Count);
        }

        private static string Intro(SideJob job)
        {
            var name = job.chosenIntro != null && job.chosenIntro.preset != null ? job.chosenIntro.preset.name : null;
            if (name == null) return "?";
            if (name.Contains("MeetUp")) return Text.L("encontro", "meeting");
            if (name.Contains("GooseChase")) return Text.L("orelhão", "payphone");
            if (name.Contains("TelephoneCall")) return Text.L("ligação", "phone call");
            return name;
        }
    }

    // "Mercados negros": every black market (and black market sync clinic) in the city, nearest first, with its
    // password, a teleport to the outside of its door, and its trader.
    internal sealed class BlackMarketsPanel : ListPanel
    {
        public BlackMarketsPanel(UIBase owner) : base(owner) { }

        public override string Name => Text.L("Mercados negros, clínicas e cassinos", "Black markets, clinics and gambling dens");
        public override Vector2 DefaultAnchorMin => new(0.03f, 0.2f);
        public override Vector2 DefaultAnchorMax => new(0.53f, 0.8f);

        protected override void Fill(GameObject list)
        {
            var markets = BlackMarkets.All();
            if (markets.Count == 0)
            {
                Info(list, Text.L("Esta cidade não tem mercado negro, clínica clandestina nem cassino ilegal.", "This city has no black market, sync clinic or gambling den."), 1);
                return;
            }
            foreach (var m in markets)
            {
                var (text, lines) = Describe(m);
                Info(list, text, lines);
                var market = m;
                DevMenu.Row(list,
                    (Text.L("Ir para a porta (lado de fora)", "Go to the door (outside)"), () => BlackMarkets.TeleportOutside(market)),
                    (Text.L("Ir a quem atende", "Go to who runs it"), () => DevActions.TeleportToHuman(BlackMarkets.Trader(market), Text.L("responsável", "owner"))),
                    (Text.L("Aprender a senha", "Learn the password"), () => { BlackMarkets.LearnPassword(market); Rebuild(); }));
                Gap(list);
            }
        }

        private static (string text, int lines) Describe(NewAddress a)
        {
            string kind = BlackMarkets.Kind(a);
            string company = a.company != null && !string.IsNullOrEmpty(a.company.name) ? a.company.name : N(a);
            string password = BlackMarkets.Password(a);
            string known = BlackMarkets.PlayerKnows(a) ? Grey(Text.L(" (você já sabe)", " (you know it)")) : "";
            var door = BlackMarkets.Outside(a);
            var player = Player.Instance;
            string distance = door != null && player != null ? $"{(door.position - player.transform.position).magnitude:0} m" : "?";
            var lines = new List<string>
            {
                $"<b>{kind}: {company}</b>  {Grey(distance)}",
                Text.L($"Senha: <b>{password}</b>{known}", $"Password: <b>{password}</b>{known}"),
                Text.L($"Onde: {N(a)} — {N(a.building)}, andar {(a.floor != null ? a.floor.floor.ToString() : "?")}",
                       $"Where: {N(a)} — {N(a.building)}, floor {(a.floor != null ? a.floor.floor.ToString() : "?")}"),
                Grey(Text.L($"Quem atende: {Who(BlackMarkets.Trader(a))}{FixerMark(a)}", $"Run by: {Who(BlackMarkets.Trader(a))}{FixerMark(a)}")),
            };
            return (string.Join("\n", lines), lines.Count);
        }

        // The Fixer often runs one of these (gambling den owner, black market trader…).
        private static string FixerMark(NewAddress a)
        {
            var trader = BlackMarkets.Trader(a);
            int fixer = ContractStore.Heat.FixerId;
            return trader != null && fixer >= 0 && trader.humanID == fixer ? Text.L(" — é o seu Fixer", " — your Fixer") : "";
        }
    }

    // The three underground businesses (decided 20/10/2026): black market trader, black market sync clinic and gambling
    // den, plus any other address the game puts behind a password door (AddressPreset.needsPassword).
    internal static class BlackMarkets
    {
        private static readonly string[] Presets = { "BlackmarketTrader", "BlackmarketSyncClinic", "GamblingDen" };

        internal static string Kind(NewAddress a) => (a?.addressPreset != null ? a.addressPreset.name : null) switch
        {
            "BlackmarketTrader" => Text.L("Mercado negro", "Black market"),
            "BlackmarketSyncClinic" => Text.L("Clínica clandestina (sync disks)", "Black market sync clinic"),
            "GamblingDen" => Text.L("Cassino ilegal", "Gambling den"),
            var other => other ?? "?",
        };

        // Nearest first.
        internal static List<NewAddress> All()
        {
            var result = new List<NewAddress>();
            foreach (var a in Each(CityData.Instance != null ? CityData.Instance.addressDirectory : null))
                if (a != null && a.addressPreset != null && (Presets.Contains(a.addressPreset.name) || a.addressPreset.needsPassword)) result.Add(a);
            var player = Player.Instance;
            if (player == null) return result;
            var at = player.transform.position;
            return result.OrderBy(a => Outside(a) is NewNode n ? (n.position - at).sqrMagnitude : float.MaxValue).ToList();
        }

        internal static string Password(NewAddress a)
        {
            try
            {
                var p = a.GetPassword();
                return string.IsNullOrEmpty(p) ? Text.L("(nenhuma)", "(none)") : p;
            }
            catch (Exception e) { return "? " + e.Message; }
        }

        internal static bool PlayerKnows(NewAddress a)
        {
            var known = GameplayController.Instance != null ? GameplayController.Instance.playerKnowsPasswords : null;
            return known != null && known.Contains(a.id);
        }

        internal static void LearnPassword(NewAddress a)
        {
            GameplayController.Instance.SetPlayerKnowsPassword(a);
            Text.Notify($"[Menu] Senha de {N(a)}: {Password(a)}", $"[Menu] Password of {N(a)}: {Password(a)}");
        }

        // The node on the other side of the market's door (the hallway or the street).
        internal static NewNode Outside(NewAddress a)
        {
            NewNode street = null;
            foreach (var access in Each(a != null ? a.entrances : null))
            {
                if (access == null || access.door == null) continue;
                foreach (var node in new[] { access.fromNode, access.toNode })
                {
                    var place = node != null ? node.gameLocation : null;
                    if (place == null || place == a) continue;
                    if (!place.isOutside) return node;
                    street ??= node;
                }
            }
            return street;
        }

        internal static void TeleportOutside(NewAddress a)
        {
            var node = Outside(a);
            DevActions.TeleportTo(node, Text.L($"porta de {N(a)} (senha: {Password(a)})", $"door of {N(a)} (password: {Password(a)})"));
        }

        // Whoever works there (the trader), else the company's director.
        internal static Human Trader(NewAddress a)
        {
            var company = a != null ? a.company : null;
            if (company == null) return null;
            foreach (var job in Each(company.companyRoster))
                if (job != null && job.employee != null && !job.employee.isDead) return job.employee;
            return company.director;
        }
    }
}
