using System;
using System.Collections.Generic;
using SOD.Common;
using SOD.Common.Helpers.DialogObjects;
using SODHitman.Contracts;
using UnityEngine;
using static SODHitman.Dev.Describe;

namespace SODHitman.Heat
{
    // Decided 19/10/2026: at heat level 2 the Enforcer visit is an interrogation at the player's home, without a search.
    //  - the visit waits until the player is at home; still away AvoidHours after it is due counts as avoiding it
    //  - the first Enforcer of the call who reaches the player asks two questions: where the player was at the time of
    //    the last kill, and whether they knew the victim
    //  - each answer is credible (-1), neutral (0) or suspicious (+1) from what the police could know: a witness who
    //    reported or a camera that caught the last kill (KillEvidence), the victim living in the player's building
    //  - v0.7: when the police hold such evidence, a third question asks what the player was doing at the scene
    //  - total <= -1 lowers heat, 0 changes nothing, >= 1 raises it per point; then the call is closed so the
    //    Enforcers leave instead of searching the apartment
    internal static class Interrogation
    {
        private static HeatState H => ContractStore.Heat;

        // Spoken by the Enforcer when he reaches the player (content/Heat/DDSContent, text set right before).
        private const string OpenerBlockId = "bef4572f-1b03-4516-876c-a1e145226518";

        private sealed class Answer
        {
            public int Question;            // 1 or 2
            public string Pt, En;            // the player's line
            public Func<int> Score;
            public Func<int, string> Reaction; // the Enforcer's reply, given this answer's score
            public DialogObject Dialog;
        }

        private static readonly List<Answer> Answers = new();
        private static string reply;     // the reply to the answer being chosen (read by the response text getter)
        private static int replyScore;

        // ---- registration (plugin load) ----

        internal static void Register()
        {
            if (!HitmanConfig.InterrogationEnabled.Value) return;

            // Question 1: where were you at the time of the last kill?
            Add(1, "Em casa, dormindo.", "At home, asleep.",
                () => KillEvidence.LastKillKnown() ? 1 : -1,
                s => s > 0
                    ? Text.L("Engraçado. Tem gente que diz ter visto alguém como você por lá.", "Funny. Some people say they saw someone like you there.")
                    : Text.L("Certo. Anotado.", "Right. Noted."));
            Add(1, "Trabalhando num caso, do outro lado da cidade.", "Working a case, across town.",
                () => 0,
                _ => Text.L("Detetive, é? Vamos conferir isso.", "A detective, huh? We'll check on that."));
            Add(1, "Não é da sua conta.", "That's none of your business.",
                () => 1,
                _ => Text.L("Resposta errada, amigo.", "Wrong answer, pal."));

            // Question 2: did you know the victim?
            Add(2, "Nunca ouvi falar.", "Never heard of them.",
                () => H.LastVictimNeighbour ? 1 : -1,
                s => s > 0
                    ? Text.L("Morava no seu prédio e você nunca ouviu falar? Sei.", "They lived in your building and you never heard of them? Sure.")
                    : Text.L("Tudo bem.", "All right."));
            Add(2, "Só de vista, do bairro.", "Only by sight, from the neighbourhood.",
                () => H.LastVictimNeighbour ? -1 : 0,
                s => s < 0
                    ? Text.L("Faz sentido, era seu vizinho.", "Makes sense, they were your neighbour.")
                    : Text.L("Hum.", "Hm."));
            Add(2, "Quero um advogado.", "I want a lawyer.",
                () => 1,
                _ => Text.L("Como quiser.", "Have it your way."));

            // Question 3 (v0.7), only when the police hold evidence: a witness who talked or camera footage.
            Add(3, "Só passando. Eu ando muito por lá.", "Just passing by. I'm around there a lot.",
                () => 0,
                _ => Text.L("Muita coincidência, mas tudo bem.", "Quite the coincidence, but fine."));
            Add(3, "Não era eu.", "That wasn't me.",
                () => LastCamera() ? 1 : 0,
                s => s > 0
                    ? Text.L("A câmera diz outra coisa.", "The camera says otherwise.")
                    : Text.L("É a sua palavra contra a da testemunha.", "It's your word against the witness's."));
            Add(3, "Quero um advogado.", "I want a lawyer.",
                () => 1,
                _ => Text.L("Você vai precisar de um.", "You're going to need one."));

            Plugin.Logger.LogInfo($"[Interrogation] {Answers.Count} answer options registered.");
        }

