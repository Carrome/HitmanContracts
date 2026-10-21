using System.IO;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;

namespace SODHitman
{
    internal static class HitmanConfig
    {
        internal static ConfigEntry<float> PaymentBase;
        internal static ConfigEntry<float> PaymentSalaryDivisor;
        internal static ConfigEntry<float> PaymentFighterRisk;
        internal static ConfigEntry<float> PaymentRandomSpread;
        internal static ConfigEntry<int> PaymentMin;
        internal static ConfigEntry<int> PaymentMax;
        internal static ConfigEntry<float> PaymentDepositShare;
        internal static ConfigEntry<string> FighterJobs;

        internal static ConfigEntry<bool> BribeEnabled;
        internal static ConfigEntry<float> BribeSalaryShare;
        internal static ConfigEntry<float> BribeSalaryPeriods;
        internal static ConfigEntry<bool> BribeShowToEveryone;
        internal static ConfigEntry<bool> BribeForceCombatFallback;

        internal static ConfigEntry<bool> HeatEnabled;
        internal static ConfigEntry<int> HeatContractKill;
        internal static ConfigEntry<int> HeatOtherKill;
        internal static ConfigEntry<int> HeatPerWitness;
        internal static ConfigEntry<int> HeatMaxWitnesses;
        internal static ConfigEntry<int> HeatCamera;
        internal static ConfigEntry<int> HeatPublicPlace;
        internal static ConfigEntry<int> HeatLevel1;
        internal static ConfigEntry<int> HeatLevel2;
        internal static ConfigEntry<int> HeatLevel3;
        internal static ConfigEntry<bool> HeatNewspaper;
        internal static ConfigEntry<bool> HeatInvestigations;
        internal static ConfigEntry<float> HeatInvestigationHoursL2;
        internal static ConfigEntry<float> HeatInvestigationHoursL3;
        internal static ConfigEntry<bool> HeatWarrant;
        internal static ConfigEntry<float> HeatWantedValue;
        internal static ConfigEntry<bool> HeatHunters;
        internal static ConfigEntry<float> HeatHunterHours;
        internal static ConfigEntry<int> HeatMaxHunters;

        internal static ConfigEntry<bool> StealthEnabled;
        internal static ConfigEntry<bool> StealthHints;
        internal static ConfigEntry<bool> StealthCrimeScene;
        internal static ConfigEntry<float> WitnessDelayMinHours;
        internal static ConfigEntry<float> WitnessDelayMaxHours;
        internal static ConfigEntry<int> WitnessBribePrice;
        internal static ConfigEntry<float> WitnessBribeAcceptChance;
        internal static ConfigEntry<bool> WitnessDartSilences;
        internal static ConfigEntry<float> RatingBonusSilent;
        internal static ConfigEntry<float> RatingBonusPro;
        internal static ConfigEntry<bool> BribeTargetLeavesTown;
        internal static ConfigEntry<float> BribeDiscoveryChance;

        internal static ConfigEntry<string> ArsenalFirearms;
        internal static ConfigEntry<string> ArsenalSnipers;
        internal static ConfigEntry<string> ArsenalBlades;
        internal static ConfigEntry<string> ArsenalGarrotes;
        internal static ConfigEntry<string> ArsenalPoisons;
        internal static ConfigEntry<bool> ArsenalSellFirearms;
        internal static ConfigEntry<float> ArsenalSniperZoom;
        internal static ConfigEntry<float> ArsenalHearingRadius;
        internal static ConfigEntry<float> ArsenalBladeDamageMultiplier;
        internal static ConfigEntry<int> ArsenalBladeHitsToKill;
        internal static ConfigEntry<float> PoisonDelayMinHours;
        internal static ConfigEntry<float> PoisonDelayMaxHours;
        internal static ConfigEntry<int> ArsenalNaturalDeathPoints;
        internal static ConfigEntry<int> ArsenalSuicidePoints;

