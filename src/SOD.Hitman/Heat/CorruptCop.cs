using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SOD.Common;
using SOD.Common.Helpers.DialogObjects;
using SODHitman.Contracts;
using static SODHitman.Dev.Describe;

namespace SODHitman.Heat
{
    // Spec 11.1, decided 19/10/2026:
    //  - an existing Enforcer, picked when the save starts and kept in the save; replaced CopReplaceDays after dying
    //  - calls first when heat reaches level 2: the player's apartment, then a payphone near the player if unanswered,
    //    again every CopRetryHours until answered; a replacement cop calls to introduce himself
    //  - the player calls his number to buy one of three packages (one death, half, everything with a discount),
    //    price per point = 50% of the average contract per death, x2 under a warrant
    //  - the money goes into an envelope left at a dead drop (a payphone) before a deadline
    internal static class CorruptCop
    {
        private static HeatState H => ContractStore.Heat;

        // content/Heat/DDSContent: the tree the cop speaks when he calls, one message with one block whose text is
        // set right before each call.
        internal const string CallTreeId = "89391e96-ae62-48fb-8b68-45a870b3f8e1";
        internal const string CallLineBlockId = "1cd8cd1b-d77a-417c-a5a1-ba0930bb74ee";

        // The player calling his number: the greeting is the cop's answer, then the package options.
        private static DialogObject greeting;
        private static DialogResponse greetingResponse;
        // Each package as a phone option and as a face-to-face one (PhoneCalls.DialogIsRemote).
        private static readonly List<PackageLogic> PackageOptions = new();

        private static bool sessionReady;
        private static TelephoneController.PhoneCall ringingCall;
        private static bool ringingAtHome;
        private static bool ringingAnswered;
        private static bool warnedNoMoney;

        // Offer being made right now (chosen before the reply text is spoken). On the phone the money goes into an
        // envelope at a payphone; face to face it changes hands on the spot.
        private enum Offer { None, Nothing, NoMoney, Envelope, Paid }
        private static Offer offer;
        private static int offerCost, offerDrop = -1;
        private static float offerDeadline;

        // ---- registration (plugin load) ----

        internal static void Register()
        {
            if (!HitmanConfig.CopEnabled.Value) return;

            greetingResponse = new DialogResponse(GreetingLine, r =>
            {
                r.isSuccessful = true;
                r.endsDialog = false;
            });
            greeting = Lib.Dialogs.Builder("HitmanCopGreeting")
                .SetText("Hello?")
                .ModifyDialogOptions(p =>
                {
                    p.defaultOption = false;
                    p.telephoneCallOption = false;
                })
                .SetDialogLogic(new GreetingLogic())
                .AddResponse(greetingResponse)
                .CreateAndRegister();
            PhoneDialogs.Add(greeting);

            for (int i = 0; i < 3; i++)
                foreach (bool phone in new[] { true, false })
                {
                    int kind = i;
                    var logic = new PackageLogic(kind, phone);
                    logic.Reply = new DialogResponse(() => PackageReply(kind), r =>
                    {
                        r.isSuccessful = true;
                        r.endsDialog = true;
                    });
                    // defaultOption: SOD.Common only offers custom dialogs listed in the game's default options
                    // (IsDialogShown then shows them only to him, on the phone or face to face).
                    logic.Dialog = Lib.Dialogs.Builder($"HitmanCopPackage{i}{(phone ? "" : "P")}")
                        .SetText("...")
                        .ModifyDialogOptions(p => PhoneDialogs.MenuOption(p, phone))
                        .SetDialogLogic(logic)
                        .AddResponse(logic.Reply)
                        .CreateAndRegister();
                    PhoneDialogs.Add(logic.Dialog);
                    PackageOptions.Add(logic);
                }
            Plugin.Logger.LogInfo("[Cop] Phone dialogs registered.");
        }

        internal static void ResetSession()
        {
            sessionReady = false;
            ringingCall = null;
            ringingAnswered = false;
            offerDrop = -1;
        }

        // ---- setup per save ----

