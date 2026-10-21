using System;
using System.Linq;
using SOD.Common;
using SOD.Common.Helpers;
using UnityEngine;
using static SODHitman.Dev.Describe;

namespace SODHitman.Weapons
{
    // Two silenced pistols the player can fire: a tranquilizer (darts put citizens to sleep) and a lethal one.
    // Infinite ammo and no reload (the game has no reload system or animation; decided 19/10/2026).
    //
    // Merged from the author's TranquilizerPistol mod. Aiming/firing flow, aim zoom and the Attack->Fire /
    // Throw->Aim prompt swaps are adapted from "Guns" by Spyraxi (ReveredRedHood), MIT License.
    internal static class WeaponController
    {
        private const string ActionDictionary = "ui.interaction";
        private const string ActionAim = "Aim";
        private const string ActionAttack = "Attack";
        private const string ActionFire = "Fire";
        private const string ActionThrow = "Throw";
        private const string Primary = "Primary";
        private const string Secondary = "Secondary";
        private const string IllegalKey = "sod.hitman.weapons";
        private const float RecoilSpeed = 1f;
        private const float EjectProjection = 6f;
        private const float AimTweenSeconds = 0.25f;
        // A hit that lands this long after a shot still counts as that shot (in case bullets are not instant).
        private const float HitWindowSeconds = 1f;

        private static readonly Vector3 PistolRotation = new(0f, -90f, 0f);
        private static readonly Vector3 RecoilPattern = new Vector3(1f, 1f, 0.5f).normalized;

        private static InteractablePreset tranquilizer;
        private static InteractablePreset lethal;
        private static bool ready;
        private static bool paused;
        private static bool fireHeld;
        private static float cooldownLeft;
        private static float tweenLeft;
        private static bool tweenEnter;
        private static PistolKind lastShotKind;
        private static float lastShotTime = -100f;
        private static WeaponPrimaryState primary;
        private static WeaponSecondaryState secondary;

        internal static InteractablePreset Tranquilizer => tranquilizer;
        internal static InteractablePreset Lethal => lethal;

        // ---- setup ----

        internal static void Init()
        {
            if (!HitmanConfig.WeaponsEnabled.Value) return;
            Lib.InputDetection.OnButtonStateChanged += OnButton;
            Lib.Time.OnGamePaused += (_, _) => paused = true;
            Lib.Time.OnGameResumed += (_, _) => paused = false;
        }

        internal static void OnCityReady()
        {
            ready = false;
            if (!HitmanConfig.WeaponsEnabled.Value) return;
            primary = WeaponPrimaryState.NotAiming;
            secondary = WeaponSecondaryState.NotEquipped;
            fireHeld = false;
            cooldownLeft = 0f;
            WeaponPatches.ResetCounters();
            Weapons.Tranquilizer.ResetSession();
            Weapons.Tranquilizer.WakeOversleepers();

            Lib.DdsStrings.AddOrUpdateEntries(ActionDictionary,
                (ActionAim, Text.L("Mirar", "Aim")),
                (ActionFire, Text.L("Atirar", "Fire")));

            tranquilizer = FindPreset(HitmanConfig.TranquilizerPreset.Value);
            lethal = FindPreset(HitmanConfig.LethalPreset.Value);
            foreach (var preset in new[] { tranquilizer, lethal })
            {
                if (preset == null) continue;
                // Make the pistol point forward in the player's hand.
                preset.fpsItemOffset = Vector3.zero;
                preset.fpsItemRotation = PistolRotation;
            }
            ready = tranquilizer != null || lethal != null;
            Plugin.Logger.LogInfo($"[Weapons] Tranquilizer={N(tranquilizer) ?? "MISSING " + HitmanConfig.TranquilizerPreset.Value}, " +
                                  $"lethal={N(lethal) ?? "MISSING " + HitmanConfig.LethalPreset.Value}, weapon={N(lethal != null ? lethal.weapon : tranquilizer?.weapon)}");
            WeaponShops.Stock(tranquilizer, lethal);
            Arsenal.OnCityReady();
            Contracts.Fixer.OnCityReady();
            ready = ready || Arsenal.Items.Any();
        }

        // Custom presets loaded by AssetBundleLoader live in AssetLoader.allPresets, next to the vanilla ones.
        internal static InteractablePreset FindPreset(string name)
        {
            var all = AssetLoader.Instance != null ? AssetLoader.Instance.allPresets : null;
            if (all == null || string.IsNullOrEmpty(name)) return null;
            for (int i = 0; i < all.Count; i++)
            {
                var preset = all[i] != null ? all[i].TryCast<InteractablePreset>() : null;
                if (preset != null && (preset.name == name || preset.presetName == name)) return preset;
            }
            return null;
        }

        // ---- what the player holds ----