        internal static ConfigEntry<bool> CareerEnabled;
        internal static ConfigEntry<string> CareerTierThresholds;
        internal static ConfigEntry<string> CareerTierPay;
        internal static ConfigEntry<string> CareerPresetTiers;
        internal static ConfigEntry<float> CareerNonLethalPay;
        internal static ConfigEntry<bool> FixerEnabled;
        internal static ConfigEntry<float> FixerBonus;
        internal static ConfigEntry<int> FixerFirstContactReputation;
        internal static ConfigEntry<float> FixerJobCooldownHours;
        internal static ConfigEntry<float> FixerReplaceDays;
        internal static ConfigEntry<string> FixerShop;
        internal static ConfigEntry<bool> ConsequencesEnabled;
        internal static ConfigEntry<float> TraitorChance;
        internal static ConfigEntry<int> TraitorPoliceHeat;
        internal static ConfigEntry<float> RevengeChance;
        internal static ConfigEntry<float> RevengeDelayMinHours;
        internal static ConfigEntry<float> RevengeDelayMaxHours;
        internal static ConfigEntry<float> ScaredHours;
        internal static ConfigEntry<bool> CampaignEnabled;
        internal static ConfigEntry<int> CampaignStartReputation;
        internal static ConfigEntry<float> CampaignPayMultiplier;
        internal static ConfigEntry<int> CampaignFinalReward;

        internal static ConfigEntry<bool> InterrogationEnabled;
        internal static ConfigEntry<int> InterrogationConvinced;
        internal static ConfigEntry<int> InterrogationSuspicious;
        internal static ConfigEntry<int> InterrogationAvoided;
        internal static ConfigEntry<float> InterrogationAvoidHours;
        internal static ConfigEntry<float> InterrogationWaitMinutes;
        internal static ConfigEntry<float> InterrogationArrivalHours;
        internal static ConfigEntry<string> KeyInterrogate;
        internal static ConfigEntry<string> KeyMenu;

        internal static ConfigEntry<bool> WeaponsEnabled;
        internal static ConfigEntry<string> TranquilizerPreset;
        internal static ConfigEntry<string> LethalPreset;
        internal static ConfigEntry<string> SellAtShops;
        internal static ConfigEntry<float> WeaponAccuracy;
        internal static ConfigEntry<float> WeaponDelayFactor;
        internal static ConfigEntry<float> WeaponRecoil;
        internal static ConfigEntry<float> WeaponZoom;
        internal static ConfigEntry<int> DartsToSleep;
        internal static ConfigEntry<float> SleepSeconds;
        internal static ConfigEntry<bool> TranqIllegalWhenSeen;
        internal static ConfigEntry<bool> TranqAlertSurrounding;
        internal static ConfigEntry<int> LethalShotsToKill;
        internal static ConfigEntry<float> LethalDamageMultiplier;
        internal static ConfigEntry<bool> LethalIllegalWhenSeen;
        internal static ConfigEntry<bool> LethalAlertSurrounding;
        internal static ConfigEntry<float> IllegalSecondsOnShot;
        internal static ConfigEntry<bool> ReapplyKillsOnLoad;
        internal static ConfigEntry<string> KeyGiveWeapons;

        internal static ConfigEntry<bool> CopEnabled;
        internal static ConfigEntry<float> CopRetryHours;
        internal static ConfigEntry<float> CopReplaceDays;
        internal static ConfigEntry<float> CopRingHours;
        internal static ConfigEntry<float> CopPayphoneRadius;
        internal static ConfigEntry<int> CopFallbackPricePerPoint;
        internal static ConfigEntry<float> CopFilePremium;
        internal static ConfigEntry<bool> CiviliansUseGuns;
        internal static ConfigEntry<float> CopWarrantMultiplier;
        internal static ConfigEntry<float> CopDropRadius;
        internal static ConfigEntry<float> CopDropDeadlineHours;
        internal static ConfigEntry<string> KeyCopCall;

        internal static ConfigEntry<string> KeyHeatUp;
        internal static ConfigEntry<string> KeyHeatDown;

        internal static ConfigEntry<int> MeetDelayMinutes;
        internal static ConfigEntry<int> LateToleranceMinutes;
        internal static ConfigEntry<bool> EnforceMeetingDeadline;
        internal static ConfigEntry<bool> ShowNotifications;

        internal static ConfigEntry<bool> DevHotkeys;
        internal static ConfigEntry<bool> DevProbes;
        internal static ConfigEntry<bool> DevAutoDump;
        internal static ConfigEntry<string> DevForcePostPreset;
        internal static ConfigEntry<string> KeyDump;
        internal static ConfigEntry<string> KeyForcePost;
        internal static ConfigEntry<string> KeyListJobs;
        internal static ConfigEntry<string> KeyKillTarget;
        internal static ConfigEntry<string> KeyMurderTarget;

