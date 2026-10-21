using System;
using SODHitman.Contracts;
using SODHitman.Heat;
using SODHitman.Weapons;
using static SODHitman.Dev.Describe;

namespace SODHitman.Dev
{
    // What the test menu (DevMenu) does. Each action needs a loaded city, logs as [Menu] and reports on screen.
    internal static class DevActions
    {
        private static HeatState H => ContractStore.Heat;

        internal static void Run(string label, Action action)
        {
            if (!DevHotkeys.CityReady())
            {
                Text.Notify("[Menu] Carregue uma cidade primeiro.", "[Menu] Load a city first.");
                return;
            }
            try
            {
                Plugin.Logger.LogInfo($"[Menu] {label}");
                action();
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"[Menu] '{label}' failed: {e}");
                Text.Notify($"[Menu] Falhou: {label} (veja o log).", $"[Menu] Failed: {label} (see the log).");
            }
        }

        private static void Say(string pt, string en) => Text.Notify("[Menu] " + pt, "[Menu] " + en);

        // ---- contracts ----

        // The accepted Hitman contract, else the newest one on a board.
        internal static SideJob Contract()
        {
            var accepted = ContractRegistry.CurrentAcceptedJob();
            if (accepted != null && ContractRegistry.IsHitmanJob(accepted)) return accepted;
            SideJob newest = null;
            foreach (var j in ContractRegistry.ActiveHitmanJobs())
                if (newest == null || j.jobID > newest.jobID) newest = j;
            return newest;
        }

        private static SideJob NeedContract()
        {
            var job = Contract();
            if (job == null) Say("Nenhum contrato ativo. Use \"Postar contrato\".", "No active contract. Use \"Post contract\".");
            return job;
        }

        internal static void TeleportToPosting() => TeleportToPosting(NeedContract());
        internal static void TeleportToClient() => TeleportToClient(NeedContract());
        internal static void TeleportToTarget() => TeleportToTarget(NeedContract());
        internal static void TeleportToMeeting() => TeleportToMeeting(NeedContract());

        // ---- one contract (the Hitman missions window) ----

        internal static void TeleportToPosting(SideJob job)
        {
            if (job == null) return;
            if (job.post == null)
            {
                Say($"O anúncio do contrato #{job.jobID} ainda não está num quadro.", $"Contract #{job.jobID}'s note is not on a board yet.");
                return;
            }
            TeleportTo(job.post.node, Text.L($"anúncio do contrato #{job.jobID} ({Place(job.post)})", $"note of contract #{job.jobID} ({Place(job.post)})"));
        }

        internal static void TeleportToClient(SideJob job)
        {
            if (job != null) TeleportTo(job.poster?.currentNode, Text.L($"cliente {Who(job.poster)}", $"client {Who(job.poster)}"));
        }

        internal static void TeleportToTarget(SideJob job)
        {
            if (job != null) TeleportTo(job.purp?.currentNode, Text.L($"alvo {Who(job.purp)}", $"target {Who(job.purp)}"));
        }

        internal static void TeleportToMeeting(SideJob job)
        {
            if (job == null) return;
            var meet = job.chosenMeetingPoint;
            if (meet == null)
            {
                Say("O encontro ainda não foi marcado (ligue para o número do anúncio).", "The meeting is not set yet (call the number on the note).");
                return;
            }
            TeleportTo(meet.node, Text.L("ponto de encontro", "meeting point"));
        }

        // As the note's Accept button does (EvidenceLocationalControls.OnAcceptJob): opens the case.
        internal static void AcceptNow(SideJob job)
        {
            if (job == null) return;
            if (job.accepted)
            {
                Say($"O contrato #{job.jobID} já foi aceito.", $"Contract #{job.jobID} is already accepted.");
                return;
            }
            job.AcceptJob();
            if (job.accepted) Say($"Contrato #{job.jobID} aceito: o caso foi aberto. Ligue para o número do anúncio.", $"Contract #{job.jobID} accepted: the case is open. Call the number on the note.");
            else Say("O jogo não aceitou (casos demais abertos?). Veja o log.", "The game did not accept it (too many open cases?). See the log.");
        }

