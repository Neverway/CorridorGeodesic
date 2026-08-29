using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles events for when _actors enter and leave a specified area
/// </summary>
public class BVolume : MonoBehaviour
{
    // The actor that created this volume, left blank if it's part of the level or unowned
    public BActor owner;
    // Only _actors within these groups trigger events on this volume, If left empty, all actors are valid
    [Polymorphic, SerializeReference] public List<ActorFilter> affectedActorsFilters;
    // The _actors that are currently triggering this volume
    public List<BActor> actorsInVolume;

    public event Action<BActor> onVolumeEnter;
    public event Action<BActor> onVolumeStay;
    public event Action<BActor> onVolumeExit;
    public event Action<BActor> onBecomeOccupied;
    public event Action<BActor> onBecomeUnoccupied;

    public void OnTriggerEnter(Collider _other)
    {
        var _actor = _other.GetComponentInParent<BActor>();
        
        // Exit if not an _actor
        if (_actor == null) return;
        
        // Exit if already in volume
        if (actorsInVolume.Contains(_actor)) return;

        // If using affectGroupsFilter
        if (affectedActorsFilters != null)
        {
            // Exit if it doesn't pass all filters
            var passedFilters = true;
            foreach (var filter in affectedActorsFilters)
            {
                passedFilters &= filter.PassesFilter(_actor);
            }

            if (passedFilters == false)
            {
                return;
            }
        }
        
        // Do stuff
        if (actorsInVolume.Any() is false)
        {
            OnBecomeOccupied(_actor);
        }
        actorsInVolume.Add(_actor);
        OnVolumeEnter(_actor);
    }

    public void OnTriggerStay(Collider _other)
    {
        var _actor = _other.GetComponentInParent<BActor>();
        
        // Exit if not an _actor
        if (_actor == null) return;

        // If using affectGroupsFilter
        if (affectedActorsFilters != null)
        {
            // Exit if it doesn't pass all filters
            var passedFilters = true;
            foreach (var filter in affectedActorsFilters)
            {
                passedFilters &= filter.PassesFilter(_actor);
            }

            if (passedFilters == false)
            {
                return;
            }
        }

        // Do stuff
        OnVolumeStay(_actor);
    }

    public void OnTriggerExit(Collider _other)
    {
        var _actor = _other.GetComponentInParent<BActor>();
        
        // Exit if not an _actor
        if (_actor == null) return;
        
        // Exit if not already in volume
        if (actorsInVolume.Contains(_actor) is false) return;

        // If using affectGroupsFilter
        if (affectedActorsFilters != null)
        {
            // Exit if it doesn't pass all filters
            var passedFilters = true;
            foreach (var filter in affectedActorsFilters)
            {
                passedFilters &= filter.PassesFilter(_actor);
            }

            if (passedFilters == false)
            {
                return;
            }
        }

        // Do stuff
        OnVolumeExit(_actor);
        actorsInVolume.Remove(_actor);
        if (actorsInVolume.Any() is false)
        {
            OnBecomeUnoccupied(_actor);
        }
    }

    /// <summary>
    /// Called when an _actor enters a volume
    /// </summary>
    /// <param name="_actor">The _actor that entered the volume</param>
    public virtual void OnVolumeEnter(BActor _actor)
    {
        onVolumeEnter?.Invoke(_actor);
    }
    
    /// <summary>
    /// Called when an _actor stays within a volume
    /// </summary>
    /// <param name="_actor">The _actor that has stayed in the volume</param>
    public virtual void OnVolumeStay(BActor _actor)
    {
        onVolumeStay?.Invoke(_actor);
    }
    
    /// <summary>
    /// Called when an _actor leaves a volume
    /// </summary>
    /// <param name="_actor">The _actor that left the volume</param>
    public virtual void OnVolumeExit(BActor _actor)
    {
        onVolumeExit?.Invoke(_actor);
    }

    /// <summary>
    /// Called when an _actor enters the volume, but it was previously empty
    /// </summary>
    /// <param name="_actor">The _actor that entered the volume</param>
    public virtual void OnBecomeOccupied(BActor _actor)
    {
        onBecomeOccupied?.Invoke(_actor);
    }

    /// <summary>
    /// Called when the last _actor has left the volume
    /// </summary>
    /// <param name="_actor">The last _actor that left the volume</param>
    public virtual void OnBecomeUnoccupied(BActor _actor)
    {
        onBecomeUnoccupied?.Invoke(_actor);
    }
}