        private static void EnsureReady()
        {
            if (sessionReady || greeting == null || CityData.Instance == null || TelephoneController.Instance == null) return;
            if (H.CopId < 0) PickCop(replacing: false);
            if (H.CopNumber < 0) H.CopNumber = NewFakeNumber();

            var fake = TelephoneController.Instance.fakeTelephoneDictionary;
            if (greeting != null && fake != null && !fake.ContainsKey(H.CopNumber))
                TelephoneController.Instance.AddFakeNumber(H.CopNumber,
                    new TelephoneController.CallSource(TelephoneController.CallType.fakeOutbound, greeting.Preset));

            Lib.DdsStrings["dds.blocks", greeting.BlockId] = Text.L("Alô?", "Hello?");
            PhoneCalls.SetAnswerer(greeting.Preset, CopHuman); // he takes the calls to his number himself
            if (H.CopContacted) NoteNumber(show: false); // notebook name from 1.2.2 on (was "<invalidstringaddress>")
            sessionReady = true;
            Plugin.Logger.LogInfo($"[Cop] Ready: {H.CopName} #{H.CopId}, number {NumberText}, contacted={H.CopContacted}, next call {(H.CopNextCall >= 0 ? Text.Clock(H.CopNextCall) : "-")}");
        }

        private static void PickCop(bool replacing)
        {
            var citizens = CityData.Instance.citizenDirectory;
            var candidates = Each(citizens)
                .Where(c => c != null && !c.isDead && !c.isPlayer && c.job != null && c.job.preset != null && c.job.preset.name == "Enforcer")
                .Where(c => c.humanID != H.CopId && !H.HunterIds.Contains(c.humanID))
                .ToList();
            if (candidates.Count == 0)
            {
                Plugin.Logger.LogWarning("[Cop] No living Enforcer to pick.");
                return;
            }
            var withPhone = candidates.Where(c => c.home != null && c.home.telephones != null && c.home.telephones.Count > 0).ToList();
            var pool = withPhone.Count > 0 ? withPhone : candidates;
            var cop = pool[new Random().Next(pool.Count)];

            if (replacing)
            {
                H.PreviousCopName = H.CopName;
                H.CopAnnounceReplacement = true;
            }
            H.CopId = cop.humanID;
            H.CopName = cop.citizenName;
            Plugin.Logger.LogInfo($"[Cop] Picked {Who(cop)} (home {N(cop.home)}), replacing={replacing}");
        }

        private static int NewFakeNumber()
        {
            var phones = CityData.Instance.phoneDictionary;
            var fake = TelephoneController.Instance.fakeTelephoneDictionary;
            var rng = new Random();
            for (int i = 0; i < 100; i++)
            {
                int n = rng.Next(6000000, 6999999);
                if ((phones == null || !phones.ContainsKey(n)) && (fake == null || !fake.ContainsKey(n))) return n;
            }
            return 6123456;
        }

        private static string NumberText => H.CopNumber < 0 ? "?" : H.CopNumber.ToString().Insert(3, "-");

        private static Human CopHuman()
        {
            Human cop = null;
            return H.CopId >= 0 && CityData.Instance != null && CityData.Instance.GetHuman(H.CopId, out cop, false) ? cop : null;
        }

        // ---- lines ----

        // What he says when the player calls him (or talks to him face to face).
        private static string GreetingLine()
        {
            if (H.PendingDropNumber >= 0)
                return Text.L($"Ainda estou esperando o envelope no orelhão em {DropPlace(H.PendingDropNumber)}.",
                              $"I'm still waiting for that envelope at the payphone at {DropPlace(H.PendingDropNumber)}.");
            if (!HasAnything())
                return Text.L("Você está limpo. Me liga quando precisar.", "You're clean. Call me when you need me.");
            return Text.L("Fala. O que você quer que suma do arquivo?", "Talk. What do you need gone from the file?");
        }

        // What he says when he calls the player (the line on the phone; the deal is made by calling him back).
        private static string CallLine()
        {
            if (H.CopAnnounceReplacement)
                return Text.L($"Aqui é {H.CopName}. {H.PreviousCopName} não está mais entre nós... O acordo continua, só que agora comigo. Mesmo número: {NumberText}.",
                              $"This is {H.CopName}. {H.PreviousCopName} isn't with us anymore... The deal stands, only now it's with me. Same number: {NumberText}.");
            if (!H.CopContacted)
                return Text.L($"Aqui é {H.CopName}, da delegacia. Ando lendo uns relatórios sobre umas mortes por aí, e o seu nome aparece demais neles. Papel some, sabe? Por um preço. Anota aí: {NumberText}.",
                              $"This is {H.CopName}, from the precinct. I've been reading reports about some deaths around town, and your name keeps coming up. Paper gets lost, you know? For a price. Write this down: {NumberText}.");
            return Text.L($"É o {H.CopName}. O seu arquivo na delegacia só engorda. Se quiser que alguma coisa suma, me liga no {NumberText} ou me procura.",
                          $"It's {H.CopName}. Your file at the precinct keeps getting fatter. If you want something gone, call me at {NumberText} or come find me.");
        }

