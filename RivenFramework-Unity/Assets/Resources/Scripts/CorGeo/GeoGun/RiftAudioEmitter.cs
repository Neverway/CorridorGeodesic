//===================== (Neverway 2024) Written by Connorses, Soulex =====================
//
// Purpose: Positions an audio source so that sounds can emanate from the rift-planes.
// Notes: Ported this from the old project, I'm commenting out the FMOD bits. For now.
//
//=============================================================================

using FMOD.Studio;
using FMODUnity;

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using Unity.VisualScripting;

public class RiftAudioEmitter : MonoBehaviour
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

    //   FMOD audio instances
    private EventInstance riftIdleInstance;
    private EventInstance riftCollapseInstance;
    private EventInstance riftExpandInstance;

    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GameObject activeCamera;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        // Whoops, we need this reference, but it's not here!
        if (riftManager is null) riftManager = FindObjectOfType<RiftManager> ();

        riftIdleInstance = Audio_FMODAudioManager.CreateInstance(Audio_FMODEvents.Instance.riftIdle);
        riftCollapseInstance = Audio_FMODAudioManager.CreateInstance(Audio_FMODEvents.Instance.riftCollapsing);
        riftExpandInstance = Audio_FMODAudioManager.CreateInstance(Audio_FMODEvents.Instance.riftExpanding);
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
        riftIdleInstance.release();
        riftCollapseInstance.release();
        riftExpandInstance.release();
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
            riftCollapseInstance.start();
        }
        else
        {
            riftCollapseInstance.stop (FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        if (expandStart)
        {
            riftExpandInstance.start ();
        }
        else
        {
            riftExpandInstance.stop (FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    //Determines the position rift sound effects should come from.
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

        Vector3 camPos = GetCameraPosition ();

        //places audio on nearest part of rift plane, unless we are inside the rift
        //in which case it places audio on the camera.
        RiftSpace space = riftManager.GetRiftSpaceOfPoint(camPos);

        if (space == RiftSpace.A)
        {
            return RiftManager.cutPlaneA.ClosestPointOnPlane (camPos);
        }
        if (space == RiftSpace.B)
        {
            return RiftManager.cutPlaneB.ClosestPointOnPlane(camPos);
        }
        if (space == RiftSpace.NULLSpace)
        {
            return camPos;
        }
        //fallback
        return camPos;
    }

    private void Update3DAttributes()
    {
        FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(transform.position);

        riftIdleInstance.set3DAttributes(attributes);
        riftCollapseInstance.set3DAttributes(attributes);
        riftExpandInstance.set3DAttributes(attributes);
    }
    private void OnRiftCreated()
    {
        Debug.Log ("Sound: rift created");
        //Put code here for when rift first starts moving
        Audio_FMODAudioManager.PlayOneShot(Audio_FMODEvents.Instance.riftSpawned);

        riftIdleInstance.start();
    }
    private void OnRiftRemoved()
    {
        //Debug.Log ("Sound: rift removed");
        //Put code here for when rift is cleared.
        Audio_FMODAudioManager.PlayOneShot(Audio_FMODEvents.Instance.riftKilled);

        riftIdleInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private Vector3 GetCameraPosition ()
    {
        if (!activeCamera)
        {
            var localPlayer = GameInstance.Get<GI_PawnManager>().localPlayerCharacter;
            if (localPlayer) activeCamera = localPlayer.GetComponentInChildren<Camera>().gameObject;
            if (!activeCamera)
            {
                Debug.LogWarning("RiftAudioEmitter could not find camera (If this happens repeatedly, it's probably an issue)");
                return Vector3.zero;
            }
        }
        return activeCamera.transform.position;
    }

    //=-----------------=
    // External Functions
    //=-----------------=
}