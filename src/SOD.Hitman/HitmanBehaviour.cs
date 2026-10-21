using System;
using UnityEngine;

namespace SODHitman
{
    // Component injected into IL2CPP only to get a per-frame Update(); the logic lives in plain C# classes.
    public class HitmanBehaviour : MonoBehaviour
    {
        public HitmanBehaviour(IntPtr ptr) : base(ptr) { }

        private void Update()
        {
            Weapons.WeaponController.Update();
            if (HitmanConfig.WeaponsEnabled.Value) Weapons.Tranquilizer.Update();
            if (HitmanConfig.DevHotkeys.Value) Dev.DevHotkeys.Update();
        }
    }
}