        internal static Interactable HeldItem()
        {
            var bio = BioScreenController.Instance;
            var slot = bio != null ? bio.selectedSlot : null;
            return slot != null ? slot.GetInteractable() : null;
        }

        private static InteractablePreset HeldPreset()
        {
            var item = HeldItem();
            return item != null ? item.preset : null;
        }

        internal static PistolKind HeldKind()
        {
            var preset = HeldPreset();
            if (preset == null) return PistolKind.None;
            if (tranquilizer != null && preset.presetName == tranquilizer.presetName) return PistolKind.Tranquilizer;
            if (lethal != null && preset.presetName == lethal.presetName) return PistolKind.Lethal;
            if (Arsenal.IsLoudFirearm(preset)) return PistolKind.Lethal; // v0.8: the game's other firearms
            return PistolKind.None;
        }

        // Which pistol fired the hit being processed right now (None for punches, bashes and NPC shots).
        internal static PistolKind GunshotKind()
        {
            if (primary == WeaponPrimaryState.Firing) return lastShotKind;
            if (UnityEngine.Time.time - lastShotTime <= HitWindowSeconds && HeldKind() == lastShotKind) return lastShotKind;
            return PistolKind.None;
        }

        // ---- input (aim with Secondary, fire with Primary) ----

        private static void OnButton(object sender, InputDetectionEventArgs e)
        {
            if (!ready || (e.ActionName != Primary && e.ActionName != Secondary)) return;
            if (Dev.DevMenu.IsOpen) return; // clicks belong to the test menu

            if (HeldKind() == PistolKind.None)
            {
                SetSecondary(WeaponSecondaryState.NotEquipped);
                StartAimTween(false);
                return;
            }

            if (e.ActionName == Primary) fireHeld = e.IsDown;

            switch (secondary)
            {
                case WeaponSecondaryState.NotAiming:
                    if (e.ActionName == Secondary && e.IsDown)
                    {
                        SetSecondary(WeaponSecondaryState.Aiming);
                        SetPrimary(WeaponPrimaryState.Aiming);
                        StartAimTween(true);
                    }
                    break;

                case WeaponSecondaryState.Aiming:
                    if (e.ActionName == Secondary && !e.IsDown)
                    {
                        SetSecondary(WeaponSecondaryState.NotAiming);
                        SetPrimary(WeaponPrimaryState.NotAiming);
                        StartAimTween(false);
                    }
                    else if (fireHeld)
                    {
                        Fire();
                    }
                    break;
            }
        }

        internal static void OnItemDrawn()
        {
            if (!ready) return;
            SetPrimary(WeaponPrimaryState.NotAiming);
            SetSecondary(HeldKind() != PistolKind.None ? WeaponSecondaryState.NotAiming : WeaponSecondaryState.NotEquipped);
        }

        // ---- firing ----

        private static void Fire()
        {
            if (!ready || paused || cooldownLeft > 0f) return;
            var kind = HeldKind();
            if (kind == PistolKind.None) return;
            if (secondary != WeaponSecondaryState.Aiming || primary != WeaponPrimaryState.Aiming) return;
            var weapon = HeldPreset().weapon;
            if (weapon == null) return;

            var player = Player.Instance;
            float range = weapon.GetAttackValue(MurderWeaponPreset.AttackValue.range, player);
            float refire = weapon.GetAttackValue(MurderWeaponPreset.AttackValue.fireDelay, player) * HitmanConfig.WeaponDelayFactor.Value;
            float accuracy = HitmanConfig.WeaponAccuracy.Value < 0f
                ? weapon.GetAttackValue(MurderWeaponPreset.AttackValue.accuracy, player)
                : HitmanConfig.WeaponAccuracy.Value;
            float damage = weapon.GetAttackValue(MurderWeaponPreset.AttackValue.damage, player);

            // Largely taken from Game.ShootFromPlayer (via Guns).
            var hand = FirstPersonItemController.Instance.rightHandObjectParent;
            var ejectPoint = hand.TransformPoint(weapon.itemRightLocalPos + Vector3.back * EjectProjection);
            var cam = CameraController.Instance.cam.transform;
            var aimFrom = cam.position;
            var aimForward = cam.TransformPoint(Vector3.forward);

            // A lethal shot is illegal before it lands, so whoever sees the kill counts as a witness.
            bool loud = Arsenal.IsLoudFirearm(HeldPreset());
            if (kind == PistolKind.Lethal) ApplyIllegalStatus(HitmanConfig.LethalIllegalWhenSeen.Value || loud);
            if (loud)
            {
                Arsenal.LastLoudShotTime = UnityEngine.Time.time;
                Arsenal.LastLoudShotPosition = player.transform.position;
            }

            SetPrimary(WeaponPrimaryState.Firing);
            lastShotKind = kind;
            lastShotTime = UnityEngine.Time.time;
            var muzzleFlash = weapon.muzzleFlash;
            if (!loud) weapon.muzzleFlash = null; // the two pistols are silenced; the other firearms flash
            try
            {
                Toolbox.Instance.Shoot(player, aimFrom, aimForward, range, accuracy, damage, weapon, false, ejectPoint, false, true);
            }
            finally
            {
                weapon.muzzleFlash = muzzleFlash;
            }

            if (HitmanConfig.WeaponRecoil.Value > float.Epsilon)
            {
                var recoil = new Vector3(Toolbox.Instance.Rand(-1f, 1f), Toolbox.Instance.Rand(-1f, 1f), Toolbox.Instance.Rand(-1f, 1f));
                recoil.Scale(RecoilPattern);
                player.fps.JoltCamera(recoil, HitmanConfig.WeaponRecoil.Value, RecoilSpeed);
            }

            if (kind == PistolKind.Tranquilizer) ApplyIllegalStatus(HitmanConfig.TranqIllegalWhenSeen.Value);

            cooldownLeft = refire;
            SetPrimary(WeaponPrimaryState.Aiming);
        }