        private static bool LastCamera() => H.KillRecords != null && H.KillRecords.Count > 0 && H.KillRecords[^1].Camera;

        private static string Question3()
        {
            var place = H.KillRecords != null && H.KillRecords.Count > 0 ? H.KillRecords[^1].Place : null;
            string what = LastCamera() ? Text.L("imagens de câmera", "camera footage") : Text.L("uma testemunha", "a witness");
            return string.IsNullOrEmpty(place)
                ? Text.L($"Temos {what} que te colocam na cena. O que você fazia lá?", $"We have {what} putting you at the scene. What were you doing there?")
                : Text.L($"Temos {what} que te colocam em {place}. O que você fazia lá?", $"We have {what} putting you at {place}. What were you doing there?");
        }

        private static void Add(int question, string pt, string en, Func<int> score, Func<int, string> reaction)
        {
            var answer = new Answer { Question = question, Pt = pt, En = en, Score = score, Reaction = reaction };
            answer.Dialog = Lib.Dialogs.Builder($"HitmanAsk{question}{(char)('A' + Answers.Count % 3)}")
                // Question 2 keeps the conversation open when a question 3 follows.
                .SetText(en)
                // defaultOption: SOD.Common only offers custom dialogs listed in the game's default options; with
                // false (up to 1.2.0) the answers never showed up. IsDialogShown keeps them to the interrogator.
                .ModifyDialogOptions(p =>
                {
                    p.defaultOption = true;
                    p.telephoneCallOption = false;
                    p.ranking = PhoneDialogs.Ranking; // on top of the game's options
                })
                .SetDialogLogic(new Logic(answer))
                .AddResponse(new DialogResponse(() => reply ?? "...", r =>
                {
                    r.isSuccessful = true;
                    r.endsDialog = question == 3;
                }))
                .CreateAndRegister();
            Answers.Add(answer);
        }

        // ---- lines ----

        private static string Question1()
        {
            float t = H.LastKillTime;
            if (t < 0f) return Text.L("Onde você estava ontem à noite?", "Where were you last night?");
            int days = (int)Math.Floor(Text.Now / 24f) - (int)Math.Floor(t / 24f);
            string pt = days <= 0 ? "hoje" : days == 1 ? "ontem" : $"há {days} dias";
            string en = days <= 0 ? "today" : days == 1 ? "yesterday" : $"{days} days ago";
            return Text.L($"Onde você estava {pt}, por volta das {Text.Clock(t)}?", $"Where were you {en}, around {Text.Clock(t)}?");
        }

        private static string Question2() =>
            string.IsNullOrEmpty(H.LastVictimName)
                ? Text.L("Você conhecia a vítima do último caso?", "Did you know the victim of the last case?")
                : Text.L($"Você conhecia {H.LastVictimName}?", $"Did you know {H.LastVictimName}?");

        private static string Opener() =>
            Text.L("Polícia. Estamos investigando umas mortes no bairro e temos umas perguntas pra você. ",
                   "Police. We're looking into some deaths in the neighbourhood and we have a few questions for you. ")
            + Question1();

        private static string Conclusion(int total) =>
            total <= -1 ? Text.L("Obrigado pela colaboração. Se lembrar de alguma coisa, procure a delegacia.",
                                 "Thanks for your cooperation. If you remember anything, come by the precinct.")
            : total == 0 ? Text.L("Vamos ficar de olho em você. Não saia da cidade.", "We'll be keeping an eye on you. Don't leave town.")
            : Text.L("Você não me convenceu. Isso não acaba aqui.", "You didn't convince me. This isn't over.");

        // ---- visit ----

        // A level 2 investigation is due: wait for the player to be at home.
        internal static void Schedule(float now)
        {
            if (H.VisitDue >= 0f || H.VisitCallTime >= 0f || H.InterrogatorId >= 0) return;
            H.VisitDue = now;
            H.VisitForced = false;
            Plugin.Logger.LogInfo($"[Interrogation] Visit due at {Text.Clock(now)}; waiting for the player to be at home.");
        }