        internal static void KillTarget(SideJob job)
        {
            var purp = job?.purp;
            if (purp == null || purp.isDead)
            {
                Say("O alvo não existe ou já está morto.", "The target is missing or already dead.");
                return;
            }
            purp.RecieveDamage(99999f, Player.Instance, purp.transform.position, UnityEngine.Vector3.forward, null, null, forceRagdoll: true, enableKill: false);
            WeaponKills.Kill(purp, null, "dev menu");
            Say($"{Who(purp)} morto (contrato #{job.jobID}).", $"{Who(purp)} killed (contract #{job.jobID}).");
        }

        internal static void EndJob(SideJob job)
        {
            if (job == null) return;
            job.End();
            Say($"Contrato #{job.jobID} encerrado.", $"Contract #{job.jobID} ended.");
        }

        // The job's resolve questions, for hand-in problems.
        internal static void ShowQuestions()
        {
            var job = NeedContract();
            if (job == null) return;
            var state = ContractStore.Get(job);
            int n = 0;
            foreach (var q in Each(ContractRegistry.Questions(job)))
            {
                if (q == null) continue;
                n++;
                Plugin.Logger.LogInfo($"[Menu] Job #{job.jobID} question '{q.name}' type={q.inputType} progress={q.progress} completedRevenge={q.completedRevenge} " +
                                      $"isCorrect={q.isCorrect} reward={q.reward} optional={q.isOptional}");
            }
            var kill = ContractRegistry.KillQuestion(job);
            Say($"Contrato #{job.jobID}: {n} pergunta(s); \"matar\" {(kill == null ? "não encontrada" : kill.completedRevenge ? "cumprida" : "pendente")}. " +
                $"Aceito={job.accepted}, info={state.InfoAcquired}, morte={state.KillRecorded}. Detalhes no log.",
                $"Contract #{job.jobID}: {n} question(s); kill {(kill == null ? "not found" : kill.completedRevenge ? "done" : "pending")}. " +
                $"Accepted={job.accepted}, info={state.InfoAcquired}, kill={state.KillRecorded}. Details in the log.");
        }

        // Marks the kill as done without killing anyone, to test the hand-in.
        internal static void ForceFulfilled() => ForceFulfilled(NeedContract());

        internal static void ForceFulfilled(SideJob job)
        {
            if (job == null) return;
            if (!job.accepted)
            {
                Say("Aceite o contrato antes.", "Accept the contract first.");
                return;
            }
            KillTracker.CompleteKill(job, Player.Instance, "dev menu");
            Say($"Contrato #{job.jobID} marcado como cumprido (alvo vivo).", $"Contract #{job.jobID} marked fulfilled (target alive).");
        }

        // ---- heat ----

        internal static void SetHeat(int points)
        {
            HeatSystem.Change(points - H.Points, "dev menu");
            DevHotkeys.ShowHeatOnScreen();
        }

        internal static void SetHeatLevel(int level)
        {
            int points = level switch
            {
                1 => HitmanConfig.HeatLevel1.Value,
                2 => HitmanConfig.HeatLevel2.Value,
                3 => HitmanConfig.HeatLevel3.Value,
                _ => 0,
            };
            SetHeat(points);
        }

        internal static void VanillaEnforcers()
        {
            var home = Player.Instance.home;
            if (home == null)
            {
                Say("Você não tem casa.", "You have no home.");
                return;
            }
            GameplayController.Instance.CallEnforcers(home, false, false, 0f);
            Say("Enforcers chamados para a sua casa (reação normal do jogo).", "Enforcers called to your home (the game's own response).");
        }

        // ---- corrupt cop ----

        internal static void TeleportToCop()
        {
            var cop = CorruptCop.DevCop();
            TeleportTo(cop?.currentNode, Text.L($"policial {Who(cop)}", $"cop {Who(cop)}"));
        }

        internal static void TeleportToDrop()
        {
            var phone = CorruptCop.DevDropPhone();
            if (phone == null)
            {
                Say("Não há envelope pendente.", "No pending envelope.");
                return;
            }
            TeleportTo(phone.node, Text.L("orelhão do envelope", "envelope payphone"));
        }

        internal static void PayEnvelope()
        {
            if (CorruptCop.DevPayNow()) Say("Envelope pago.", "Envelope paid.");
            else Say("Sem envelope pendente ou sem dinheiro.", "No pending envelope, or not enough money.");
        }

        // ---- player ----

        internal static void Heal()
        {
            var p = Player.Instance;
            p.ResetHealthToMaximum();
            p.ResetNerveToMaximum();
            Say("Vida e nervos cheios.", "Health and nerve full.");
        }