        // ---- the packages (decided 20/10/2026, 6th test): from the cheapest and narrowest to the whole file ----
        //  0 witness statements: the reported ones vanish (their heat goes), the pending witnesses never talk
        //  1 camera footage and statements: also every recording of the player (found bodies lose the camera heat,
        //    hidden ones will never bring it)
        //  2 the player's name off the investigation: heat to zero, and nothing on file brings heat again
        // Priced per point taken off now or kept from coming; the third one costs FilePremium more.

        private static readonly string[] PackageNamesPt =
            { "Some com os depoimentos das testemunhas", "Apaga as gravações das câmeras e os depoimentos", "Tira meu nome da investigação" };
        private static readonly string[] PackageNamesEn =
            { "Lose the witness statements", "Wipe the camera footage and the statements", "Take my name off the investigation" };

        internal static int PricePerPoint()
        {
            var payments = ContractStore.All.Where(s => s.Payment > 0).Select(s => s.Payment).ToList();
            double perPoint = payments.Count > 0
                ? 0.5 * payments.Average() / Math.Max(1, HitmanConfig.HeatContractKill.Value)
                : HitmanConfig.CopFallbackPricePerPoint.Value;
            if (H.Level >= 3) perPoint *= HitmanConfig.CopWarrantMultiplier.Value;
            return Math.Max(1, (int)Math.Round(perPoint));
        }

        // Heat a package takes off now, heat it keeps from coming, and its price.
        internal static (int now, int later, int cost) Package(int kind)
        {
            var witnesses = KillEvidence.WitnessValue();
            var cameras = KillEvidence.CameraValue();
            int now, later;
            switch (kind)
            {
                case 0: now = witnesses.now; later = witnesses.later; break;
                case 1: now = witnesses.now + cameras.now; later = witnesses.later + cameras.later; break;
                default: now = H.Points; later = KillEvidence.FutureValue(); break;
            }
            now = Math.Min(now, H.Points);
            double cost = (now + later) * PricePerPoint();
            if (kind == 2) cost *= 1.0 + HitmanConfig.CopFilePremium.Value;
            int rounded = now + later > 0 ? Math.Max(50, (int)Math.Round(cost / 10.0) * 10) : 0;
            return (now, later, rounded);
        }

        private static bool HasAnything() => Package(2) is var p && p.now + p.later > 0;

        private static string PackageLine(int kind)
        {
            var (now, later, cost) = Package(kind);
            string name = Text.L(PackageNamesPt[kind], PackageNamesEn[kind]);
            if (now + later == 0) return Text.L($"{name}. (nada para apagar)", $"{name}. (nothing to erase)");
            if (kind == 2) return Text.L($"{name}. (heat zerado, Cr{cost})", $"{name}. (heat to zero, Cr{cost})");
            string effect = later > 0
                ? Text.L($"heat −{now} agora e −{later} que viria", $"heat −{now} now and −{later} to come")
                : Text.L($"heat −{now}", $"heat −{now}");
            return $"{name}. ({effect}, Cr{cost})";
        }

        private static string PackageReply(int kind) => offer switch
        {
            Offer.Nothing => Text.L("Não tem nada disso contra você no arquivo.", "There's nothing like that against you on file."),
            Offer.NoMoney => Text.L($"Isso custa Cr{offerCost}. Volta quando tiver.", $"That's Cr{offerCost}. Come back when you have it."),
            Offer.Envelope => Text.L($"Fechado. Deixa Cr{offerCost} num envelope no orelhão em {DropPlace(offerDrop)}. Você tem até {Text.Clock(offerDeadline)}.",
                                     $"Deal. Leave Cr{offerCost} in an envelope at the payphone at {DropPlace(offerDrop)}. You have until {Text.Clock(offerDeadline)}."),
            Offer.Paid => kind switch
            {
                0 => Text.L("Passa o dinheiro por baixo da mesa. Amanhã esses depoimentos não existem mais.", "Slide the money under the table. By tomorrow those statements never existed."),
                1 => Text.L("Passa o dinheiro. As fitas vão ter um acidente com um ímã, e os depoimentos somem junto.", "Hand it over. The tapes are going to have an accident with a magnet, and the statements go with them."),
                _ => Text.L("Passa o dinheiro. Pra delegacia, você nunca existiu.", "Hand it over. As far as the precinct knows, you never existed."),
            },
            _ => Text.L("Agora não dá. Me liga depois.", "Not now. Call me later."),
        };

