//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Finds pinned markers, does rift stuff, referenced by gun script to control rift movements
/// </summary>
public class GI_RiftManager : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public bool riftActive;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private float maxRiftWidth = 30;
    private float minRiftWidth = -30;
    private float minAbsoluteRiftWidth = 0.15f; // This is to prevent physics bugs if nullspace scales too close to 0 without being 0.
    private float currentRiftPercent; //current percent scaling of the rift
    private float currentRiftWidth; //current width after applying percent scale
    private float riftStartingWidth; //width of the rift when it was first placed
    private bool collapseHeld = false;
    private bool expandHeld = false;
    private bool waitForCollapseReleased = false; //Waits for you to release collapse so that the player has to press it again to collapse rift.
    private Vector3 riftNullSpacePosition; //The starting position of the null space container.


    //  rift movement speed stuff:  //
    private float minRiftSpeed = 0.5f;
    private float maxRiftSpeed = 6f;
    private float riftAcceleration = 2f;
    private float currentRiftMoveSpeed;

    // state stuff //
    private bool riftIsMoving;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private Item_Utility_Geogun linkedGeogun;
    [SerializeField] private GameObject cutPlanePrefab, spaceContainerA, spaceContainerB, spaceContainerNull;
    [HideInInspector] public GameObject cutPlaneA, cutPlaneB;
    [HideInInspector] public Plane planeA, planeB;
    [HideInInspector] public Projectile_Marker markerA, markerB;
    [HideInInspector] public List<GameObject> spaceAMeshes, spaceBMeshes, spaceNullMeshes, hiddenOriginalMeshes;
    public Graphics_RiftPreviewEffects riftPreviewEffects;
    public Material nullSpaceMaterial;
    

    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Update()
    {
        // Link the manager to a geogun if it's not yet
        if (!linkedGeogun)
        {
            LinkToGeogun();
        }
        
        // Initialize rift objects if they are missing
        if (IsRiftInitialized() is false) InitializeRiftObjects();
        
        // Markers found, enable rift
        if (GetPinnedMarkers() && riftActive is false)
        {
            SetRiftHidden(false);
            StartCoroutine(riftPreviewEffects.OnRiftCreated(this));
            PositionCutPlanes();
        }
        // Markers lost, disable rift
        else if (GetPinnedMarkers() is false && riftActive)
        {
            SetRiftHidden(true);
            RestoreRift();
        }

        if (waitForCollapseReleased && collapseHeld == false)
        {
            waitForCollapseReleased = false;
        }

        riftIsMoving = false;
        if (riftActive)
        {
            if (collapseHeld && waitForCollapseReleased == false)
            {
                MoveRiftByDistance (-currentRiftMoveSpeed * Time.deltaTime);
                AccelerateRift ();
            }
            else if (expandHeld)
            {
                MoveRiftByDistance (currentRiftMoveSpeed * Time.deltaTime);
                AccelerateRift ();
            }
            else
            {
                currentRiftMoveSpeed = 0;
                riftIsMoving = false;
            }
        }
    }

    private void AccelerateRift ()
    {
        currentRiftMoveSpeed += riftAcceleration * Time.deltaTime;
        if (currentRiftMoveSpeed > maxRiftSpeed)
        {
            currentRiftMoveSpeed = maxRiftSpeed;
        }
    }

    private void OnDestroy()
    {
        RestoreRift();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Gets a reference to an unlinked Geogun in the scene so the rift manager can subscribe to the guns action events
    /// (Like clearing, collapsing, or expanding the rift)
    /// </summary>
    private void LinkToGeogun()
    {
        if (linkedGeogun) return; // Sanity check to avoid multiple function calls
        foreach (var geogun in FindObjectsOfType<Item_Utility_Geogun>())
        {
            if (geogun.isLinkedToManager is false)
            {
                linkedGeogun = geogun;
                linkedGeogun.isLinkedToManager = true;
                //linkedGeogun.OnGunDestroyMarkers += () => RestoreRift();
                linkedGeogun.OnCollapseHeld += () => collapseHeld = true;
                linkedGeogun.OnCollapseReleased += () => collapseHeld = false;
                linkedGeogun.OnExpandHeld += () => expandHeld = true;
                linkedGeogun.OnExpandReleased += () => expandHeld = false;
                return;
            }
        }
    }
    
    /// <summary>
    /// Detects if any of the rift objects are missing
    /// </summary>
    /// <returns>Returns false if any of the rift object references are null</returns>
    private bool IsRiftInitialized()
    {
        return cutPlaneA && cutPlaneB && spaceContainerA && spaceContainerB && spaceContainerNull;
    }
    
    /// <summary>
    /// Creates the cut planes and containers, then hides them until they are ready for use
    /// </summary>
    private void InitializeRiftObjects()
    {
        // The objects used for the rift are created when the level loads
        // They are never destroyed, only hidden or unhidden
        // This is done this way to avoid lag spikes caused by having to constantly spawn and despawn the rift objects
        CreateCutPlanes();
        CreateSpaceContainers();
        SetRiftHidden(true);
    }
    
    /// <summary>
    /// Called in Start, Spawn the planes used for the cutting of the world as well as the visuals for previewing the cut
    /// </summary>
    private void CreateCutPlanes()
    {
        cutPlaneA = Instantiate(cutPlanePrefab, null);
        cutPlaneB = Instantiate(cutPlanePrefab, null);
        cutPlaneA.name = "CutPlaneA";
        cutPlaneB.name = "CutPlaneB";
    }    
    
    /// <summary>
    /// Called in Start, Spawn the empty game objects that represent the space that matter exists in while a rift is active
    /// These objects are used to scale and reposition all objects at once, according to the space they occupy
    /// </summary>
    private void CreateSpaceContainers()
    {
        var spaceContainer = new GameObject();
        spaceContainer.name = "ASpace";
        spaceContainerA = spaceContainer;
        spaceContainer = new GameObject();
        spaceContainer.name = "BSpace";
        spaceContainerB = spaceContainer;
        spaceContainer = new GameObject();
        spaceContainer.name = "NullSpace";
        spaceContainerNull = spaceContainer;
    }
    
    /// <summary>
    /// Toggles the visibility of the cut planes, used for hiding/showing the rift objects when the rift is deactivated/activated
    /// </summary>
    private void SetRiftHidden(bool _hidden)
    {
        riftActive = !_hidden;
        cutPlaneA.SetActive(!_hidden);
        cutPlaneB.SetActive(!_hidden);
    }
    
    /// <summary>
    /// Assigns the 'markerA/B' references to the first pinned marks found
    /// </summary>
    /// <returns>Returns true if two pinned markers are found and assigned</returns>
    private bool GetPinnedMarkers()
    {
        if (markerA && markerB) return true;
        return false;
    }

    /// <summary>
    /// Moves the cut planes to the position of the markers
    /// </summary>
    private void PositionCutPlanes()
    {
        // Set the positions and rotations of the cut plane objects
        cutPlaneA.transform.position = markerA.transform.position;
        cutPlaneB.transform.position = markerB.transform.position;
        
        cutPlaneA.transform.LookAt(markerB.transform);
        cutPlaneB.transform.LookAt(markerA.transform);
        
        // Assign the mathematical plane values
        planeA = new Plane(cutPlaneA.transform.forward, cutPlaneA.transform.position);
        planeB = new Plane(cutPlaneB.transform.forward, cutPlaneB.transform.position);

        //Place the Space Containers at the edges of the rift.
        spaceContainerNull.transform.position = cutPlaneA.transform.position;
        spaceContainerB.transform.position = cutPlaneB.transform.position;
        //Aim spaceContainerNull so that when we scale it, it will squish parallel to the rift planes.
        spaceContainerNull.transform.LookAt (cutPlaneB.transform.position);
        //Initialize the rift measurements
        riftStartingWidth = Vector3.Distance(cutPlaneA.transform.position, cutPlaneB.transform.position);
        currentRiftPercent = 1;
        currentRiftWidth = riftStartingWidth;

        riftNullSpacePosition = spaceContainerNull.transform.position; //I'm preserving this position because negative scaling moves the object.
        
        // Slice the cut planes (This is for debugging right now)
        SliceCutPlanes();
    }

    /// <summary>
    /// Makes the initial cuts at the positions of the two cut planes
    /// </summary>
    private void SliceCutPlanes()
    {
        var sliceableMeshes = FindObjectsOfType<Mesh_Sliceable> ();
        foreach (var sliceableMesh in sliceableMeshes)
        {
            sliceableMesh.ApplyCuts();
        }

        // Clean up glitched duplicate mesh colliders that sometimes appear on sub-cuts
        StartCoroutine(CleanupExtraMeshColliders());

        StartCoroutine(AssignSpaceContainerForMeshes());

        waitForCollapseReleased = true;
    }
    
    /// <summary>
    /// Sometimes multi-cut meshes have an extra, broken, mesh collider as the first one in the index, this fixes those
    /// </summary>
    private IEnumerator CleanupExtraMeshColliders()
    {
        // Wait for a bit so the async await operations have time to finish creating their new meshes
        // is 0.25 seconds enough? ~Liz
        // It was not, I have changed it to wait for the end of the frame and that seems to have done the trick! ~Liz
        yield return new WaitForEndOfFrame();
        foreach (var newMesh in spaceBMeshes)
        {
            var meshColliders = newMesh.GetComponents<MeshCollider>();
            if (meshColliders.Length > 1)
            {
                Destroy (meshColliders[0]);
            }
        }

        foreach (var newMesh in spaceNullMeshes)
        {
            var meshColliders = newMesh.GetComponents<MeshCollider>();
            if (meshColliders.Length > 1)
            {
                Destroy (meshColliders[0]);
            }
        }
    }

    /// <summary>
    /// Sets the parent for all the meshes in the lists to the correct space container
    /// </summary>
    private IEnumerator AssignSpaceContainerForMeshes()
    {
        // Wait for a bit so the async await operations have time to finish creating their new meshes
        yield return new WaitForEndOfFrame();
        
        foreach (var mesh in spaceAMeshes)
        {
            mesh.transform.parent = spaceContainerA.transform;
        }
        foreach (var mesh in spaceBMeshes)
        {
            mesh.transform.parent = spaceContainerB.transform;
        }
        foreach (var mesh in spaceNullMeshes)
        {
            mesh.transform.parent = spaceContainerNull.transform;
        }
    }

    /// <summary>
    /// Sorts all dynamic (moving/movable) actors into 'A', 'B', and 'Null' spaces
    /// </summary>
    private void UpdateActorSpaces()
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    private void EmptyMatterInSpaceContainers()
    {
        // Clear lists
        spaceAMeshes.Clear();
        spaceBMeshes.Clear();
        spaceNullMeshes.Clear();
     
        // There is a possibility that when calling this from OnDestroy, the spaceContainers don't exist, which is fine
        // This just exits if that's the case, so it doesn't throw a null error
        if (!spaceContainerA || !spaceContainerB || !spaceContainerNull) return;
        
        // Un-parent matter
        for (int i = 0; i < spaceContainerA.transform.childCount; i++)
        {
            spaceContainerA.transform.GetChild(i).parent = null;
        }
        for (int i = 0; i < spaceContainerB.transform.childCount; i++)
        {
            spaceContainerB.transform.GetChild(i).parent = null;
        }
        for (int i = 0; i < spaceContainerNull.transform.childCount; i++)
        {
            spaceContainerNull.transform.GetChild(i).parent = null;
        }
    }

    /// <summary>
    /// Cleans up cloned cut meshes and restores original meshes
    /// </summary>
    private void RestoreCutGeometry()
    {
        // Destroy cloned cut geometry
        var sliceableMeshes = FindObjectsOfType<Mesh_Sliceable>();
        foreach (var sliceableMesh in sliceableMeshes)
        {
            if (sliceableMesh.isSlicedByPlane && !hiddenOriginalMeshes.Contains(sliceableMesh.gameObject))
            {
                Destroy(sliceableMesh.gameObject);
            }
        }

        // Un-hide the original meshes
        for (int i = 0; i < hiddenOriginalMeshes.Count; i++)
        {
            hiddenOriginalMeshes[i].SetActive(true);
        }
        
        hiddenOriginalMeshes.Clear();
    }

    /// <summary>
    /// Sets the rift back to it's zero point and restores cut geometry
    /// </summary>
    private void RestoreRift()
    {
        SetRiftPosition(1);
        RestoreCutGeometry();
        EmptyMatterInSpaceContainers();
        currentRiftMoveSpeed = 0;
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Controls the collapsing and expanding of a deployed rift
    /// 1 is the start position (no compression/expansion), 0 is fully collapsed, and 2 is expanded to twice the distance of the rift planes
    /// </summary>
    /// <param name="_percent">The size of the rift relative to it's starting size.</param>
    public void SetRiftPosition(float _percent)
    {
        currentRiftPercent = _percent;
        currentRiftWidth = riftStartingWidth * currentRiftPercent;
        MoveGeometryWithRift ();
    }

    /// <summary>
    /// Changes the size of the rift by the specified number of units.
    /// </summary>
    /// <param name="distance"></param>
    public void MoveRiftByDistance(float distance)
    {
        if (currentRiftWidth + distance > maxRiftWidth)
        {
            distance = maxRiftWidth - currentRiftWidth;
        }
        if (currentRiftWidth + distance < minRiftWidth)
        {
            currentRiftWidth = minRiftWidth;
        }
        float percentChange = 1 / riftStartingWidth * distance;

        SetRiftPosition (currentRiftPercent + percentChange);
        riftIsMoving = true;
    }

    private void MoveGeometryWithRift ()
    {
        if (!spaceContainerNull) return;
        //  We use minAbsoluteRiftWidth to prevent the rift scale from getting too close to zero
        //  because collision mesh generation will bug out if the mesh is too skinny.

        float moddedRiftPercent = currentRiftPercent;

        if (currentRiftPercent < 0)
        {
            //Special case for negative rift scaling.
            //Don't know if we will use this mechanic or not, but you can flip the rift space by collapsing past zero percent.

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