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

public class RiftManager : MonoBehaviour, ILoggable
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [field: SerializeField] public bool EnableRuntimeLogging { get; set; }
    
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
    [SerializeField] public float maxRiftSpeed = 6f;
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
    [HideInInspector] public Transform markerA, markerB;
    [Tooltip("The mathematical plane where the rift is cut")]
    [HideInInspector] public static Plane cutPlaneA, cutPlaneB;

    [Header("REFERENCES")] 
    [Tooltip("The script that is currently controlling this rift manager")]
    private RiftController linkedRiftController;
    [Tooltip("If either collapseHeld or expandHeld is enabled, the rift will attempt to move")]
    private bool collapseHeld, expandHeld, expandDueToCrush;
    
    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        stateHandler = new RiftManager_StateHandler(this);
        spaceController = new RiftManager_SpaceController(this);
        geometryHandler = new RiftManager_GeometryHandler(this, spaceController);
        actorHandler = new RiftManager_ActorHandler(this);
    }

    private void Update()
    {
        // Create rift when markers pinned
        if (createRiftOnMarkersPinned && IsMarkersPinned() && stateHandler.IsState<RiftState_None>())
        {
            CreateRift(markerA, markerB);
        }
        
        // Erase rift when marker transforms are destroyed
        else if (createRiftOnMarkersPinned && !IsMarkersPinned() && !stateHandler.IsState<RiftState_None>())
        {
            DestroyRift();
        }

        // Handle rift inputs
        UpdateState();
        stateHandler.Update();
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    
    private bool IsMarkersPinned()
    {
        return (markerA != null && markerB != null);
    }

    /// <summary>
    /// Sets the state machine state based on the rift controller inputs
    /// </summary>
    private void UpdateState()
    {
        if (!riftActive) return;

        // Expand due to crush
        if (expandDueToCrush) { stateHandler.SetState<RiftState_ExpandingFromCrush>(); } 
        // Collapsing
        else if (collapseHeld) { stateHandler.SetState<RiftState_Collapsing>(); }
        // Expanding
        else if (expandHeld) { stateHandler.SetState<RiftState_Expanding>(); }
        // Idling
        else { stateHandler.SetState<RiftState_Idle>(); }
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Slice the world and assign all objects to space containers
    /// </summary>
    public async void CreateRift(Transform _markerA, Transform _markerB)
    {
        this.Log($"CreateRift called (_markerA: '{_markerA}', _markerB: '{_markerB}')");
        stateHandler.SetState<RiftState_Preview>();
        geometryHandler.SetRiftPlanesVisible(true);
        geometryHandler.PositionCutPlanes(_markerA, _markerB);
        await geometryHandler.PerformCutProcedure();
        spaceController.ReparentGeometryToSpaceContainers();
        spaceController.ReparentActorsToSpaceContainers();
        riftActive = true;
    }

    /// <summary>
    /// Unslice the world and remove objects from space containers
    /// </summary>
    public void DestroyRift()
    {
        this.Log("DestroyRift called");
        stateHandler.SetState<RiftState_None>();
        geometryHandler.SetRiftPlanesVisible(false);
        SetRiftPercentage(1);
        geometryHandler.RestoreCutGeometry();
        spaceController.RemoveObjectsFromSpaceContainers();
        actorHandler.RestoreActors();
        riftActive = false;
    }

    /// <summary>
    /// Lerp the movement of the rift and space containers by a specified amount in meters
    /// </summary>
    public void MoveRiftByDistance(float _distance)
    {
        this.Log($"MoveRiftByDistance called (_distance: '{_distance}')");
        
        // Keep from expanding if allowExpandingRift is false
        if (!linkedRiftController.allowExpandingRift && currentRiftWidth + _distance > riftStartingWidth)
        {
            _distance = 0;
        }

        if (linkedRiftController.collapseBehavior == Item_Utility_Geogun.CollapseBehavior.Default)
        {
            if (_distance < 0 && currentRiftWidth + _distance < minAbsoluteRiftWidth)
            {
                SetRiftPercentage(0);
                return;
            }
            if (_distance > 0 && currentRiftWidth == 0)
            {
                SetRiftPercentage(1/riftStartingWidth * minAbsoluteRiftWidth);
            }
        }

        if (currentRiftWidth + _distance < minRiftWidth)
        {
            currentRiftWidth = minRiftWidth;
        }
        
        
        float percentChange = 1 / riftStartingWidth * _distance;

        // Does anything use this value? ~Liz
        //currentRiftOffset = (currentRiftWidth - riftStartingWidth) * riftNormal;

        SetRiftPercentage (currentRiftPercent + percentChange);
        // This value is probably not needed anymore for the new system
        //riftIsMoving = true;
    }

    /// <summary>
    /// Force set the exact percentage of rift collapse
    /// </summary>
    public void SetRiftPercentage(float _distance)
    {
        this.Log($"SetRiftPercentage called (_distance: '{_distance}')");
        if (linkedRiftController.collapseBehavior == Item_Utility_Geogun.CollapseBehavior.Default)
        {
            if (_distance <= 0)
            {
                stateHandler.SetState<RiftState_Closed>();
                // Disable null-space objects
            }
            if (currentRiftPercent == 0 && _distance > 0)
            {
                stateHandler.SetState<RiftState_Expanding>();
                // Enable null-space objects
            }
        }

        if (!geometryHandler.visualPlaneB || !spaceController.spaceContainerNull.activeInHierarchy) return;

        // TODO Create function parallels for commented sections
        currentRiftPercent = _distance;
        currentRiftWidth = riftStartingWidth * currentRiftPercent;
        //MoveActorsWithRift (_distance);
        //MoveGeometryWithRift();
    }

    /// <summary>
    /// Assign a controller, like the Geogun, to control the rift manager
    /// </summary>
    public void RegisterRiftController(RiftController _linkedRiftController)
    {
        this.Log($"RegisterRiftController called (_linkedRiftController: '{_linkedRiftController}')");
        linkedRiftController = _linkedRiftController;
        //linkedRiftController.isLinkedToManager = true; 
        linkedRiftController.OnCollapseHeld += () => collapseHeld = true;
        linkedRiftController.OnCollapseReleased += () => collapseHeld = false;
        linkedRiftController.OnExpandHeld += () => expandHeld = true;
        linkedRiftController.OnExpandReleased += () => expandHeld = false;
    }

    
    
    
    
    
    // TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP
    
    private void MoveGeometryWithRift()
    {
        if (!spaceContainerNull) return;

        // If the rift collapsed, ignore minimum size rule so that we don't have a gap.
        if (linkedGeogun.collapseBehavior == Item_Utility_Geogun.CollapseBehavior.Default && currentRiftPercent == 0)
        {
            spaceContainerB.transform.position = riftNullSpacePosition;
            cutPlaneB.transform.position = spaceContainerB.transform.position;
            return;
        }

        //  We use minAbsoluteRiftWidth to prevent the rift scale from getting too close to zero
        //  because collision mesh generation will bug out if the mesh is too skinny.

        float moddedRiftPercent = currentRiftPercent;

        if (currentRiftPercent < 0)
        {
            //Special case for negative rift scaling, where the rift can be mirrored.

            if (currentRiftWidth > -minAbsoluteRiftWidth)
            {
                moddedRiftPercent = 1 / riftStartingWidth * -minAbsoluteRiftWidth;
                currentRiftWidth = -minAbsoluteRiftWidth;
            }
            spaceContainerNull.transform.localScale = new Vector3 (1, 1, moddedRiftPercent);
            spaceContainerNull.transform.position = riftNullSpacePosition + spaceContainerNull.transform.forward * -currentRiftWidth;
            spaceContainerB.transform.position = spaceContainerNull.transform.position;
        }
        if (currentRiftPercent >= 0)
        {
            if (currentRiftWidth < minAbsoluteRiftWidth)
            {
                moddedRiftPercent = 1 / riftStartingWidth * minAbsoluteRiftWidth;
                currentRiftWidth = minAbsoluteRiftWidth;
            }
            spaceContainerNull.transform.localScale = new Vector3 (1, 1, moddedRiftPercent);
            spaceContainerB.transform.position = spaceContainerNull.transform.position + spaceContainerNull.transform.forward * currentRiftWidth;
            spaceContainerNull.transform.position = riftNullSpacePosition;
        }
        cutPlaneB.transform.position = spaceContainerB.transform.position;
    }
    
    
    
    
    
    #endregion

}
