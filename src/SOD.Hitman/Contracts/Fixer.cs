using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using SOD.Common;
using SOD.Common.Helpers.DialogObjects;
using SODHitman.Heat;
using SODHitman.Weapons;
using static SODHitman.Dev.Describe;

namespace SODHitman.Contracts
{
    // Pillar 3 (v0.9): the Fixer, an underworld middleman, in the mould of the corrupt cop (Heat/CorruptCop).
    //  - a real citizen with a criminal job (gambling den owner, criminal accountant, black market trader...), picked
    //    once per save and kept in it; replaced FixerReplaceDays after dying
    //  - calls the player once their reputation reaches FirstContactReputation, introduces himself, leaves a number
    //  - calling that number: ask for work (an exclusive contract of the best tier unlocked, paying FixerBonus more),
    //    ask how the career is going, or buy gear (the shop list in the cfg)
    internal static class Fixer
    {
        private static HeatState H => ContractStore.Heat;
        private const int ShopSlots = 5;

        private static readonly string[] CriminalJobs =
            { "GamblingDenOwner", "CriminalAccountant", "BlackmarketTrader", "BlackmarketDoctor", "DebtCollector", "WeaponsDealer" };

        private static DialogObject greeting;
        private static DialogResponse greetingResponse;
        private static readonly List<string> ReplyBlocks = new();
        private static string reply = "...";

        // On the phone the mod settles the dialog itself (PhoneDialogs), so SOD.Common never refreshes dynamic text:
        // the reply goes straight into the DDS strings of every option's response (only one is spoken).
        private static string Reply
        {
            set
            {
                reply = value;
                foreach (var id in ReplyBlocks) Lib.DdsStrings["dds.blocks", id] = value;
            }
        }
        private static bool sessionReady;
        private static TelephoneController.PhoneCall ringingCall;

        internal static bool Enabled => HitmanConfig.FixerEnabled.Value;

        // ---- registration (plugin load) ----

        internal static void Register()
        {
            if (!Enabled) return;
            // Decided 20/10/2026 (8th test): on the phone he only says where he is; business is face to face. His line
            // ends the call.
            greetingResponse = new DialogResponse(() => GreetingLine(), r =>
            {
                r.isSuccessful = true;
                r.endsDialog = true;
            });
            greeting = Lib.Dialogs.Builder("HitmanFixerGreeting")
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

            // His menu is face to face only.
            Option("HitmanFixerJobP", new JobLogic(), false);
            Option("HitmanFixerSheetP", new SheetLogic(), false);
            Option("HitmanFixerCampaignP", new CampaignLogic(), false);
            for (int i = 0; i < ShopSlots; i++) Option($"HitmanFixerShop{i}P", new ShopLogic(i), false);
            Plugin.Logger.LogInfo("[Fixer] Dialogs registered.");
        }

        private static void Option(string name, FixerOption logic, bool phone)
        {
            var response = new DialogResponse(() => reply, r =>
            {
                r.isSuccessful = true;
                r.endsDialog = false;
            });
            // defaultOption: SOD.Common only offers custom dialogs listed in the game's default options
            // (IsDialogShown then shows them only to him, on the phone or face to face).
            var d = Lib.Dialogs.Builder(name)
                .SetText("...")
                .ModifyDialogOptions(p => PhoneDialogs.MenuOption(p, phone))
                .SetDialogLogic(logic)
                .AddResponse(response)
                .CreateAndRegister();
            logic.Self = d;
            logic.Phone = phone;
            ReplyBlocks.Add(response.BlockId);
            PhoneDialogs.Add(d);
        }

        // An option of his menu: face to face with him only.
        private abstract class FixerOption : IDialogLogic
        {
            internal DialogObject Self;
            internal bool Phone;

            protected bool Visible(Citizen saysTo) => !PhoneCalls.DialogIsRemote() && FaceToFace(saysTo);
            protected void Label(string text) => Lib.DdsStrings["dds.blocks", Self.BlockId] = text;

