//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftManager : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("RIFT SETTINGS")] 
    [Tooltip("Creates a rift when the two marker transform variables are set")]
    [SerializeField] private bool createRiftOnMarkersPinned;
    [Header("Size")]
    [Tooltip("Max size a rift can *expand* to in worldspace units")]
    [SerializeField] private float maxRiftWidth = 30;
    [Tooltip("Max size an *inverted* rift can expand to in the negative direction")]
    [SerializeField] private float minRiftWidth = -30;
    [Tooltip("This is to prevent physics bugs if nullspace scales too close to 0 without being 0")]
    [SerializeField] private float minAbsoluteRiftWidth = 0.15f;
    [Header("Speed")]
    [Tooltip("The speed of the rift when it starts moving")]
    [SerializeField] private float minRiftSpeed = 0.5f;
    [Tooltip("The maximum speed of the rift when it moves")]
    [SerializeField] private float maxRiftSpeed = 6f;
    [Tooltip("How quickly the rift picks up in speed while moving")]
    [SerializeField] private float riftAcceleration = 2f;
    
    [Header("RIFT VISUALS")] 
    [Tooltip("The game object that is used to represent the visual planes of the rift")]
    public GameObject visualPlanePrefab;
    [Tooltip("The material used to represent geometry exposed by a total null collapse")]
    public Material nullSpaceMaterial;
    [Tooltip("")]
    public Graphics_RiftPreviewEffects riftPreviewEffects;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Header("CURRENT RIFT DATA")]
    [Tooltip("Width of the rift when it was first placed")]
    public static float riftStartingWidth;
    [Tooltip("Direction the rift space is facing (this is the line the rift moves along when expanding and contracting)")]
    public static Vector3 riftNormal;
    [Tooltip("The starting position of the null space container, used to restore its position after scaling the rift")]
    public static Vector3 riftNullSpaceStartingPosition;
    [Tooltip("Current percent scaling of the rift (the local scale)")]
    public static float currentRiftPercent;
    [Tooltip("Current width after applying percent scale")]
    public static float currentRiftWidth;
    [Tooltip("How fast the rift planes are currently moving")]
    private static float currentRiftMoveSpeed;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private bool riftActive;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Header("HELPER CLASSES")] 
    [Tooltip("Handles the rift states")]
    [Box] public RiftManager_StateHandler stateHandler;
    [Tooltip("Controls space containers and rift movement")]
    [Box] public RiftManager_SpaceController spaceController;
    [Tooltip("Handles rift positioning and mesh slicing")]
    [Box] public RiftManager_GeometryHandler geometryHandler;
    [Tooltip("Handles actor restoring")]
    [Box] public RiftManager_ActorHandler actorHandler;
    
    [Header("REFERENCES")]
    [Tooltip("The positions where the rift planes will be created")]
    private Transform markerA, markerB;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        stateHandler = new RiftManager_StateHandler();
        spaceController = new RiftManager_SpaceController();
        geometryHandler = new RiftManager_GeometryHandler(spaceController);
        actorHandler = new RiftManager_ActorHandler();
    }

    private void Update()
    {
        // Create rift when markers pinned
        if (createRiftOnMarkersPinned && IsMarkersPinned() && RiftManager_StateHandler.currentState == RiftState.None)
        {
            CreateRift(markerA, markerB);
        }
        
        // Erase rift when marker transforms are destroyed
        else if (createRiftOnMarkersPinned && !IsMarkersPinned() && RiftManager_StateHandler.currentState != RiftState.None)
        {
            DestroyRift();
        }
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private bool IsMarkersPinned()
    {
        return (markerA != null && markerB != null);
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Slice the world and assign all objects to space containers
    /// </summary>
    public void CreateRift(Transform _markerA, Transform _markerB)
    {
        stateHandler.SetState(RiftState.Preview);
        geometryHandler.SetRiftPlanesVisible(true);
        geometryHandler.PositionCutPlanes(_markerA, _markerB);
        geometryHandler.PerformCutProcedure();
        spaceController.ReparentGeometryToSpaceContainers();
        spaceController.ReparentActorsToSpaceContainers();
    }

    /// <summary>
    /// Unslice the world and remove objects from space containers
    /// </summary>
    public void DestroyRift()
    {
        stateHandler.SetState(RiftState.None);
        geometryHandler.SetRiftPlanesVisible(false);
        SetRiftPercentage(1);
        geometryHandler.RestoreCutGeometry();
        spaceController.RemoveObjectsFromSpaceContainers();
        actorHandler.RestoreActors();
    }

    /// <summary>
    /// Lerp the movement of the rift and space containers by a specified amount in meters
    /// </summary>
    public void MoveRiftByDistance(float _distance)
    {
        
    }

    /// <summary>
    /// Force set the exact percentage of rift collapse
    /// </summary>
    public void SetRiftPercentage(float _distance)
    {
        
    }

    /// <summary>
    /// Assign a controller, like the Geogun, to control the rift manager
    /// </summary>
    public void RegisterRiftController(GameObject _linkedRiftController)
    {
        
    }

    #endregion
}