        // Dev hotkey: a visit right now, whatever the heat level.
        internal static void ForceVisit()
        {
            Reset(keepDue: false);
            H.VisitDue = Text.Now;
            H.VisitForced = true;
            Plugin.Logger.LogInfo("[Interrogation] Visit forced (dev hotkey).");
        }

        internal static void DevCancel() => Cancel("dev menu");

        // No visit any more (the corrupt cop took the player's name off the investigation, or the dev menu).
        internal static void Cancel(string why)
        {
            var player = Player.Instance;
            if (player != null && (H.InterrogatorId >= 0 || H.VisitCallTime >= 0f)) CloseCall(player);
            Reset(keepDue: false);
            Plugin.Logger.LogInfo($"[Interrogation] Visit cancelled ({why}).");
        }

        internal static string Status()
        {
            if (H.InterrogatorId >= 0) return Text.L($"interrogando (Enforcer #{H.InterrogatorId}, pergunta {H.InterrogationStep + 1}, pontos {H.InterrogationScore})",
                                                     $"interrogating (Enforcer #{H.InterrogatorId}, question {H.InterrogationStep + 1}, score {H.InterrogationScore})");
            if (H.VisitCallTime >= 0f) return Text.L($"Enforcers a caminho desde {Text.Clock(H.VisitCallTime)}", $"Enforcers on the way since {Text.Clock(H.VisitCallTime)}");
            if (H.VisitDue >= 0f) return Text.L($"visita pendente desde {Text.Clock(H.VisitDue)} (vá para casa)", $"visit due since {Text.Clock(H.VisitDue)} (go home)");
            return Text.L("nenhuma", "none");
        }

        private static bool AtHome(Player player) =>
            player.home != null && (player.isHome || player.currentGameLocation == player.home);

        internal static void Tick(Player player, float now)
        {
            if (!HitmanConfig.InterrogationEnabled.Value) return;
            bool allowed = H.VisitForced || H.Level == 2;
            if (!allowed)
            {
                // Level 3 means a warrant (the game's own response); below 2 there is nothing to investigate.
                if (H.VisitDue >= 0f || H.InterrogatorId >= 0 || H.VisitCallTime >= 0f)
                {
                    Plugin.Logger.LogInfo($"[Interrogation] Visit dropped: heat level is now {H.Level}.");
                    if (H.InterrogatorId >= 0 || H.VisitCallTime >= 0f) CloseCall(player);
                    Reset(keepDue: false);
                }
                return;
            }

            if (H.InterrogatorId >= 0) TickInterrogation(player, now);
            else if (H.VisitCallTime >= 0f) TickArrival(player, now);
            else if (H.VisitDue >= 0f) TickPending(player, now);
        }

        // Where the Enforcers are sent: the hallway (or street) on the other side of the apartment's door. A call on the
        // apartment itself makes the game's response go in and search it (seen in the 1.2.0 test).
        private static NewGameLocation callLocation;

        private static NewGameLocation Doorstep(NewAddress home)
        {
            if (home == null) return null;
            NewGameLocation street = null;
            foreach (var access in Each(home.entrances))
            {
                if (access == null || access.door == null) continue;
                foreach (var node in new[] { access.fromNode, access.toNode })
                {
                    var place = node != null ? node.gameLocation : null;
                    if (place == null || place == home) continue;
                    if (!place.isOutside) return place;
                    street ??= place;
                }
            }
            return street;
        }

        private static void TickPending(Player player, float now)
        {
            if (AtHome(player) && GameplayController.Instance != null)
            {
                var doorstep = Doorstep(player.home);
                callLocation = doorstep ?? player.home;
                GameplayController.Instance.CallEnforcers(callLocation, false, false, 0f);
                H.VisitCallTime = now;
                Plugin.Logger.LogInfo($"[Interrogation] Player at home ({N(player.home)}): Enforcers called to {N(callLocation)}{(doorstep == null ? " (no doorstep found)" : "")}.");
                Text.Notify("Enforcers estão vindo à sua porta para fazer umas perguntas. Fique em casa e fale com eles.",
                            "Enforcers are coming to your door with a few questions. Stay at home and talk to them.");
                return;
            }
            if (now - H.VisitDue > HitmanConfig.InterrogationAvoidHours.Value)
            {
                Plugin.Logger.LogInfo("[Interrogation] The player was never home for the visit.");
                Avoided(player, "Os Enforcers passaram no seu apartamento várias vezes e nunca te encontraram. Isso pega mal.",
                                "The Enforcers came by your apartment several times and never found you. That looks bad.");
            }
        }

