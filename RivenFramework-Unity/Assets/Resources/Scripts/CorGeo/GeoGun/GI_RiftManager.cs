//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using System.Collections.Generic;
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
    [SerializeField] private GameObject cutPlanePrefab;
    public GameObject cutPlaneA, cutPlaneB, spaceContainerA, spaceContainerB, spaceContainerNull;
    [HideInInspector] public Plane planeA, planeB;
    [HideInInspector] public Projectile_Marker markerA, markerB;
    

    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Update()
    {
        // Initialize rift objects if they are missing
        if (IsRiftInitialized() is false) InitializeRiftObjects();
        
        // Markers found, enable rift
        if (GetPinnedMarkers() && riftActive is false)
        {
            SetRiftHidden(false);
            PositionCutPlanes();
        }
        // Markers lost, disable rift
        else if (GetPinnedMarkers() is false && riftActive)
        {
            SetRiftHidden(true);
        }
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
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
    }

    /// <summary>
    /// Sorts all objects into 'A', 'B', and 'Null' spaces
    /// </summary>
    private void UpdateMatterInSpaces()
    {
        
    }

    /// <summary>
    /// Sets the rift back to it's zero point and restores cut geometry
    /// </summary>
    private void RestoreRift()
    {
        
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_amount"></param>
    public void AdjustRiftPosition(float _amount)
    {
        
    }


    #endregion
}