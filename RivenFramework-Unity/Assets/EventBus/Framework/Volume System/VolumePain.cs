using RivenFramework.EventBus;
using UnityEngine;

/// <summary>
/// A volume that inflicts damage or healing when a health receiver enters the area
/// </summary>
public class VolumePain : BVolume
{
    [Tooltip("The amount of damage to apply per tick to actors within the volume. Negative values heal.")]
    public int damageAmount = 10;

    [Tooltip("If enabled, the pain volume only hurts actors when they enter the volume instead of repeatedly while inside the volume")]
    public bool onlyDamageOnEnter;
    
    public override void OnVolumeEnter(BActor _actor)
    {
        base.OnVolumeEnter(_actor);
        if (onlyDamageOnEnter is false) return;
        EventBus<Event_ModifyHealth>.Publish(new Event_ModifyHealth
        {
            DamageAmount = damageAmount,
            AffectedActorsFilters = affectedActorsFilters,
            Target = _actor,
            Source = owner
        });
    }

    public override void OnVolumeStay(BActor _actor)
    {
        base.OnVolumeStay(_actor);
        if (onlyDamageOnEnter) return;
        EventBus<Event_ModifyHealth>.Publish(new Event_ModifyHealth
        {
            DamageAmount = damageAmount,
            AffectedActorsFilters = affectedActorsFilters,
            Target = _actor,
            Source = owner
        });
    }

    public override void OnVolumeExit(BActor _actor)
    {
        base.OnVolumeExit(_actor);
    }

    public override void OnBecomeOccupied(BActor _actor)
    {
        base.OnBecomeOccupied(_actor);
    }

    public override void OnBecomeUnoccupied(BActor _actor)
    {
        base.OnBecomeUnoccupied(_actor);
    }
}