        private static void TickArrival(Player player, float now)
        {
            var enforcer = FindInterrogator(player);
            if (enforcer != null)
            {
                Start(enforcer, player, now);
                return;
            }
            if (now - H.VisitCallTime > HitmanConfig.InterrogationArrivalHours.Value)
            {
                Plugin.Logger.LogInfo("[Interrogation] No Enforcer reached the player; visit dropped.");
                CloseCall(player);
                Reset(keepDue: false);
            }
        }

        // An Enforcer of this call (or, failing that, any on duty) close to the player in the apartment.
        private static Human FindInterrogator(Player player)
        {
            var position = player.transform.position;
            Human best = null;
            float bestDistance = 12f;

            void Consider(Human h)
            {
                if (h == null || h.isDead || h.isPlayer || h.ai == null || !h.isEnforcer || h.humanID == H.CopId) return;
                if (h.ai.ko || h.isStunned) return;
                float d = (h.transform.position - position).magnitude;
                bool sameRoom = h.currentGameLocation == player.currentGameLocation;
                // At the door: the Enforcer waits in the hallway, the player inside or on the threshold.
                if ((sameRoom || d < 6f) && d < bestDistance)
                {
                    best = h;
                    bestDistance = d;
                }
            }

            var call = Call(player);
            if (call != null && call.response != null)
                foreach (var id in Each(call.response))
                    if (CityData.Instance.GetHuman(id, out var h, false)) Consider(h);
            if (best == null && GameplayController.Instance != null)
                foreach (var h in Each(GameplayController.Instance.enforcers)) Consider(h);
            return best;
        }

        private static void Start(Human enforcer, Player player, float now)
        {
            H.InterrogatorId = enforcer.humanID;
            H.InterrogationStep = 0;
            H.InterrogationScore = 0;
            H.InterrogationStart = now;
            H.InterrogationHasQ3 = KillEvidence.LastKillKnown();
            var opener = Opener();
            Lib.DdsStrings["dds.blocks", OpenerBlockId] = opener;
            try
            {
                enforcer.speechController?.Speak("dds.blocks", OpenerBlockId, false, false, true, 0f, false, default(Color),
                                                 null, false, false, null, null, null, player.interactable);
            }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Interrogation] Speak failed: {e.Message}"); }
            Plugin.Logger.LogInfo($"[Interrogation] {Who(enforcer)} starts the interrogation (last kill seen={H.LastKillSeen}, victim '{H.LastVictimName}', neighbour={H.LastVictimNeighbour}).");
            Text.Notify($"Enforcer: \"{opener}\"", $"Enforcer: \"{opener}\"");
            Text.Notify("Fale com o Enforcer para responder.", "Talk to the Enforcer to answer.");
        }

        private static void TickInterrogation(Player player, float now)
        {
            Human enforcer = null;
            if (!CityData.Instance.GetHuman(H.InterrogatorId, out enforcer, false) || enforcer == null || enforcer.isDead)
            {
                Plugin.Logger.LogInfo("[Interrogation] The Enforcer is gone; interrogation dropped.");
                CloseCall(player);
                Reset(keepDue: false);
                return;
            }
            float distance = (enforcer.transform.position - player.transform.position).magnitude;
            if (!AtHome(player) && distance > 25f)
            {
                Plugin.Logger.LogInfo("[Interrogation] The player walked out.");
                Avoided(player, "Você saiu no meio do interrogatório.", "You walked out in the middle of the interrogation.");
                return;
            }
            if ((now - H.InterrogationStart) * 60f > HitmanConfig.InterrogationWaitMinutes.Value)
            {
                Plugin.Logger.LogInfo("[Interrogation] No answers in time.");
                Avoided(player, "O Enforcer cansou de esperar pelas suas respostas.", "The Enforcer got tired of waiting for your answers.");
            }
        }

