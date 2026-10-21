using System;
using System.Linq;
using UnityEngine;
using static SODHitman.Dev.Describe;

namespace SODHitman.Weapons
{
    // The pistols are only found in the world or bought (decided 19/10/2026): both go on the stock list of the
    // shops in Weapons.SellAtShops. The silenced Lazarus can already turn up as a killer's weapon.
    internal static class WeaponShops
    {
        internal static void Stock(params InteractablePreset[] items)
        {
            var shops = HitmanConfig.SellAtShops.Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var all = AssetLoader.Instance != null ? AssetLoader.Instance.allPresets : null;
            if (shops.Length == 0 || all == null) return;

            for (int i = 0; i < all.Count; i++)
            {
                var menu = all[i] != null ? all[i].TryCast<MenuPreset>() : null;
                if (menu == null || !shops.Contains(menu.name) || menu.itemsSold == null) continue;
                foreach (var item in items)
                {
                    if (item == null || Each(menu.itemsSold).Any(x => x != null && x.name == item.name)) continue;
                    menu.itemsSold.Add(item);
                    Plugin.Logger.LogInfo($"[Weapons] {item.name} now sold at {menu.name} (price {item.value.x:0}-{item.value.y:0})");
                }
            }
        }

        // Dev: both pistols into the inventory.
        internal static void GiveBoth()
        {
            foreach (var preset in new[] { WeaponController.Tranquilizer, WeaponController.Lethal })
                Give(preset);
        }

        // Dev / shops: one item into the inventory (it drops at the player's feet when the inventory is full).
        internal static bool Give(InteractablePreset preset)
        {
            if (preset == null) return false;
            var player = Player.Instance;
            var item = InteractableCreator.Instance.CreateWorldInteractable(preset, player, player, null,
                player.transform.position, Vector3.zero, null, null, "");
            bool picked = item != null && FirstPersonItemController.Instance.PickUpItem(item, false, false, true, true, true);
            Plugin.Logger.LogInfo($"[Weapons] Gave {preset.name}: created={item != null}, inInventory={picked}");
            return picked;
        }
    }
}
