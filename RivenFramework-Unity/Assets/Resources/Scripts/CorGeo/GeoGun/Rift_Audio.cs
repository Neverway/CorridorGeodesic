//===================== (Neverway 2024) Written by Connorses, Soulex =====================
//
// Purpose: Positions an audio source so that sounds can emanate from the rift-planes.
// Notes: Ported this from the old project, I'm commenting out the FMOD bits. For now.
//
//=============================================================================

//using FMOD.Studio;
//using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neverway.Framework.PawnManagement;
using Unity.VisualScripting;

public class Rift_Audio : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private Transform playerTransform;
    private CorGeo_Actor playerActorData;

    private RiftManager riftManager;

    //   FMOD audio instances (unused)
    //private EventInstance riftIdleInstance;
    //private EventInstance riftCollapseInstance;
    //private EventInstance riftExpandInstance;

    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        // Whoops, we need this reference, but it's not here!
        if (riftManager is null) riftManager = FindObjectOfType<RiftManager> ();

        //riftIdleInstance = Audio_FMODAudioManager.CreateInstance(Audio_FMODEvents.Instance.riftIdle);
        //riftCollapseInstance = Audio_FMODAudioManager.CreateInstance(Audio_FMODEvents.Instance.riftCollapsing);
        //riftExpandInstance = Audio_FMODAudioManager.CreateInstance(Audio_FMODEvents.Instance.riftExpanding);
    }
    private void OnEnable()
    {
        RiftManager_StateHandler.OnStateChanged += OnStateChanged;
    }
    private void OnDisable()
    {
        RiftManager_StateHandler.OnStateChanged -= OnStateChanged;
    }
    private void OnDestroy()
    {
        //riftIdleInstance.release();
        //riftCollapseInstance.release();
        //riftExpandInstance.release();
    }

    private void Update()
    {
        Update3DAttributes();

        if (riftManager.stateHandler.currentState.GetType() == typeof(RiftState_None) )
        {
            transform.position = GetCameraPosition();
            return;
        }

        transform.position = GetAudioClosestPosition();
    }
    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void OnStateChanged ()
    {
        // Whoops, we need this reference, but it's not here!
        if (riftManager is null) riftManager = FindObjectOfType<RiftManager> ();
        // Still didn't find it? Okay, stop everything else
        if (riftManager is null) return;

        if (riftManager.stateHandler.currentState.GetType () != typeof (RiftState_None) && riftManager.stateHandler.previousState.GetType () == typeof (RiftState_None))
        {
            OnRiftCreated ();
        }

        bool collapseStart = false;
        bool expandStart = false;

        var state = riftManager.stateHandler.currentState.GetType ();

        if (state == typeof (RiftState_None))
        {
            OnRiftRemoved ();
        }
        else if (state == typeof (RiftState_Preview))
        {

        }
        else if (state == typeof (RiftState_Collapsing))
        {
            collapseStart = true;
        }
        else if (state == typeof (RiftState_Closed))
        {
        }
        else if (state == typeof (RiftState_Expanding))
        {
            expandStart = true;
        }
        else if (state == typeof (RiftState_Idle))
        {
        }

        if (collapseStart)
        {
            //riftCollapseInstance.start();
            //todo: play collapse loop
        }
        else
        {
            //riftCollapseInstance.stop (FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            //todo: stop collapse loop
        }

        if (expandStart)
        {
            //riftExpandInstance.start ();
            //todo: play expand loop
        }
        else
        {
            //riftExpandInstance.stop (FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            //todo: stop expand loop
        }
    }
    private Vector3 GetAudioClosestPosition()
    {
        if (playerActorData == null)
        {
            var player = FindAnyObjectByType<FPPawn_Player> ();
            if (player == null)
            {
                return GetCameraPosition ();
            }
            playerTransform = player.transform;
            playerActorData = player.gameObject.GetComponent<CorGeo_Actor> ();
            if (playerActorData == null) {
                return GetCameraPosition ();
            }
        }
        playerActorData.DetermineRiftSpace ();
        if (playerActorData.riftSpace == RiftSpace.NULLSpace)
        {
            return GetCameraPosition ();
        }

        Vector3 camPos = GetCameraPosition ();

        Vector3 closestPoint = camPos;

        if(playerActorData.riftSpace == RiftSpace.NULLSpace)
            return closestPoint;

        Vector3 planeAAlignment = RiftManager.cutPlaneA.ClosestPointOnPlane(camPos);
        Vector3 planeBAlignment = RiftManager.cutPlaneB.ClosestPointOnPlane(camPos);

        if ((planeAAlignment - camPos).sqrMagnitude < (planeBAlignment - camPos).sqrMagnitude)
            closestPoint = planeAAlignment;
        else
            closestPoint = planeBAlignment;

        return closestPoint;
    }
    private void Update3DAttributes()
    {
        //FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(transform.position);

        //riftIdleInstance.set3DAttributes(attributes);
        //riftCollapseInstance.set3DAttributes(attributes);
        //riftExpandInstance.set3DAttributes(attributes);
    }
    private void OnRiftCreated()
    {
        Debug.Log ("Sound: rift created");
        //Put code here for when rift first starts moving
        //Audio_FMODAudioManager.PlayOneShot(Audio_FMODEvents.Instance.riftSpawned);

        //riftIdleInstance.start();
        
        //todo: play riftSpawned sound
    }
    private void OnRiftRemoved()
    {
        Debug.Log ("Sound: rift removed");
        //Put code here for when rift is cleared.
        //Audio_FMODAudioManager.PlayOneShot(Audio_FMODEvents.Instance.riftKilled);

        //riftIdleInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        //todo: play riftKilled sound
    }

    private Vector3 GetCameraPosition ()
    {
        return Camera.current.transform.position;
    }

    //=-----------------=
    // External Functions
    //=-----------------=
}