        internal static void Bind(ConfigFile config)
        {
            PaymentBase = config.Bind("Payment", "Base", 400f,
                "Contract payment = (Base + target salary / SalaryDivisor) x risk x random(1 - RandomSpread, 1 + RandomSpread). " +
                "Provisional formula (spec 5.2b); the bribe rule accepts when 2 x payment <= salary x 0.4 / 12.");
            PaymentSalaryDivisor = config.Bind("Payment", "SalaryDivisor", 120f, "Divides the target's salary in the payment formula.");
            PaymentFighterRisk = config.Bind("Payment", "FighterRiskMultiplier", 1.5f, "Risk multiplier when the target's job is in FighterJobs.");
            PaymentRandomSpread = config.Bind("Payment", "RandomSpread", 0.2f, "Random variation, 0.2 = +/-20%. Seeded by the job, so a contract always pays the same.");
            PaymentMin = config.Bind("Payment", "Min", 300, "Lowest payment after rounding.");
            PaymentMax = config.Bind("Payment", "Max", 5000, "Highest payment after rounding.");
            PaymentDepositShare = config.Bind("Payment", "DepositShare", 0.5f, "Share of the payment the client hands over at the meeting (0.5 = half); the rest comes with the hand-in. 0 = all at the hand-in.");
            FighterJobs = config.Bind("Payment", "FighterJobs",
                "Enforcer,EnforcerCaptain,SecurityGuard,SecurityGuardDesk,DebtCollector,WeaponsDealer,GamblingDenOwner,HiredKiller",
                "OccupationPreset names of targets that fight back (spec 5.3). Comma separated.");

            BribeEnabled = config.Bind("Bribe", "Enabled", true,
                "Dialog option to reveal the contract to the target and ask for double the payment (spec 5).");
            BribeSalaryShare = config.Bind("Bribe", "SalaryShare", 0.4f,
                "The target accepts when 2 x payment <= SalaryShare x salary / SalaryPeriods. Targets without a job always refuse.");
            BribeSalaryPeriods = config.Bind("Bribe", "SalaryPeriods", 12f,
                "Divides the salary in the bribe rule (12 = monthly, if the game's salary is yearly).");
            BribeShowToEveryone = config.Bind("Bribe", "ShowToEveryone", true,
                "Offer the option to anyone while a contract is active (others just shrug), so it does not give away who the target is.");
            BribeForceCombatFallback = config.Bind("Bribe", "ForceCombatFallback", true,
                "If a refusing fighter does not start combat from the dialog alone, make them pursue the player.");

            HeatEnabled = config.Bind("Heat", "Enabled", true,
                "Heat score for deaths caused by the player (spec 11). It never decays; only the corrupt cop lowers it.");
            HeatContractKill = config.Bind("Heat", "ContractKill", 10, "Points for killing the target of an active contract.");
            HeatOtherKill = config.Bind("Heat", "OtherKill", 20, "Points for any other death caused by the player.");
            HeatPerWitness = config.Bind("Heat", "PerWitness", 15, "Extra points per witness who saw the player's illegal act.");
            HeatMaxWitnesses = config.Bind("Heat", "MaxWitnesses", 4, "Witnesses counted at most per death.");
            HeatCamera = config.Bind("Heat", "Camera", 10, "Extra points when the place has security cameras.");
            HeatPublicPlace = config.Bind("Heat", "PublicPlace", 5, "Extra points on the street or in a place open to the public.");
            HeatLevel1 = config.Bind("Heat", "Level1", 30, "Level 1 (newspaper) from this many points.");
            HeatLevel2 = config.Bind("Heat", "Level2", 60, "Level 2 (enforcers interrogate, the corrupt cop calls) from this many points.");
            HeatLevel3 = config.Bind("Heat", "Level3", 90, "Level 3 (warrant and hunters) from this many points.");
            // Decided 19/10/2026: 50/100/160 took too long. A cfg written by an older version still holds the old
            // defaults; move it to the new ones unless the player changed any of them.
            if (HeatLevel1.Value == 50 && HeatLevel2.Value == 100 && HeatLevel3.Value == 160)
            {
                HeatLevel1.Value = 30;
                HeatLevel2.Value = 60;
                HeatLevel3.Value = 90;
            }
            HeatNewspaper = config.Bind("Heat", "Newspaper", true, "Each new level puts an article on the next newspaper's front page.");
            HeatInvestigations = config.Bind("Heat", "Investigations", true, "From level 2, enforcers are sent to the player's home now and then.");
            HeatInvestigationHoursL2 = config.Bind("Heat", "InvestigationHoursLevel2", 24f, "Game hours between enforcer visits at level 2.");
            HeatInvestigationHoursL3 = config.Bind("Heat", "InvestigationHoursLevel3", 8f, "Game hours between enforcer visits at level 3.");
            HeatWarrant = config.Bind("Heat", "Warrant", true, "At level 3 the player is wanted in every building they enter.");
            HeatWantedValue = config.Bind("Heat", "WantedValue", 1f, "Value passed to the game's SetWantedInBuilding on every game minute at level 3 (experimental).");
            HeatHunters = config.Bind("Heat", "Hunters", true, "At level 3 a hired killer comes after the player now and then.");
            HeatHunterHours = config.Bind("Heat", "HunterIntervalHours", 12f, "Game hours between hunters.");
            HeatMaxHunters = config.Bind("Heat", "MaxHunters", 1, "Hunters chasing the player at the same time.");

            StealthEnabled = config.Bind("Stealth", "Enabled", true,
                "Heat from what the police find out (v0.7): a death counts when the body is found, a witness when they report, a camera only if it was on and saw you. Off = everything counts at the kill.");
            StealthHints = config.Bind("Stealth", "Hints", true, "On-screen hints when someone saw the kill or a camera caught you.");
            StealthCrimeScene = config.Bind("Stealth", "CrimeScene", true, "A found body becomes a crime scene: the game sends Enforcers to the place.");
            WitnessDelayMinHours = config.Bind("Stealth", "WitnessDelayMinHours", 1f, "Earliest a witness goes to the police (game hours after the kill).");
            WitnessDelayMaxHours = config.Bind("Stealth", "WitnessDelayMaxHours", 2f, "Latest a witness goes to the police.");
            WitnessBribePrice = config.Bind("Stealth", "WitnessBribePrice", 200, "Money offered to a witness to forget what they saw.");
            WitnessBribeAcceptChance = config.Bind("Stealth", "WitnessBribeAcceptChance", 0.75f, "Chance a witness takes the money (Enforcers never do). A refusal reports at once.");
            WitnessDartSilences = config.Bind("Stealth", "DartSilencesWitness", true, "A witness put to sleep with the tranquilizer before reporting forgets what they saw.");
            RatingBonusSilent = config.Bind("Stealth", "RatingBonusSilentAssassin", 0.25f, "Bonus (share of the payment) for a Silent Assassin rating.");
            RatingBonusPro = config.Bind("Stealth", "RatingBonusProfessional", 0.1f, "Bonus for a Professional rating.");
            BribeTargetLeavesTown = config.Bind("Bribe", "TargetLeavesTown", true,
                "A target who takes the bribe leaves town and the contract counts as done, so the client pays too. Off = the contract just ends (0.5 behaviour).");
            BribeDiscoveryChance = config.Bind("Bribe", "ClientDiscoveryChance", 0.25f,
                "Chance the client later finds out the target is alive and sends a killer after you (never on the first mission).");

            ArsenalFirearms = config.Bind("Arsenal", "LoudFirearms", "Revolver,Shotgun,SniperRifle,BattleRifle",
                "The game's firearms the player can fire like the lethal pistol (item names). They are loud: everyone within HearingRadius becomes a witness.");
            ArsenalSnipers = config.Bind("Arsenal", "SniperRifles", "SniperRifle,BattleRifle", "Firearms that zoom further when aiming.");
            ArsenalBlades = config.Bind("Arsenal", "Blades", "CombatKnife,KitchenKnifeLarge,KitchenKnifeSmall,Katana,BoxCutter,Razor",
                "Melee items that kill: a hit that knocks out, empties health or reaches BladeHitsToKill.");
            ArsenalGarrotes = config.Bind("Arsenal", "Garrotes", "RopeWeapon,MetalWire", "Items that strangle: one silent kill from behind (or on someone down).");
            ArsenalPoisons = config.Bind("Arsenal", "Poisons", "Syringe,PoisonBottle", "Items that poison someone asleep or knocked out: they die hours later of 'natural causes'.");
            ArsenalSellFirearms = config.Bind("Arsenal", "SellFirearms", true, "Weapons dealers and the black market also sell the loud firearms.");
            ArsenalSniperZoom = config.Bind("Arsenal", "SniperZoom", 0.6f, "Camera zoom when aiming a sniper rifle (0 to 1).");
            ArsenalHearingRadius = config.Bind("Arsenal", "HearingRadius", 30f, "Metres within which a loud gunshot makes witnesses.");
            CiviliansUseGuns = config.Bind("Arsenal", "CiviliansUseGuns", false,
                "Ordinary citizens may fight with a gun they carry. Off (decided 20/10/2026): only Enforcers, fighting jobs (FighterJobs), the game's killers and hunters sent after you draw guns; everyone else fights bare-handed or with a blade.");
            ArsenalBladeDamageMultiplier = config.Bind("Arsenal", "BladeDamageMultiplier", 2f, "Damage multiplier for blade hits.");
            ArsenalBladeHitsToKill = config.Bind("Arsenal", "BladeHitsToKill", 2, "Blade hits that kill even if the victim is still standing.");
            PoisonDelayMinHours = config.Bind("Arsenal", "PoisonDelayMinHours", 1f, "Earliest a poisoned citizen dies (game hours).");
            PoisonDelayMaxHours = config.Bind("Arsenal", "PoisonDelayMaxHours", 3f, "Latest a poisoned citizen dies.");
            ArsenalNaturalDeathPoints = config.Bind("Arsenal", "NaturalDeathHeat", 2, "Heat when the body of someone who died 'of natural causes' (poison) is found.");
            ArsenalSuicidePoints = config.Bind("Arsenal", "ForgedSuicideHeat", 0,
                "Heat when the body passes for suicide: poison or garrote with a suicide note in the victim's home.");

            CareerEnabled = config.Bind("Career", "Enabled", true,
                "Reputation tiers (v0.9): rated contracts build reputation; higher tiers unlock contracts and pay more. Off = every contract from the start, no tier pay.");
            CareerTierThresholds = config.Bind("Career", "TierThresholds", "0,5,15,30", "Reputation for Beginner, Professional, Veteran, Legend.");
            CareerTierPay = config.Bind("Career", "TierPayMultipliers", "1,1.25,1.5,2", "Payment multiplier per tier.");
            CareerPresetTiers = config.Bind("Career", "ContractTiers",
                "HitmanPizza:0,HitmanIntimidate:0,HitmanCall:1,HitmanPayphone:1,HitmanPassion:2,HitmanBadge:3",
                "Tier each contract needs before it shows up on the boards (preset:tier).");
            // 0.9.0 wrote placeholder names for the v1.0 catalog; move that default to the real contracts.
            if (CareerPresetTiers.Value == "HitmanPizza:0,HitmanIntimidate:0,HitmanPayphone:1,HitmanBriefcase:1,HitmanStaff:2,HitmanPassion:2,HitmanVisit:3")
                CareerPresetTiers.Value = (string)CareerPresetTiers.DefaultValue;
            CareerNonLethalPay = config.Bind("Career", "NonLethalPayMultiplier", 0.4f, "Payment of non-lethal jobs (intimidation) relative to a kill.");
            FixerEnabled = config.Bind("Fixer", "Enabled", true,
                "A middleman (a citizen with a criminal job) who calls once you have some reputation: exclusive contracts, your record, and gear for sale.");
            FixerBonus = config.Bind("Fixer", "ContractBonus", 0.2f, "Extra pay on contracts the Fixer hands out (0.2 = +20%).");
            FixerFirstContactReputation = config.Bind("Fixer", "FirstContactReputation", 1, "Reputation at which the Fixer calls for the first time.");
            FixerJobCooldownHours = config.Bind("Fixer", "JobCooldownHours", 12f, "Game hours between contracts the Fixer hands out.");
            FixerReplaceDays = config.Bind("Fixer", "ReplaceDays", 3f, "Game days until someone else takes over after the Fixer dies.");
            FixerShop = config.Bind("Fixer", "Shop", "Syringe:300,PoisonBottle:400,SuicideNote:250,RopeWeapon:150,SniperRifle:2500",
                "What the Fixer sells (item:price, up to 5).");

            ConsequencesEnabled = config.Bind("Consequences", "Enabled", true,
                "Twists from clients and targets (v1.1): traitor clients, revenge from the victim's close ones, scared targets who lay low.");
            TraitorChance = config.Bind("Consequences", "TraitorClientChance", 0.1f,
                "Chance a client betrays you at the hand-in (never on your first job): keeps the rest of the money, calls the police, or sends a killer.");
            TraitorPoliceHeat = config.Bind("Consequences", "TraitorPoliceHeat", 15, "Heat when the client calls the police at the hand-in.");
            RevengeChance = config.Bind("Consequences", "RevengeChance", 0.15f,
                "Chance someone close to a victim (partner, housemate, friend) hires a killer once the body is found. Doubled when the police have evidence.");
            RevengeDelayMinHours = config.Bind("Consequences", "RevengeDelayMinHours", 12f, "Earliest the hired killer comes (game hours).");
            RevengeDelayMaxHours = config.Bind("Consequences", "RevengeDelayMaxHours", 36f, "Latest the hired killer comes.");
            ScaredHours = config.Bind("Consequences", "ScaredTargetHours", 12f,
                "Game hours a scared target lays low at home (after refusing a bribe, or seeing you kill someone).");

            CampaignEnabled = config.Bind("Campaign", "Enabled", true,
                "The story \"The Last Contract\" (v1.2): three jobs for a big client through the Fixer, then a betrayal and a choice. Needs the Fixer.");
            CampaignStartReputation = config.Bind("Campaign", "StartReputation", 5, "Reputation at which the Fixer calls about the big client (5 = Professional).");
            CampaignPayMultiplier = config.Bind("Campaign", "PayMultiplier", 1.5f, "Payment of the campaign's jobs relative to a normal contract (on top of the Fixer bonus).");
            CampaignFinalReward = config.Bind("Campaign", "KillFixerReward", 5000, "Money for the ending where the Fixer dies.");

            InterrogationEnabled =config.Bind("Interrogation", "Enabled", true,
                "At heat level 2 the enforcer visit is a two-question interrogation at the player's home, without a search. Off = the game's own search.");
            InterrogationConvinced = config.Bind("Interrogation", "HeatWhenConvinced", -10, "Heat change when the answers convince the enforcer.");
            InterrogationSuspicious = config.Bind("Interrogation", "HeatPerSuspiciousPoint", 10, "Heat added per suspicious point (1 or 2) when they don't.");
            InterrogationAvoided = config.Bind("Interrogation", "HeatWhenAvoided", 10,
                "Heat added when the player is never home for the visit, walks out of it or never answers.");
            InterrogationAvoidHours = config.Bind("Interrogation", "AvoidHours", 24f, "Game hours a due visit waits for the player to be at home.");
            InterrogationWaitMinutes = config.Bind("Interrogation", "WaitMinutes", 45f, "Game minutes the enforcer waits for the answers.");
            InterrogationArrivalHours = config.Bind("Interrogation", "ArrivalTimeoutHours", 3f, "Game hours to wait for an enforcer to show up before the visit is dropped.");

            WeaponsEnabled = config.Bind("Weapons", "Enabled", true,
                "Two silenced pistols the player can fire (aim with Secondary, fire with Primary): a tranquilizer and a lethal one. " +
                "Infinite ammo, no reload. Not compatible with the original 'Guns' mod.");
            TranquilizerPreset = config.Bind("Weapons", "TranquilizerPreset", "HitmanTranquilizerPistol",
                "InteractablePreset used as the tranquilizer pistol (the mod's own copy of the silenced Lazarus 5).");
            LethalPreset = config.Bind("Weapons", "LethalPreset", "SemiAutomaticPistolSilenced",
                "InteractablePreset used as the lethal pistol (the game's silenced Lazarus 5, the same one killers use).");
            SellAtShops = config.Bind("Weapons", "SellAtShops", "WeaponsDealer,BlackmarketTrader",
                "MenuPreset names of the shops that stock both pistols. Empty = only what the game already places in the world.");
            WeaponAccuracy = config.Bind("Weapons", "Accuracy", 1f, "Accuracy from 0 to 1 (1 = perfect: the shot lands where you aim). -1 uses the game's value.");
            // 6th test (20/10/2026): shots must count from the first one; 0.9 spread made the first shot miss.
            if (WeaponAccuracy.Value == 0.9f) WeaponAccuracy.Value = 1f;
            WeaponDelayFactor = config.Bind("Weapons", "DelayBetweenShotsFactor", 1f, "Delay between shots, as a factor on the game's fire delay.");
            WeaponRecoil = config.Bind("Weapons", "RecoilAmplitude", 0.3f, "Camera kick per shot. 0 disables recoil.");
            WeaponZoom = config.Bind("Weapons", "ZoomInOnAimPct", 0.2f, "Camera zoom while aiming (0 to 1).");
            DartsToSleep = config.Bind("Tranquilizer", "DartsRequiredToSleep", 1, "Darts a citizen takes before falling asleep.");
            SleepSeconds = config.Bind("Tranquilizer", "SleepDurationSeconds", 180f,
                "Real seconds of unpaused play a tranquilized citizen sleeps (decided 19/10/2026: 3 minutes).");
            // Up to 0.5.5 this went straight to the game as hours (120 = five days of sleep); the old default moves.
            if (SleepSeconds.Value == 120f) SleepSeconds.Value = 180f;
            TranqIllegalWhenSeen = config.Bind("Tranquilizer", "IllegalWhenSeen", false, "A dart shot seen by someone gives illegal status.");
            TranqAlertSurrounding = config.Bind("Tranquilizer", "AlertSurroundingOnHit", false, "A dart hit alerts nearby citizens like a gunshot.");
            LethalShotsToKill = config.Bind("Lethal", "ShotsToKill", 1,
                "Hits that kill. A hit that knocks the citizen out or empties their health also kills.");
            LethalDamageMultiplier = config.Bind("Lethal", "DamageMultiplier", 1f, "Multiplier on the game's pistol damage.");
            LethalIllegalWhenSeen = config.Bind("Lethal", "IllegalWhenSeen", true,
                "A lethal shot seen or heard by someone gives illegal status (set before the hit, so witnesses count for heat).");
            LethalAlertSurrounding = config.Bind("Lethal", "AlertSurroundingOnHit", false, "A lethal hit alerts nearby citizens (the pistol is silenced).");
            IllegalSecondsOnShot = config.Bind("Weapons", "IllegalStatusSeconds", 3f, "Seconds of illegal status after a shot that gives it.");
            ReapplyKillsOnLoad = config.Bind("Lethal", "ReapplyKillsOnLoad", true,
                "Kill again, on load, the citizens the player shot dead (the game's save may bring them back, as LethalActionReborn found).");

            CopEnabled = config.Bind("CorruptCop", "Enabled", true,
                "An enforcer, picked when the save starts, who lowers heat for money (spec 11.1). Calls the player first at heat level 2.");
            CopRetryHours = config.Bind("CorruptCop", "RetryHours", 6f, "Game hours between call attempts until the player answers.");
            CopReplaceDays = config.Bind("CorruptCop", "ReplaceDays", 3f, "Game days until another enforcer takes over after the cop dies.");
            CopRingHours = config.Bind("CorruptCop", "RingHours", 0.15f, "How long each call rings (game hours).");
            CopPayphoneRadius = config.Bind("CorruptCop", "PayphoneRadius", 40f, "When the apartment phone is not answered, a payphone within this many metres of the player rings.");
            CopFallbackPricePerPoint = config.Bind("CorruptCop", "FallbackPricePerPoint", 35,
                "Price per heat point before any contract exists. Otherwise: 50% of the average contract payment per death (10 points).");
            CopFilePremium = config.Bind("CorruptCop", "FilePremium", 0.25f,
                "Extra on the third package, taking your name off the investigation (0.25 = +25%). The packages are priced per point of heat they take off now or keep from coming.");
            CopWarrantMultiplier = config.Bind("CorruptCop", "WarrantMultiplier", 2f, "Price multiplier while at heat level 3 (warrant).");
            CopDropRadius = config.Bind("CorruptCop", "DropRadius", 3f, "Metres from the dead drop payphone where the envelope is left.");
            CopDropDeadlineHours = config.Bind("CorruptCop", "DropDeadlineHours", 24f, "Game hours to leave the money before the deal is off.");

            MeetDelayMinutes = config.Bind("Contract", "MeetDelayMinutes", 60,
                "When the client sets a meeting on the phone, it is this many game minutes after the call (rounded up to 5 minutes).");
            LateToleranceMinutes = config.Bind("Contract", "LateToleranceMinutes", 15,
                "Minutes after the meeting time before the contract fails (spec: 15).");
            EnforceMeetingDeadline = config.Bind("Contract", "EnforceMeetingDeadline", true,
                "Fail the contract when the player misses the meeting. Turn off if contracts fail even after talking to the client.");
            ShowNotifications = config.Bind("Contract", "ShowNotifications", true,
                "On-screen messages: meeting time, job details updated, contract fulfilled, contract failed.");

            BindDev(config);
        }

