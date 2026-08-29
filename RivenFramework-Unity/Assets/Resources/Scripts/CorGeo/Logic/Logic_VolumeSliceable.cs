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

public class Logic_VolumeSliceable : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=

    [Tooltip("The original mesh to get the 3 pieces from")]
    [SerializeField] CorGeo_SliceableMesh sliceableMesh;

    //todo: On rift create/destroy events, call GetAllMeshes on the sliceable, and then get VolumeTriggers from those.
    // Then we can determine wether to send the OnFirstOccupied/OnFirstUnoccupied events from this script
    // which will then trigger logic.
    // Q: HOW do I trigger logic properly??

    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        RiftManager_StateHandler.OnRiftCreated += OnRiftCreated ();
        RiftManager_StateHandler.OnRiftDestroyed += OnRiftDestroyed ();
    }

    private RiftManager_StateHandler.RiftEvent OnRiftCreated ()
    {
        throw new NotImplementedException ();
    }

    private RiftManager_StateHandler.RiftEvent OnRiftDestroyed ()
    {
        throw new NotImplementedException ();
    }



    //=-----------------=
    // Internal Functions
    //=-----------------=

    private void GetVolumesFromSlices ()
    {
        List<VolumeTriggerEvent> volumes = new List<VolumeTriggerEvent>();
        foreach (CorGeo_SliceableMesh sliceable in sliceableMesh.GetAllMeshes ())
        {
            VolumeTriggerEvent v = sliceable.gameObject.GetComponent<VolumeTriggerEvent> ();
            if (v != null)
            {
                volumes.Add (v);
            }
        }
    }

    //=-----------------=
    // External Functions
    //=-----------------=
}