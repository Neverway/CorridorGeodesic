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
using System.Linq;
using RivenFramework;
using UnityEngine;

[Serializable]
public class RiftManager_GeometryHandler
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("Link to the space controller so this class has access to space containers")]
    private RiftManager_SpaceController spaceController;
    [Tooltip("Coroutine used to keep multiple cut operations from being called at the same time")]
    private Coroutine cutRoutine;
    
    [Tooltip("The visuals that represent the rift cut planes")]
    [HideInInspector] public GameObject visualPlaneA, visualPlaneB;
    [Tooltip("The mathematical plane where the rift is cut")]
    [HideInInspector] public Plane cutPlaneA, cutPlaneB;

    [Tooltip("The geometry that has been cut, but is stored inactive until all cuts are done")]
    public List<GameObject> cutMeshesToActivate;
    [Tooltip("The original uncut level geometry that has been set inactive while the rift is active")]
    public List<GameObject> originalMeshesToHide;


    #endregion

    public RiftManager_GeometryHandler(RiftManager_SpaceController spaceController)
    {
        this.spaceController = spaceController;
    }

    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private IEnumerator SliceCutPlanes()
    {
        cutMeshesToActivate = new List<GameObject>();
        
        // Separate and slice intersected meshes
        var intersectedMeshes = new HashSet<CorGeo_SliceableMesh>(); // This array is a HashSet to avoid collecting duplicates
        intersectedMeshes.UnionWith(CorGeo_PlaneIntersectionUtil.GetIntersectingMeshes(cutPlaneA));
        intersectedMeshes.UnionWith(CorGeo_PlaneIntersectionUtil.GetIntersectingMeshes(cutPlaneB));
        foreach (var intersectedMesh in intersectedMeshes)
        {
            intersectedMesh.ApplyCuts();
        }
        
        
        while (intersectedMeshes.Any((intersectedMesh) => intersectedMesh.isSliceInProgress))
        {
            yield return null;
        }
    }

    private IEnumerator SwitchToCutGeometry()
    {
        yield return new WaitForEndOfFrame();
        foreach (var hiddenOriginalMesh in originalMeshesToHide)
        {
            hiddenOriginalMesh.SetActive (false);
        }
        foreach (var mesh in cutMeshesToActivate) 
        {
            if (mesh == null)
            {
                Debug.LogError ("null mesh was left in the list??");
                continue;
            }
            mesh.SetActive (true);
        }
    }

    private void CleanupExtraMeshColliders()
    {
        foreach (var newMesh in spaceController.spaceMeshesB)
        {
            var meshColliders = newMesh.GetComponents<MeshCollider>();
            if (meshColliders.Length > 1)
            {
                Component.Destroy(meshColliders[0]);
            }
        }

        foreach (var newMesh in spaceController.spaceMeshesNull)
        {
            var meshColliders = newMesh.GetComponents<MeshCollider>();
            if (meshColliders.Length > 1)
            {
                Component.Destroy(meshColliders[0]);
            }
        }
    }

    private void CleanupCollisionConvexStates()
    {
    }
    
    private IEnumerator CutProcedure()
    {
        yield return SliceCutPlanes();
        yield return SwitchToCutGeometry();
        CleanupExtraMeshColliders();
        CleanupCollisionConvexStates();
        cutRoutine = null;
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Show or hide the visual effects planes that represent the rift
    /// </summary>
    public void SetRiftPlanesVisible(bool _isVisible)
    {
        visualPlaneA.SetActive(_isVisible);
        visualPlaneB.SetActive(_isVisible);
    }

    /// <summary>
    /// Specify the points in 3d space in which the mathematical cut planes will be created
    /// </summary>
    public void PositionCutPlanes(Transform _markerA, Transform _markerB)
    {
        // Set the positions and rotations of the cut plane objects
        visualPlaneA.transform.position = _markerA.transform.position;
        visualPlaneB.transform.position = _markerB.transform.position;
        
        visualPlaneA.transform.LookAt(_markerB.transform);
        visualPlaneB.transform.LookAt(_markerA.transform);
        
        // Assign the mathematical plane values
        cutPlaneA = new Plane(visualPlaneA.transform.forward, visualPlaneA.transform.position);
        cutPlaneA = new Plane(visualPlaneB.transform.forward, visualPlaneB.transform.position);

        //Place the Space Containers at the edges of the rift.
        spaceController.spaceContainerNull.transform.position = visualPlaneA.transform.position;
        spaceController.spaceContainerB.transform.position = visualPlaneB.transform.position;
        //Aim spaceContainerNull so that when we scale it, it will squish parallel to the rift planes.
        spaceController.spaceContainerNull.transform.LookAt (visualPlaneB.transform.position);
        //Initialize the rift measurements
        RiftManager.riftStartingWidth = Vector3.Distance(visualPlaneA.transform.position, visualPlaneB.transform.position);
        RiftManager.currentRiftPercent = 1;
        RiftManager.currentRiftWidth =  RiftManager.riftStartingWidth;

        // I'm preserving this position because negative scaling moves the object. ~Connorses
        RiftManager.riftNullSpaceStartingPosition = spaceController.spaceContainerNull.transform.position;

        // Saves the direction the rift is facing so we can easily reference it.
        RiftManager.riftNormal = spaceController.spaceContainerNull.transform.forward;
    }

    public void PerformCutProcedure()
    {
        if (cutRoutine != null)
        {
            Debug.LogError("Attempted to perform cut while one is already running! This is bad!?");
            return;
        }
        
        cutRoutine = GameInstance.SendCoroutine(CutProcedure());
    }

    public void RestoreCutGeometry()
    {
        throw new NotImplementedException();
    }


    #endregion

}
