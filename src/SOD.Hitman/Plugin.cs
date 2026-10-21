using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SOD.Common;
using SODHitman.Contracts;
using SODHitman.Dev;
using SODHitman.Weapons;

namespace SODHitman
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("Venomaus.SOD.Common")]
    [BepInDependency("AssetBundleLoader")]
    [BepInDependency("DDSLoader")]
    [BepInDependency("CommunityCaseLoader")]
    // The pistols include their own copy of the shooting code from "Guns", so they must not run next to it.
    [BepInIncompatibility("Guns")]
    public class Plugin : BasePlugin
    {
        public const string Guid = "sod.hitman";
        public const string Name = "Hitman Contracts";
        public const string Version = "1.2.6";

        internal static ManualLogSource Logger;

        public override void Load()
        {
            Logger = Log;
            HitmanConfig.Bind(Config);

            // Patch classes opt out through their own Prepare() (dev probes follow EnableProbes)
            new Harmony(Guid).PatchAll(typeof(Plugin).Assembly);

            Lib.SaveGame.OnBeforeNewGame += (_, _) =>
            {
                ContractStore.Loading = false;
                ContractStore.Reset();
            };
            Lib.SaveGame.OnBeforeLoad += (_, _) =>
            {
                ContractStore.Loading = true;
                ContractStore.Reset();
            };
            Lib.SaveGame.OnBeforeSave += (_, args) => ContractStore.Save(args);
            Lib.SaveGame.OnAfterLoad += (_, args) =>
            {
                try
                {
                    ContractStore.Load(args);
                    WeaponController.OnCityReady();
                    WeaponKills.ReapplyOnLoad();
                    Bribe.ReapplyOnLoad();
                }
                finally { ContractStore.Loading = false; }
                PresetDumper.OnCityReady();
            };
            Lib.SaveGame.OnAfterNewGame += (_, _) =>
            {
                ContractStore.Loading = false;
                WeaponController.OnCityReady();
                PresetDumper.OnCityReady();
            };
            Lib.Time.OnMinuteChanged += (_, _) => ContractTicker.Tick();
            Bribe.Register();
            Heat.CorruptCop.Register();
            Heat.Interrogation.Register();
            Heat.WitnessTalk.Register();
            Fixer.Register();
            Campaign.Register();
            WeaponController.Init();

            if (!ClassInjector.IsTypeRegisteredInIl2Cpp<HitmanBehaviour>())
                ClassInjector.RegisterTypeInIl2Cpp<HitmanBehaviour>();
            AddComponent<HitmanBehaviour>();
            if (HitmanConfig.DevHotkeys.Value)
            {
                DevHotkeys.Init();
                DevMenu.Init();
            }

            Log.LogInfo($"{Name} {Version} loaded. Weapons={HitmanConfig.WeaponsEnabled.Value}, dev mode={HitmanConfig.DevMode} " +
                        $"(hotkeys={HitmanConfig.DevHotkeys.Value}, probes={HitmanConfig.DevProbes.Value})");
        }
    }
}