        // Does what the package promises: the envelope was left, or the money changed hands.
        internal static void ApplyPackage(int kind, string how)
        {
            switch (kind)
            {
                case 0:
                {
                    int n = KillEvidence.EraseWitnesses(how);
                    HeatSystem.Change(-n, "corrupt cop: witness statements");
                    Text.Notify($"Os depoimentos contra você sumiram da delegacia. (heat −{n})", $"The witness statements against you vanished from the precinct. (heat −{n})");
                    break;
                }
                case 1:
                {
                    int n = KillEvidence.EraseWitnesses(how) + KillEvidence.EraseCameras(how);
                    HeatSystem.Change(-n, "corrupt cop: camera footage and statements");
                    Text.Notify($"As gravações e os depoimentos contra você sumiram. (heat −{n})", $"The footage and the statements against you are gone. (heat −{n})");
                    break;
                }
                default:
                    KillEvidence.EraseAll(how);
                    HeatSystem.Change(-H.Points, "corrupt cop: name off the investigation");
                    Interrogation.Cancel("the player's name is off the investigation");
                    Text.Notify("Seu nome saiu da investigação. Pra delegacia, você nunca existiu. (heat zerado)", "Your name is off the investigation. As far as the precinct knows, you never existed. (heat cleared)");
                    break;
            }
        }

        // ---- calls ----

        internal static void OnHeatLevelUp(int level)
        {
            if (!HitmanConfig.CopEnabled.Value || level < 2 || H.CopContacted || H.CopNextCall >= 0f) return;
            H.CopNextCall = Text.Now + 0.25f;
            Plugin.Logger.LogInfo($"[Cop] Heat level {level}: first call at {Text.Clock(H.CopNextCall)}");
        }

        internal static void CallNow()
        {
            EnsureReady();
            if (!sessionReady) return;
            H.CopNextCall = Text.Now;
            ringingCall = null;
            StartCall(home: true);
        }

        private static void StartCall(bool home)
        {
            var cop = CopHuman();
            var player = Player.Instance;
            if (cop == null || cop.isDead || player == null || TelephoneController.Instance == null) return;

            var to = home ? HomePhone(player) : NearestPayphone(player);
            if (to == null)
            {
                if (home)
                {
                    StartCall(home: false);
                    return;
                }
                H.CopNextCall = Text.Now + HitmanConfig.CopRetryHours.Value;
                Plugin.Logger.LogInfo($"[Cop] No phone to reach the player; retry at {Text.Clock(H.CopNextCall)}");
                return;
            }
            var from = CopPhone(cop, to);
            if (from == null)
            {
                Plugin.Logger.LogWarning("[Cop] No phone to call from.");
                H.CopNextCall = Text.Now + HitmanConfig.CopRetryHours.Value;
                return;
            }

            // The game's way of calling the player (PhoneCalls): the voice on the line says the greeting.
            ringingCall = PhoneCalls.CallPlayer(from, to, CallLine());
            ringingAtHome = home;
            ringingAnswered = false;
            if (ringingCall != null) NoteNumber(); // decided 20/10/2026: his number goes into the notebook on his first call
            Plugin.Logger.LogInfo($"[Cop] Calling the player at {(home ? "home" : "a payphone")} {to.numberString} ({Place(to.interactable)}) from {from.numberString}; call created={ringingCall != null}");
        }

        internal static Telephone HomePhone(Player player)
        {
            var phones = player.home != null ? player.home.telephones : null;
            return phones != null && phones.Count > 0 ? phones[0] : null;
        }