        internal static void AddMoney(int amount) => GameplayController.Instance.AddMoney(amount, true, "Dev menu");

        // Not wanted anywhere, nobody chasing or remembering the player. Heat level 3 sets "wanted" again every minute.
        internal static void ClearWanted()
        {
            int buildings = 0;
            foreach (var b in Each(CityBuildings.Instance != null ? CityBuildings.Instance.buildingDirectory : null))
            {
                if (b == null || b.wantedInBuilding <= 0f) continue;
                b.wantedInBuilding = 0f;
                buildings++;
            }
            int citizens = Tranquilizer.ForgetEveryone();
            try { StatusController.Instance?.ForceStatusCheck(false); } catch { }
            Say($"Procurado limpo em {buildings} prédio(s); {citizens} cidadãos esqueceram você." + (H.Level >= 3 ? " (Heat nível 3 volta a marcar.)" : ""),
                $"Wanted cleared in {buildings} building(s); {citizens} citizens forgot you." + (H.Level >= 3 ? " (Heat level 3 sets it again.)" : ""));
        }

        internal static void TeleportHome()
        {
            var home = Player.Instance.home;
            TeleportTo(home != null ? Player.Instance.FindSafeTeleport(home, false, true) : null, Text.L("casa", "home"));
        }

        internal static void WhereAmI()
        {
            var p = Player.Instance;
            var pos = p.transform.position;
            Plugin.Logger.LogInfo($"[Menu] Player at {N(p.currentGameLocation)} / {N(p.currentBuilding)} room={N(p.currentRoom)} pos=({pos.x:0.0}, {pos.y:0.0}, {pos.z:0.0}) gameTime={SessionData.Instance.gameTime:0.00}");
            Say($"Você está em {N(p.currentGameLocation)} ({N(p.currentBuilding)}), {Text.Clock(SessionData.Instance.gameTime)}.",
                $"You are at {N(p.currentGameLocation)} ({N(p.currentBuilding)}), {Text.Clock(SessionData.Instance.gameTime)}.");
        }

        // ---- time ----

        internal static void SkipHours(float hours)
        {
            var s = SessionData.Instance;
            s.SetGameTime(s.gameTime + hours, s.leapYearCycle);
            Say($"Relógio adiantado {hours:0} h: agora {Text.Clock(s.gameTime)}.", $"Clock moved {hours:0} h ahead: now {Text.Clock(s.gameTime)}.");
        }

        internal static void TimeSpeed(SessionData.TimeSpeed speed)
        {
            SessionData.Instance.SetTimeSpeed(speed);
            Say($"Velocidade do tempo: {speed}.", $"Time speed: {speed}.");
        }

        internal static void TeleportToHuman(Human h, string what) => TeleportTo(h?.currentNode, $"{what} {Who(h)}");

        // Outside the front door of someone's home (the hallway or the street), like the black markets' door.
        internal static void TeleportOutsideHome(Human h, string what)
        {
            if (h == null)
            {
                Say($"{what}: ainda não foi escolhido.", $"{what}: not picked yet.");
                return;
            }
            var home = h.home;
            if (home == null)
            {
                Say($"{what} {Who(h)} não tem casa.", $"{what} {Who(h)} has no home.");
                return;
            }
            var door = BlackMarkets.Outside(home);
            TeleportTo(door, Text.L($"porta da casa de {what} {Who(h)} ({N(home)}, {N(home.building)})", $"front door of {what} {Who(h)} ({N(home)}, {N(home.building)})"));
        }

        internal static void PostPreset(string preset)
        {
            var job = Postings.Request(preset, "dev", j => Say($"Contrato #{j.jobID} postado em {Place(j.post)}.", $"Contract #{j.jobID} posted at {Place(j.post)}."), out var failure);
            if (job == null) Say($"Não deu para postar '{preset}': {failure}.", $"Could not post '{preset}': {failure}.");
        }

        // ---- teleport ----

        internal static void TeleportTo(NewNode node, string what)
        {
            if (node == null)
            {
                Say($"Não achei: {what}.", $"Not found: {what}.");
                return;
            }
            Player.Instance.Teleport(node, null, true, false, true);
            Say($"Teleportado: {what}.", $"Teleported: {what}.");
        }
    }
}
