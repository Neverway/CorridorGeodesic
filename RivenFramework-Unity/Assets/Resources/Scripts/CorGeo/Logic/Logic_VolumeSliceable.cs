//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Logic_VolumeSliceable : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=

    [Tooltip("The original mesh to get the 3 pieces from")]
    [SerializeField] CorGeo_SliceableMesh sliceableMesh;
    List<VolumeTriggerEvent> volumes = new List<VolumeTriggerEvent> ();

    [Tooltip (
    "This event will only fire when something first enters (does not refire for subsequent entries until unoccupied)")]
    public UnityEvent onFirstOccupied;

    [Tooltip ("This event will only fire when last one leaves")]
    public UnityEvent onFirstUnoccupied;

    //todo: On rift create/destroy events, call GetAllMeshes on the sliceable, and then get VolumeTriggers from those.
    // Then we can determine wether to send the OnFirstOccupied/OnFirstUnoccupied events from this script
    // which will then trigger logic.
    // Q: HOW do I trigger logic properly??

    //=-----------------=
    // Private Variables
    //=-----------------=

    [SerializeField] private int occupiedVolumes = 0;
    public LogicOutput<bool> onOccupied;
    public bool hasBeenTriggered = false;

    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        RiftManager_StateHandler.OnRiftCreated.AddListener (OnRiftCreated);
        RiftManager_StateHandler.OnRiftDestroyed.AddListener (OnRiftDestroyed);
        GetVolumesFromSlices ();
    }

    private void OnDestroy ()
    {
        RiftManager_StateHandler.OnRiftDestroyed.RemoveListener (OnRiftDestroyed);
        RiftManager_StateHandler.OnRiftCreated.RemoveListener (OnRiftCreated);
    }

    private void OnRiftCreated ()
    {
        Debug.Log ("OnRiftCreated");
        GetVolumesFromSlices ();
    }

    private void OnRiftDestroyed ()
    {
        Debug.Log ("OnRiftDestroyed");
        GetVolumesFromSlices ();
    }



    //=-----------------=
    // Internal Functions
    //=-----------------=

    private void GetVolumesFromSlices ()
    {
        volumes.Clear ();
        occupiedVolumes = 0;
        foreach (CorGeo_SliceableMesh sliceable in sliceableMesh.GetAllMeshes ())
        {
            VolumeTriggerEvent v = sliceable.gameObject.GetComponent<VolumeTriggerEvent> ();
            if (v != null)
            {
                volumes.Add (v);
                if (v.onOccupied)
                {
                    occupiedVolumes++;
                }
                v.onFirstOccupied.AddListener (OnVolumeOccupied);
                v.onFirstUnoccupied.AddListener (OnVolumeUnoccupied);
            }
        }
    }

    private void OnVolumeOccupied ()
    {
        bool wasOccupied = IsOccupied();
        occupiedVolumes++;
        bool occupied = IsOccupied ();
        if (occupied && wasOccupied == false)
        {
            hasBeenTriggered = true;
            onFirstOccupied.Invoke ();
            Debug.Log ("VolumeSliceable Occupied");
        }
        onOccupied.Set (occupied);
    }

    private void OnVolumeUnoccupied ()
    {
        occupiedVolumes--;
        bool occupied = IsOccupied () ;
        if (occupied == false)
        {
            onFirstUnoccupied.Invoke ();
            Debug.Log ("VolumeSliceable Unoccupied");
        }
        onOccupied.Set (occupied);
    }

    private bool IsOccupied ()
    {
        return occupiedVolumes > 0;
    }

    //=-----------------=
    // External Functions
    //=-----------------=
}