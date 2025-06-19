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


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    public bool riftActive;
    

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private Item_Utility_Geogun linkedGeogun;
    [SerializeField] private GameObject cutPlanePrefab, spaceContainerA, spaceContainerB, spaceContainerNull;
    [HideInInspector] public GameObject cutPlaneA, cutPlaneB;
    [HideInInspector] public Plane planeA, planeB;
    [HideInInspector] public Projectile_Marker markerA, markerB;
    public List<GameObject> spaceAMeshes, spaceBMeshes, spaceNullMeshes, hiddenOriginalMeshes;
    public Graphics_RiftPreviewEffects riftPreviewEffects;
    

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
    }

    private void OnDestroy()
    {
        RestoreRift();
        //if (linkedGeogun) linkedGeogun.OnGunDestroyMarkers -= RestoreRift;
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
        
        // Clear lists
        spaceAMeshes.Clear();
        spaceBMeshes.Clear();
        spaceNullMeshes.Clear();
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
            if (sliceableMesh.isSlicedByPlane)
            {
                Destroy(sliceableMesh.gameObject);
            }
        }

        // Un-hide the original meshes
        foreach (var hiddenMesh in hiddenOriginalMeshes)
        {
            hiddenMesh.SetActive(true);
        }
    }

    /// <summary>
    /// Sets the rift back to it's zero point and restores cut geometry
    /// </summary>
    private void RestoreRift()
    {
        AdjustRiftPosition(0);
        RestoreCutGeometry();
        EmptyMatterInSpaceContainers();
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Controls the collapsing and expanding of a deployed rift
    /// 0 is the start position (no compression/expansion), -1 is fully collapsed, and 1 is expanded to twice the distance of the rift planes
    /// (I'm not exactly sure how the upper limit of expanding would work ~Liz)
    /// </summary>
    /// <param name="_amount">The amount that the rift is expanded or collapsed</param>
    public void AdjustRiftPosition(float _amount)
    {
        
    }


    #endregion
}