        private static Telephone NearestPayphone(Player player)
        {
            var dict = CityData.Instance != null ? CityData.Instance.phoneDictionary : null;
            if (dict == null) return null;
            Telephone best = null;
            float bestDistance = HitmanConfig.CopPayphoneRadius.Value;
            var position = player.transform.position;
            foreach (var kv in dict)
            {
                var t = kv.Value;
                if (!IsPayphone(t)) continue;
                float d = (t.interactable.wPos - position).magnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = t;
                }
            }
            return best;
        }

        internal static Telephone CopPhone(Human cop, Telephone avoid)
        {
            var phones = cop.home != null ? cop.home.telephones : null;
            if (phones != null && phones.Count > 0) return phones[0];
            var dict = CityData.Instance.phoneDictionary;
            foreach (var kv in dict)
                if (IsPayphone(kv.Value) && kv.Value.number != avoid.number) return kv.Value;
            return null;
        }

        private static bool IsPayphone(Telephone t) =>
            t != null && t.interactable != null && t.interactable.preset != null && t.interactable.preset.isPayphone;

        // The notebook shows textOverride as a key of the evidence.generic strings (content/Heat: "Policial Corrupto").
        internal const string NotebookKey = "hitman_corruptcop";

        private static void NoteNumber(bool show = true)
        {
            try { GameplayController.Instance.AddOrMergePhoneNumberData(H.CopNumber, false, null, NotebookKey, show); }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Cop] Could not add the number to the notebook: {e.Message}"); }
        }

        private static void MarkContacted(string how)
        {
            ringingAnswered = true;
            if (H.CopContacted && !H.CopAnnounceReplacement) return;
            H.CopContacted = true;
            H.CopAnnounceReplacement = false;
            H.CopNextCall = -1f;
            NoteNumber();
            Plugin.Logger.LogInfo($"[Cop] Player answered ({how}); number {NumberText} in the notebook.");
        }

        private static void OnCallFinished(TelephoneController.PhoneCall call)
        {
            bool answered = ringingAnswered || (Player.Instance != null && call.receiver == Player.Instance.humanID);
            if (answered) MarkContacted("call ended");
            ringingCall = null;
            if (answered) return;

            if (ringingAtHome)
            {
                Plugin.Logger.LogInfo("[Cop] Apartment phone not answered, trying a payphone near the player.");
                StartCall(home: false);
            }
            else
            {
                H.CopNextCall = Text.Now + HitmanConfig.CopRetryHours.Value;
                Plugin.Logger.LogInfo($"[Cop] Not answered; next attempt at {Text.Clock(H.CopNextCall)}");
            }
        }

        // A call to his number has no telephone at the other end (PhoneCalls): it is known by its greeting, and by him
        // taking it.
        internal static bool InCopCall()
        {
            var calls = TelephoneController.Instance != null ? TelephoneController.Instance.activeCalls : null;
            if (calls == null || H.CopNumber < 0) return false;
            string greetingName = greeting != null && greeting.Preset != null ? greeting.Preset.name : null;
            foreach (var call in Each(calls))
            {
                if (call == null || call.state != TelephoneController.CallState.started) continue;
                if (call.to == H.CopNumber || call.from == H.CopNumber || call.caller == H.CopId || call.receiver == H.CopId) return true;
                if (greetingName != null && PhoneCalls.GreetingOf(call) == greetingName) return true;
            }
            return false;
        }

        private static bool IsCop(Citizen who) => who != null && H.CopId >= 0 && who.humanID == H.CopId;

        // ---- dead drop ----

        private static int PickDrop()
        {
            var dict = CityData.Instance != null ? CityData.Instance.phoneDictionary : null;
            var player = Player.Instance;
            if (dict == null || player == null) return -1;
            var position = player.transform.position;
            var payphones = new System.Collections.Generic.List<(int number, float distance)>();
            foreach (var kv in dict)
                if (IsPayphone(kv.Value)) payphones.Add((kv.Value.number, (kv.Value.interactable.wPos - position).magnitude));
            var away = payphones.Where(p => p.distance > 60f && p.distance < 400f).ToList();
            var pool = away.Count > 0 ? away : payphones;
            return pool.Count == 0 ? -1 : pool[new Random().Next(pool.Count)].number;
        }

        private static string DropPlace(int number)
        {
            if (CityData.Instance != null && CityData.Instance.phoneDictionary.TryGetValue(number, out var t) && t != null)
                return Place(t.interactable);
            return "?";
        }

        private static void TickDrop(float now)
        {
            if (H.PendingDropNumber < 0) return;
            if (now > H.PendingDeadline)
            {
                Plugin.Logger.LogInfo("[Cop] Dead drop expired.");
                Text.Notify("O policial cansou de esperar pelo envelope. O acordo caiu.", "The cop got tired of waiting for the envelope. The deal is off.");
                ClearPending();
                return;
            }
            var player = Player.Instance;
            if (player == null || !CityData.Instance.phoneDictionary.TryGetValue(H.PendingDropNumber, out var drop) || drop?.interactable == null) return;
            if ((drop.interactable.wPos - player.transform.position).magnitude > HitmanConfig.CopDropRadius.Value)
            {
                warnedNoMoney = false;
                return;
            }

            var gameplay = GameplayController.Instance;
            if (gameplay.money < H.PendingCost)
            {
                if (!warnedNoMoney)
                    Text.Notify($"Você não tem Cr{H.PendingCost} para o envelope.", $"You don't have Cr{H.PendingCost} for the envelope.");
                warnedNoMoney = true;
                return;
            }

            gameplay.AddMoney(-H.PendingCost, true, "Corrupt cop");
            Plugin.Logger.LogInfo($"[Cop] Envelope left: Cr{H.PendingCost} for package {H.PendingKind}.");
            Text.Notify("Você deixou o envelope. Alguns papéis vão sumir da delegacia.", "You left the envelope. Some paperwork is going to go missing.");
            Deliver("envelope");
        }

        // The paid envelope's package. Saves from before 1.2.2 hold points instead.
        private static void Deliver(string how)
        {
            int kind = H.PendingKind, points = H.PendingPoints;
            ClearPending();
            if (kind >= 0) ApplyPackage(kind, how);
            else HeatSystem.Change(-points, "corrupt cop");
        }

        private static void ClearPending()
        {
            H.PendingPoints = 0;
            H.PendingKind = -1;
            H.PendingCost = 0;
            H.PendingDropNumber = -1;
            H.PendingDeadline = -1f;
        }

        // ---- per game minute ----

        internal static void Tick()
        {
            if (!HitmanConfig.CopEnabled.Value) return;
            EnsureReady();
            if (!sessionReady) return;
            float now = Text.Now;

            var cop = CopHuman();
            if (cop == null || cop.isDead)
            {
                if (H.CopReplaceAt < 0f)
                {
                    H.CopReplaceAt = now + HitmanConfig.CopReplaceDays.Value * 24f;
                    if (H.PendingDropNumber >= 0)
                    {
                        Text.Notify("O seu contato na polícia morreu. O acordo do envelope acabou.", "Your police contact is dead. The envelope deal is off.");
                        ClearPending();
                    }
                    Plugin.Logger.LogInfo($"[Cop] {H.CopName} is gone; a replacement at {Text.Clock(H.CopReplaceAt)} (+{HitmanConfig.CopReplaceDays.Value} days)");
                }
                else if (now >= H.CopReplaceAt)
                {
                    H.CopReplaceAt = -1f;
                    PickCop(replacing: true);
                    if (H.CopContacted) H.CopNextCall = now;
                }
                return;
            }

            if (ringingCall == null && (!H.CopContacted || H.CopAnnounceReplacement) && H.CopNextCall >= 0f && now >= H.CopNextCall)
                StartCall(home: true);

            TickDrop(now);
        }

        // ---- dev menu ----

        internal static Human DevCop() => CopHuman();

        internal static Interactable DevDropPhone() =>
            H.PendingDropNumber >= 0 && CityData.Instance != null && CityData.Instance.phoneDictionary.TryGetValue(H.PendingDropNumber, out var t) && t != null
                ? t.interactable : null;

        // Leaves the pending envelope without walking to the payphone. False when there is none or not enough money.
        internal static bool DevPayNow()
        {
            if (H.PendingDropNumber < 0 || GameplayController.Instance == null) return false;
            if (GameplayController.Instance.money < H.PendingCost) return false;
            GameplayController.Instance.AddMoney(-H.PendingCost, true, "Corrupt cop");
            Plugin.Logger.LogInfo($"[Cop] Envelope paid from the dev menu: Cr{H.PendingCost} for package {H.PendingKind}.");
            Deliver("dev menu");
            return true;
        }

        internal static void DevMarkContacted()
        {
            EnsureReady();
            if (sessionReady) MarkContacted("dev menu");
        }

        // ---- dialog logic ----

        private sealed class GreetingLogic : IDialogLogic
        {
            public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef) => false;

            public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
            {
                Lib.DdsStrings["dds.blocks", greetingResponse.BlockId] = GreetingLine();
                return DialogController.ForceSuccess.success;
            }

            public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
            {
                if (ringingCall != null) MarkContacted("greeting");
            }
        }

        // Far from the player: a call, or a conversation the phone turned into (the money goes into an envelope).
        private static bool CopFar()
        {
            var cop = CopHuman();
            var player = Player.Instance;
            return cop == null || player == null || (cop.transform.position - player.transform.position).magnitude > 8f;
        }

        private sealed class PackageLogic : IDialogLogic
        {
            private readonly int kind;
            private readonly bool phone;
            internal DialogObject Dialog;
            internal DialogResponse Reply;

            public PackageLogic(int kind, bool phone)
            {
                this.kind = kind;
                this.phone = phone;
            }

            // On a call to his number, or face to face with him once he is the player's contact (decided 20/10/2026);
            // the phone form or the face-to-face form, whichever the open conversation is.
            public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (!H.CopContacted || H.PendingDropNumber >= 0 || !HasAnything()) return false;
                if (PhoneCalls.DialogIsRemote() != phone) return false;
                if (!InCopCall() && !IsCop(saysTo)) return false;
                Lib.DdsStrings["dds.blocks", Dialog.BlockId] = PackageLine(kind);
                return true;
            }

            public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
            {
                var (now, later, cost) = Package(kind);
                offerCost = cost;
                offerDrop = -1;
                if (now + later == 0) offer = Offer.Nothing;
                else if (InCopCall() || PhoneCalls.DialogIsRemote() || CopFar())
                {
                    offerDrop = PickDrop();
                    offerDeadline = Text.Now + HitmanConfig.CopDropDeadlineHours.Value;
                    offer = offerDrop >= 0 ? Offer.Envelope : Offer.None;
                }
                else offer = GameplayController.Instance != null && GameplayController.Instance.money >= cost ? Offer.Paid : Offer.NoMoney;
                Lib.DdsStrings["dds.blocks", Reply.BlockId] = PackageReply(kind);
                return DialogController.ForceSuccess.success;
            }

            public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
            {
                var made = offer;
                offer = Offer.None;
                if (made == Offer.Envelope)
                {
                    H.PendingKind = kind;
                    H.PendingPoints = 0;
                    H.PendingCost = offerCost;
                    H.PendingDropNumber = offerDrop;
                    H.PendingDeadline = offerDeadline;
                    Plugin.Logger.LogInfo($"[Cop] Package {kind}: Cr{offerCost} at payphone {offerDrop} ({DropPlace(offerDrop)}) by {Text.Clock(offerDeadline)}");
                    Text.Notify($"Deixe Cr{offerCost} no orelhão em {DropPlace(offerDrop)} até {Text.Clock(offerDeadline)}.",
                                $"Leave Cr{offerCost} at the payphone at {DropPlace(offerDrop)} by {Text.Clock(offerDeadline)}.");
                }
                else if (made == Offer.Paid && GameplayController.Instance != null)
                {
                    GameplayController.Instance.AddMoney(-offerCost, true, "Corrupt cop");
                    Plugin.Logger.LogInfo($"[Cop] Package {kind} paid face to face: Cr{offerCost}");
                    ApplyPackage(kind, "paid face to face");
                }
                offerDrop = -1;
            }
        }

        // Ringing -> answered (started) or missed (ended without the player picking up).
        [HarmonyPatch(typeof(TelephoneController.PhoneCall), nameof(TelephoneController.PhoneCall.SetCallState))]
        internal static class PhoneCall_SetCallState
        {
            static void Postfix(TelephoneController.PhoneCall __instance, TelephoneController.CallState newState)
            {
                if (ringingCall == null || __instance == null || __instance.Pointer != ringingCall.Pointer) return;
                Plugin.Logger.LogInfo($"[Cop] Call state -> {newState} (receiver {__instance.receiver})");
                if (newState == TelephoneController.CallState.started) MarkContacted("picked up");
                else if (newState == TelephoneController.CallState.ended || newState == TelephoneController.CallState.denied) OnCallFinished(__instance);
            }
        }
    }
}