        private static void ApplyIllegalStatus(bool illegalWhenSeen)
        {
            var player = Player.Instance;
            if (!illegalWhenSeen || player.isHome || (!player.isSeenByOthers && !IsHeardByOthers()))
            {
                Lib.PlayerStatus.RemoveIllegalStatusModifier(IllegalKey);
                return;
            }
            Lib.PlayerStatus.SetIllegalStatusModifier(IllegalKey, TimeSpan.FromSeconds(HitmanConfig.IllegalSecondsOnShot.Value), true);
        }

        private static bool IsHeardByOthers()
        {
            foreach (var icon in Each(InterfaceController.Instance.awarenessIcons))
                if (icon != null && icon.alertProgress > 0f) return true;
            return false;
        }

        // ---- per frame (HitmanBehaviour.Update): shot cooldown and aim zoom ----

        internal static void Update()
        {
            if (!ready || paused) return;
            float dt = UnityEngine.Time.deltaTime;
            if (cooldownLeft > 0f) cooldownLeft = Math.Max(0f, cooldownLeft - dt);
            if (tweenLeft > 0f)
            {
                tweenLeft -= dt;
                AdjustFov(1f - Math.Max(0f, tweenLeft) / AimTweenSeconds);
            }
        }

        private static void StartAimTween(bool enter)
        {
            tweenEnter = enter;
            tweenLeft = AimTweenSeconds;
        }

        private static void AdjustFov(float progress)
        {
            var cam = CameraController.Instance != null ? CameraController.Instance.cam : null;
            if (cam == null || Game.Instance == null) return;
            if (tweenEnter && HeldKind() == PistolKind.None)
            {
                StartAimTween(false);
                return;
            }
            float zoom = Arsenal.IsSniper(HeldPreset()) ? HitmanConfig.ArsenalSniperZoom.Value : HitmanConfig.WeaponZoom.Value;
            float target = tweenEnter ? Game.Instance.fov * (1f - zoom) : Game.Instance.fov;
            float current = cam.fieldOfView;
            cam.fieldOfView = current - (current - target) * progress;
        }

        // ---- prompts: Attack -> Fire while aiming, Throw -> Aim while holding a pistol ----

        private static void SetPrimary(WeaponPrimaryState value)
        {
            primary = value;
            SwapAction("FPSPunch", value == WeaponPrimaryState.Aiming || value == WeaponPrimaryState.Firing, primaryAction: true);
        }

        private static void SetSecondary(WeaponSecondaryState value)
        {
            secondary = value;
            SwapAction("FPSThrow", value == WeaponSecondaryState.NotAiming || value == WeaponSecondaryState.Aiming, primaryAction: false);
        }

        private static void SwapAction(string actionPreset, bool weaponMode, bool primaryAction)
        {
            if (!ready) return;
            var held = HeldPreset();
            var fpsItem = held != null ? held.fpsItem : null;
            if (fpsItem == null) return;

            foreach (var a in Each(fpsItem.actions))
            {
                if (a == null || a.action == null || a.action.presetName != actionPreset) continue;
                if (primaryAction)
                {
                    a.interactionName = weaponMode ? ActionFire : ActionAttack;
                    a.useCameraJolt = !weaponMode;
                    a.mainSpecialAction = weaponMode ? FirstPersonItem.SpecialAction.none : FirstPersonItem.SpecialAction.punch;
                }
                else
                {
                    a.interactionName = weaponMode ? ActionAim : ActionThrow;
                    a.action.throwObjectsAtTarget = !weaponMode;
                    a.mainSpecialAction = weaponMode ? FirstPersonItem.SpecialAction.none : FirstPersonItem.SpecialAction.attack;
                }
                InteractionController.Instance.UpdateInteractionText();
                return;
            }
        }
    }
}