        // ---- dev mode (decided 20/10/2026: hidden and off in the public release) ----

        // On when a file named devmode.txt sits next to the dll, or the main cfg has a line "DevMode = true" (added by
        // hand; never written by the mod). The mod page tells players how.
        internal static bool DevMode { get; private set; }

        private const string DevMarkerFile = "devmode.txt";

        private static bool DetectDevMode(ConfigFile config)
        {
            try
            {
                var dir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
                if (!string.IsNullOrEmpty(dir) && File.Exists(Path.Combine(dir, DevMarkerFile))) return true;
                var cfg = config.ConfigFilePath;
                if (File.Exists(cfg) && Regex.IsMatch(File.ReadAllText(cfg), @"^\s*DevMode\s*=\s*true\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                    return true;
            }
            catch { }
            return false;
        }

        // The dev settings live in their own file, sod.hitman.dev.cfg, created only in dev mode, so the player's cfg
        // never lists hotkeys, the menu or the probes. Outside dev mode they are bound to an unsaved file and off.
        private static void BindDev(ConfigFile config)
        {
            DevMode = DetectDevMode(config);
            ConfigFile dev;
            if (DevMode)
                dev = new ConfigFile(Path.Combine(Paths.ConfigPath, "sod.hitman.dev.cfg"), true);
            else
            {
                dev = new ConfigFile(Path.Combine(Path.GetTempPath(), "sod.hitman.dev.unused.cfg"), false);
                dev.SaveOnConfigSet = false;
            }

            DevHotkeys = dev.Bind("Dev", "EnableHotkeys", true,
                "Development hotkeys and the test menu. Read directly from Windows, only while the game window is focused.");
            DevProbes = dev.Bind("Dev", "EnableProbes", true,
                "Log deaths (Human.Murder) and side job lifecycle (state changes, fail, end, info acquired, DeadPeopleJobCheck). Applied at startup.");
            DevAutoDump = dev.Bind("Dev", "AutoDump", true,
                "Write the preset/citizen/job dump to BepInEx/plugins/SOD.Hitman/dump once per session, when a city finishes loading.");
            DevForcePostPreset = dev.Bind("Dev", "ForcePostPreset", "HitmanPizza",
                "JobPreset name the force-post hotkey asks SideJobController to create. Any vanilla side job name works too.");

            KeyMenu = dev.Bind("Dev", "KeyMenu", "Insert", "Open and close the in-game test menu.");
            KeyDump = dev.Bind("Dev", "KeyDump", "F2", "Write the dump now.");
            KeyGiveWeapons = dev.Bind("Dev", "KeyGiveWeapons", "F3", "Put both pistols in the inventory.");
            KeyCopCall = dev.Bind("Dev", "KeyCopCall", "F4", "Make the corrupt cop call the player now.");
            KeyHeatUp = dev.Bind("Dev", "KeyHeatUp", "Shift+F6", "Add 10 heat.");
            KeyHeatDown = dev.Bind("Dev", "KeyHeatDown", "Ctrl+F6", "Remove 10 heat.");
            KeyForcePost = dev.Bind("Dev", "KeyForcePost", "F8", "Ask the game to post one more job of ForcePostPreset.");
            KeyListJobs = dev.Bind("Dev", "KeyListJobs", "F9", "Log every side job (posted/accepted) and write dump/jobs_active.json.");
            KeyKillTarget = dev.Bind("Dev", "KeyKillTarget", "F10",
                "Lethal damage from the player to the target (purp) of the accepted job: Hitman jobs first, else the newest accepted job.");
            KeyMurderTarget = dev.Bind("Dev", "KeyMurderTarget", "F11", "Same target, but calls Human.Murder directly.");
            KeyInterrogate = dev.Bind("Dev", "KeyInterrogate", "F12", "Enforcers come to interrogate the player now (at home), at any heat level.");

            if (!DevMode)
            {
                DevHotkeys.Value = false;
                DevProbes.Value = false;
                DevAutoDump.Value = false;
            }
        }
    }
}