        private static void Finish(Player player)
        {
            int total = H.InterrogationScore;
            int delta = total <= -1 ? HitmanConfig.InterrogationConvinced.Value
                      : total == 0 ? 0
                      : HitmanConfig.InterrogationSuspicious.Value * total;
            Plugin.Logger.LogInfo($"[Interrogation] Finished: score {total}, heat {(delta >= 0 ? "+" : "")}{delta}.");
            CloseCall(player);
            Reset(keepDue: false);
            HeatSystem.Change(delta, $"interrogation (score {total})");
            Text.Notify(
                total <= -1 ? $"O Enforcer acreditou em você. (heat {delta})" : total == 0 ? "O Enforcer não se convenceu nem desconfiou. (heat igual)" : $"O Enforcer desconfiou de você. (heat +{delta})",
                total <= -1 ? $"The Enforcer believed you. (heat {delta})" : total == 0 ? "The Enforcer was neither convinced nor suspicious. (heat unchanged)" : $"The Enforcer is suspicious of you. (heat +{delta})");
        }

        private static void Avoided(Player player, string pt, string en)
        {
            CloseCall(player);
            Reset(keepDue: false);
            HeatSystem.Change(HitmanConfig.InterrogationAvoided.Value, "avoided the interrogation");
            Text.Notify(pt + $" (heat +{HitmanConfig.InterrogationAvoided.Value})", en + $" (heat +{HitmanConfig.InterrogationAvoided.Value})");
        }

        private static void Reset(bool keepDue)
        {
            if (!keepDue)
            {
                H.VisitDue = -1f;
                H.VisitForced = false;
            }
            H.VisitCallTime = -1f;
            H.InterrogatorId = -1;
            H.InterrogationStep = 0;
            H.InterrogationScore = 0;
            H.InterrogationStart = -1f;
            callLocation = null;
        }

        private static GameplayController.EnforcerCall Call(Player player)
        {
            var calls = GameplayController.Instance != null ? GameplayController.Instance.enforcerCalls : null;
            var place = callLocation ?? player.home;
            if (calls == null || place == null) return null;
            return calls.TryGetValue(place, out var call) ? call : null;
        }

        // Marks the game's enforcer call on the apartment as done, so the Enforcers leave instead of searching.
        private static void CloseCall(Player player)
        {
            var call = Call(player);
            if (call == null) return;
            Plugin.Logger.LogInfo($"[Interrogation] Enforcer call on {N(player.home)}: {call.state} -> completed.");
            call.state = GameplayController.EnforcerCallState.completed;
        }

        // ---- dialog logic ----

        private sealed class Logic : IDialogLogic
        {
            private readonly Answer answer;

            public Logic(Answer answer) => this.answer = answer;

            public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (saysTo == null || H.InterrogatorId < 0 || saysTo.humanID != H.InterrogatorId) return false;
                if (H.InterrogationStep != answer.Question - 1) return false;
                Lib.DdsStrings["dds.blocks", answer.Dialog.BlockId] = Text.L(answer.Pt, answer.En);
                return true;
            }

            public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
            {
                replyScore = answer.Score();
                int total = H.InterrogationScore + replyScore;
                var next = NextQuestion(answer.Question);
                reply = answer.Reaction(replyScore) + " " + (next ?? Conclusion(total));
                return DialogController.ForceSuccess.success;
            }

            public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
            {
                if (H.InterrogatorId < 0 || H.InterrogationStep != answer.Question - 1) return;
                H.InterrogationScore += replyScore;
                H.InterrogationStep = answer.Question;
                Plugin.Logger.LogInfo($"[Interrogation] Q{answer.Question}: '{answer.En}' scored {replyScore} (total {H.InterrogationScore}).");
                var player = Player.Instance;
                var next = NextQuestion(answer.Question);
                if (next != null) Text.Notify($"Enforcer: \"{next}\"", $"Enforcer: \"{next}\"");
                else if (player != null) Finish(player);
            }
        }

        // Question after the one just answered, or null when the interrogation is over.
        private static string NextQuestion(int answered) => answered switch
        {
            1 => Question2(),
            2 => H.InterrogationHasQ3 ? Question3() : null,
            _ => null,
        };
    }
}
