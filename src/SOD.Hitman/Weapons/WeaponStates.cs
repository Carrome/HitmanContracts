namespace SODHitman.Weapons
{
    // Adapted from "Guns" by Spyraxi (ReveredRedHood), MIT License (see THIRD_PARTY_NOTICES.md).
    internal enum WeaponPrimaryState
    {
        NotAiming,
        Aiming,
        Firing,
    }

    internal enum WeaponSecondaryState
    {
        NotEquipped,
        NotAiming,
        Aiming,
    }

    internal enum PistolKind
    {
        None,
        Tranquilizer,
        Lethal,
    }
}