            public abstract bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef);
            public abstract DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy);
            public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef) { }
        }

        internal static void ResetSession()
        {
            sessionReady = false;
            ringingCall = null;
        }

        // ---- setup per save ----

        private static void EnsureReady()
        {
            if (sessionReady || greeting == null || CityData.Instance == null || TelephoneController.Instance == null) return;
            if (H.FixerId < 0) Pick(replacing: false);
            if (H.FixerId < 0) return;
            if (H.FixerNumber < 0) H.FixerNumber = NewNumber();
            var fake = TelephoneController.Instance.fakeTelephoneDictionary;
            if (fake != null && !fake.ContainsKey(H.FixerNumber))
                TelephoneController.Instance.AddFakeNumber(H.FixerNumber,
                    new TelephoneController.CallSource(TelephoneController.CallType.fakeOutbound, greeting.Preset));
            Lib.DdsStrings["dds.blocks", greeting.BlockId] = Text.L("Alô?", "Hello?");
            PhoneCalls.SetAnswerer(greeting.Preset, FixerHuman); // he takes the calls to his number himself
            if (H.FixerContacted) NoteNumber(show: false); // notebook name from 1.2.2 on (was "<invalidstringaddress>")
            sessionReady = true;
            Plugin.Logger.LogInfo($"[Fixer] Ready: {H.FixerName} #{H.FixerId}, number {NumberText}, contacted={H.FixerContacted}");
        }

        private static void Pick(bool replacing)
        {
            var exclude = new HashSet<int> { H.CopId };
            foreach (var j in ContractRegistry.ActiveHitmanJobs())
            {
                if (j.purp != null) exclude.Add(j.purp.humanID);
                if (j.poster != null) exclude.Add(j.poster.humanID);
            }
            var candidates = Each(CityData.Instance.citizenDirectory)
                .Where(c => c != null && !c.isDead && !c.isPlayer && c.job != null && c.job.preset != null
                            && CriminalJobs.Contains(c.job.preset.name) && !exclude.Contains(c.humanID) && !H.HunterIds.Contains(c.humanID))
                .OrderBy(c => Array.IndexOf(CriminalJobs, c.job.preset.name))
                .ToList();
            if (candidates.Count == 0)
            {
                Plugin.Logger.LogWarning("[Fixer] No citizen with a criminal job to be the Fixer.");
                return;
            }
            // Prefer the jobs listed first, random among them.
            var best = candidates.Where(c => c.job.preset.name == candidates[0].job.preset.name).ToList();
            var fixer = best[new Random().Next(best.Count)];
            if (replacing) H.FixerContacted = false;
            H.FixerId = fixer.humanID;
            H.FixerName = fixer.GetCitizenName();
            Plugin.Logger.LogInfo($"[Fixer] Picked {Who(fixer)} ({fixer.job.preset.name}), replacing={replacing}");
        }

        private static int NewNumber()
        {
            var phones = CityData.Instance.phoneDictionary;
            var fake = TelephoneController.Instance.fakeTelephoneDictionary;
            var rng = new Random();
            for (int i = 0; i < 100; i++)
            {
                int n = rng.Next(5000000, 5999999);
                if ((phones == null || !phones.ContainsKey(n)) && (fake == null || !fake.ContainsKey(n)) && n != H.CopNumber) return n;
            }
            return 5123456;
        }

        private static string NumberText => H.FixerNumber < 0 ? "?" : H.FixerNumber.ToString().Insert(3, "-");

        private static Human FixerHuman()
        {
            Human h = null;
            return H.FixerId >= 0 && CityData.Instance != null && CityData.Instance.GetHuman(H.FixerId, out h, false) ? h : null;
        }

        // ---- first contact ----

        internal static void OnReputation()
        {
            if (!Enabled || H.FixerContacted || H.FixerNextCall >= 0f) return;
            if (H.Reputation < HitmanConfig.FixerFirstContactReputation.Value) return;
            H.FixerNextCall = Text.Now + 0.5f;
            Plugin.Logger.LogInfo($"[Fixer] Reputation {H.Reputation}: first call at {Text.Clock(H.FixerNextCall)}");
        }

        internal static void CallNow()
        {
            EnsureReady();
            H.FixerNextCall = Text.Now;
            ringingCall = null;
            StartCall();
        }

        private static string IntroLine() =>
            Text.L($"Aqui é {H.FixerName}. Andei ouvindo falar de você, e falam bem. Eu arrumo serviço pra quem é bom no que faz, paga melhor que bilhete em quadro. " +
                   $"Também consigo equipamento. Anota: {NumberText}. Liga que eu te digo onde me achar.",
                   $"This is {H.FixerName}. I've been hearing about you, and it's good. I find work for people who are good at what they do, better than notes on a board. " +
                   $"I can get gear too. Write this down: {NumberText}. Call and I'll tell you where to find me.");

        private static void StartCall()
        {
            var fixer = FixerHuman();
            var player = Player.Instance;
            if (fixer == null || fixer.isDead || player == null || TelephoneController.Instance == null) return;
            var to = CorruptCop.HomePhone(player);
            if (to == null)
            {
                H.FixerNextCall = Text.Now + HitmanConfig.CopRetryHours.Value;
                return;
            }
            var from = CorruptCop.CopPhone(fixer, to);
            if (from == null)
            {
                H.FixerNextCall = Text.Now + HitmanConfig.CopRetryHours.Value;
                return;
            }
            // The game's way of calling the player (PhoneCalls): the voice on the line says the intro.
            ringingCall = PhoneCalls.CallPlayer(from, to, IntroLine());
            Plugin.Logger.LogInfo($"[Fixer] Calling the player at home {to.numberString} from {from.numberString}; call created={ringingCall != null}");
            if (ringingCall != null) NoteNumber(); // his number goes into the notebook on his first call
        }

        // The notebook shows textOverride as a key of the evidence.generic strings (content/Heat).
        private const string NotebookKey = "hitman_fixer";

        private static void NoteNumber(bool show = true)
        {
            try { GameplayController.Instance.AddOrMergePhoneNumberData(H.FixerNumber, false, null, NotebookKey, show); }
            catch (Exception e) { Plugin.Logger.LogWarning($"[Fixer] Could not add the number to the notebook: {e.Message}"); }
        }

        private static void MarkContacted()
        {
            if (H.FixerContacted) return;
            H.FixerContacted = true;
            H.FixerNextCall = -1f;
            NoteNumber();
            Plugin.Logger.LogInfo($"[Fixer] Contacted; number {NumberText} in the notebook.");
        }

        [HarmonyPatch(typeof(TelephoneController.PhoneCall), nameof(TelephoneController.PhoneCall.SetCallState))]
        internal static class PhoneCall_SetCallState
        {
            static void Postfix(TelephoneController.PhoneCall __instance, TelephoneController.CallState newState)
            {
                if (ringingCall == null || __instance == null || __instance.Pointer != ringingCall.Pointer) return;
                Plugin.Logger.LogInfo($"[Fixer] Call state -> {newState}");
                if (newState == TelephoneController.CallState.started) MarkContacted();
                else if (newState == TelephoneController.CallState.ended || newState == TelephoneController.CallState.denied)
                {
                    bool answered = H.FixerContacted;
                    ringingCall = null;
                    if (!answered) H.FixerNextCall = Text.Now + HitmanConfig.CopRetryHours.Value;
                }
            }
        }

        // ---- per game minute ----

        internal static void Tick()
        {
            if (!Enabled) return;
            EnsureReady();
            if (!sessionReady) return;
            float now = Text.Now;
            var fixer = FixerHuman();
            if (fixer == null || fixer.isDead)
            {
                if (H.FixerReplaceAt < 0f)
                {
                    H.FixerReplaceAt = now + HitmanConfig.FixerReplaceDays.Value * 24f;
                    Plugin.Logger.LogInfo($"[Fixer] {H.FixerName} is gone; a replacement at {Text.Clock(H.FixerReplaceAt)}");
                }
                else if (now >= H.FixerReplaceAt)
                {
                    H.FixerReplaceAt = -1f;
                    Pick(replacing: true);
                    if (H.Reputation >= HitmanConfig.FixerFirstContactReputation.Value) H.FixerNextCall = now;
                }
                return;
            }
            if (ringingCall == null && !H.FixerContacted && H.FixerNextCall >= 0f && now >= H.FixerNextCall) StartCall();
        }

        // A call to his number has no telephone at the other end (PhoneCalls): it is known by its greeting, and by him
        // taking it.
        private static bool InFixerCall()
        {
            var calls = TelephoneController.Instance != null ? TelephoneController.Instance.activeCalls : null;
            if (calls == null || H.FixerNumber < 0) return false;
            string greetingName = greeting != null && greeting.Preset != null ? greeting.Preset.name : null;
            foreach (var call in Each(calls))
            {
                if (call == null || call.state != TelephoneController.CallState.started) continue;
                if (call.to == H.FixerNumber || call.from == H.FixerNumber || (H.FixerId >= 0 && call.receiver == H.FixerId)) return true;
                if (greetingName != null && PhoneCalls.GreetingOf(call) == greetingName) return true;
            }
            return false;
        }

        // His menu: face to face (within a few metres) once the player knows him (decided 20/10/2026, 8th test).
        private static bool FaceToFace(Citizen saysTo)
        {
            if (!H.FixerContacted || saysTo == null || H.FixerId < 0 || saysTo.humanID != H.FixerId) return false;
            var player = Player.Instance;
            return player != null && (saysTo.transform.position - player.transform.position).magnitude <= 8f;
        }

        // ---- a job handed over in person ----

        // The intro dialog that carries the client's information, by intro: phone call, payphone, meeting.
        private static readonly string[] InfoDialogs = { "IntroCall", "IntroGooseChase2", "IntroMeetUpSpecific2" };

        // Accepts the job for the player and gives them its information at once, as if the client had called or met
        // them: SideJob.AcceptJob opens the case and sets the hand-in, SideJob.OnAcquireJobInfo applies the leads and
        // facts (and MeetingClock pays the deposit). Runs when the job's note is up (Postings).
        internal static void HandOver(SideJob job)
        {
            if (job == null) return;
            try
            {
                if (!job.accepted) job.AcceptJob();
                if (!job.accepted)
                {
                    Plugin.Logger.LogWarning($"[Fixer] Job #{job.jobID}: the game did not accept it (too many open cases?)");
                    Text.Notify("Você tem casos demais abertos para pegar o serviço do Fixer. Feche algum e peça de novo.",
                                "You have too many open cases to take the Fixer's job. Close one and ask again.");
                    return;
                }
                DialogPreset info = null;
                foreach (var r in Each(job.preset != null ? job.preset.dialogReferences : null))
                    if (r != null && r.dialog != null && Array.IndexOf(InfoDialogs, r.name) >= 0 && (info == null || job.chosenIntro == null)) info = r.dialog;
                if (info != null) job.OnAcquireJobInfo(info);
                Plugin.Logger.LogInfo($"[Fixer] Job #{job.jobID} handed over in person: target {Who(job.purp)}, client {Who(job.poster)}, info via {N(info) ?? "none"}");
                string brief = BriefShort(job);
                Text.Notify($"Serviço do Fixer: {brief} O caso já está aberto; não precisa ligar para o bilhete.",
                            $"The Fixer's job: {brief} The case is already open; no need to call the note.");
            }
            catch (Exception e) { Plugin.Logger.LogError($"[Fixer] Handing over job #{job.jobID} failed: {e}"); }
        }

        // What he tells the player about the job (spoken).
        internal static string Brief(SideJob job)
        {
            var purp = job.purp;
            var occupation = purp != null ? purp.job : null;
            string who = purp != null ? purp.GetCitizenName() : "?";
            string title = occupation != null && !string.IsNullOrEmpty(occupation.name) ? occupation.name : null;
            string work = occupation != null && occupation.employer != null ? occupation.employer.name : null;
            string door = job.poster != null && job.poster.home != null ? $"{N(job.poster.home)} ({N(job.poster.home.building)})" : "?";
            string at = (title != null ? $", {title}" : "") + (work != null ? Text.L($", trabalha em {work}", $", works at {work}") : "");
            return Text.L($"O alvo é {who}{at}. Quando terminar, passa o caso por baixo da porta em {door}. Metade adiantada já tá na sua conta.",
                          $"The target is {who}{at}. When it's done, slip the case under the door at {door}. Half up front is already in your account.");
        }

        private static string BriefShort(SideJob job)
        {
            string who = job.purp != null ? job.purp.GetCitizenName() : "?";
            string work = job.purp != null && job.purp.job != null && job.purp.job.employer != null ? job.purp.job.employer.name : null;
            string door = job.poster != null && job.poster.home != null ? N(job.poster.home) : "?";
            return Text.L($"alvo {who}{(work != null ? $" ({work})" : "")}, entrega em {door}.", $"target {who}{(work != null ? $" ({work})" : "")}, hand-in at {door}.");
        }

        // ---- the phone: where to find him ----

        // Where he is right now, for the player to come over.
        private static string WhereNow()
        {
            var fixer = FixerHuman();
            if (fixer == null) return Text.L("por aí", "around");
            var loc = fixer.currentGameLocation;
            string place = N(loc) ?? "?", building = N(fixer.currentBuilding) ?? "?";
            bool home = fixer.home != null && loc == fixer.home;
            bool work = fixer.job != null && fixer.job.employer != null && fixer.job.employer.placeOfBusiness != null && loc == fixer.job.employer.placeOfBusiness;
            if (home) return Text.L($"em casa, {place} ({building})", $"at home, {place} ({building})");
            if (work) return Text.L($"no trabalho, {place} ({building})", $"at work, {place} ({building})");
            return Text.L($"em {place} ({building})", $"at {place} ({building})");
        }

        private static string GreetingLine() =>
            Text.L($"Fala, {Career.TierName(Career.Tier).ToLowerInvariant()}. Negócio eu não trato por telefone. Tô {WhereNow()}. Vem falar comigo pessoalmente.",
                   $"Talk to me, {Career.TierName(Career.Tier).ToLowerInvariant()}. I don't do business over the phone. I'm {WhereNow()}. Come see me in person.");

        private sealed class GreetingLogic : IDialogLogic
        {
            public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef) => false;

            public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
            {
                Lib.DdsStrings["dds.blocks", greetingResponse.BlockId] = GreetingLine();
                MarkContacted();
                string place = WhereNow();
                Plugin.Logger.LogInfo($"[Fixer] Phone: he is {place}");
                Text.Notify($"Fixer ({H.FixerName}): {place}.", $"Fixer ({H.FixerName}): {place}.");
                return DialogController.ForceSuccess.success;
            }

            public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef) { }
        }

        private sealed class JobLogic : FixerOption
        {
            public override bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (!Visible(saysTo)) return false;
                Label(Text.L("Tem serviço pra mim?", "Got any work for me?"));
                return true;
            }

            public override DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
            {
                float now = Text.Now;
                if (H.FixerNextJob > now)
                {
                    Reply = Text.L($"Ainda não tenho nada. Volta depois das {Text.Clock(H.FixerNextJob)}.", $"Nothing yet. Come back after {Text.Clock(H.FixerNextJob)}.");
                    return DialogController.ForceSuccess.success;
                }
                var presets = Career.UnlockedKillPresets();
                if (presets.Count == 0)
                {
                    Reply = Text.L("Agora não tenho nada pra você.", "I've got nothing for you right now.");
                    return DialogController.ForceSuccess.success;
                }
                // Best tier unlocked, a random one when several share it.
                int top = Math.Min(presets.Count, 2);
                var name = presets[new Random().Next(top)];
                // Decided 20/10/2026 (8th test): he hands the job over right here, no note to find, no call to make.
                var job = Postings.Request(name, "fixer", HandOver, out var failure);
                if (job == null)
                {
                    Plugin.Logger.LogInfo($"[Fixer] No job: {failure}");
                    Reply = Text.L("Tô sem cliente agora. Volta mais tarde.", "No clients right now. Come back later.");
                    return DialogController.ForceSuccess.success;
                }
                H.FixerNextJob = now + HitmanConfig.FixerJobCooldownHours.Value;
                Reply = Text.L("Tenho um, e paga bem. ", "Got one, and it pays well. ") + Brief(job);
                return DialogController.ForceSuccess.success;
            }

        }

        private sealed class SheetLogic : FixerOption
        {
            public override bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (!Visible(saysTo)) return false;
                Label(Text.L("Como anda a minha fama?", "How's my reputation?"));
                return true;
            }

            public override DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
            {
                Reply = Career.Sheet();
                return DialogController.ForceSuccess.success;
            }

        }

        // v1.2: "About the big client…", the campaign's chapters (Campaign.cs).
        private sealed class CampaignLogic : FixerOption
        {
            public override bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (!Visible(saysTo)) return false;
                var text = Campaign.FixerOptionText();
                if (text == null) return false;
                Label(text);
                return true;
            }

            public override DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
            {
                Reply = Campaign.OnFixerOption();
                return DialogController.ForceSuccess.success;
            }

        }

        // ---- shop ----

        private static List<(InteractablePreset preset, int price)> shopItems;

        private static List<(InteractablePreset preset, int price)> ShopItems()
        {
            if (shopItems != null) return shopItems;
            shopItems = new List<(InteractablePreset, int)>();
            foreach (var entry in HitmanConfig.FixerShop.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var kv = entry.Split(':');
                if (kv.Length != 2 || !int.TryParse(kv[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int price)) continue;
                var preset = WeaponController.FindPreset(kv[0].Trim());
                if (preset == null)
                {
                    Plugin.Logger.LogWarning($"[Fixer] Shop item '{kv[0]}' not found in this game build.");
                    continue;
                }
                shopItems.Add((preset, price));
                if (shopItems.Count == ShopSlots) break;
            }
            return shopItems;
        }

        internal static void OnCityReady() => shopItems = null;

        private static string ItemName(InteractablePreset preset)
        {
            try
            {
                var s = Strings.Get("evidence.names", preset.presetName, Strings.Casing.asIs, false, true, false, null);
                if (!string.IsNullOrEmpty(s) && s != preset.presetName) return s;
            }
            catch { }
            return preset.presetName;
        }

        private sealed class ShopLogic : FixerOption
        {
            private readonly int slot;

            public ShopLogic(int slot) => this.slot = slot;

            public override bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                var items = ShopItems();
                if (slot >= items.Count || !Visible(saysTo)) return false;
                var (item, price) = items[slot];
                Label(Text.L($"Quero comprar: {ItemName(item)} (Cr{price})", $"I want to buy: {ItemName(item)} (Cr{price})"));
                return true;
            }

            public override DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
            {
                var items = ShopItems();
                if (slot >= items.Count) return DialogController.ForceSuccess.fail;
                var (item, price) = items[slot];
                var gameplay = GameplayController.Instance;
                if (gameplay == null || gameplay.money < price)
                {
                    Reply = Text.L("Volta quando tiver o dinheiro.", "Come back when you have the money.");
                    return DialogController.ForceSuccess.success;
                }
                gameplay.AddMoney(-price, true, "Fixer");
                bool given = WeaponShops.Give(item);
                Plugin.Logger.LogInfo($"[Fixer] Sold {item.name} for Cr{price}; in inventory={given}");
                Reply = Text.L("Fechado. Um garoto vai te entregar agora mesmo.", "Done. A kid will bring it to you right now.");
                return DialogController.ForceSuccess.success;
            }

        }

        // ---- dev menu ----

        internal static string Status() =>
            H.FixerId < 0 ? Text.L("ainda não escolhido", "not picked yet")
            : Text.L($"{H.FixerName}, número {NumberText}, contato {(H.FixerContacted ? "feito" : "não")}", $"{H.FixerName}, number {NumberText}, contacted {(H.FixerContacted ? "yes" : "no")}");

        internal static Human DevFixer() => FixerHuman();

        internal static void DevMarkContacted()
        {
            EnsureReady();
            if (sessionReady) MarkContacted();
        }
    }
}
