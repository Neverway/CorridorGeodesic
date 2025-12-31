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

[Serializable]
public class RiftManager_SpaceController : ILoggable
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public bool EnableRuntimeLogging { get; set; }


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("Link to parent class for logging")]
    private RiftManager riftManager;
    public GameObject spaceContainerA, spaceContainerB, spaceContainerNull;
    public HashSet<GameObject> spaceMeshesA, spaceMeshesB, spaceMeshesNull;


    #endregion

    // Class constructor
    public RiftManager_SpaceController(RiftManager riftManager)
    {
        this.riftManager = riftManager;
        EnableRuntimeLogging = riftManager.EnableRuntimeLogging;
    }


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void CreateSpaceContainers()
    {
        this.Log("CreateSpaceContainers called");
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


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void PositionSpaceContainers(GameObject visualPlaneA, GameObject visualPlaneB)
    {
        this.Log($"PositionSpaceContainers called (visualPlaneA: '{visualPlaneA}', visualPlaneB:  '{visualPlaneB}')");
        if (!spaceContainerA && !spaceContainerB && !spaceContainerNull) CreateSpaceContainers();
        // Place the Space Containers at the edges of the rift.
        spaceContainerNull.transform.position = visualPlaneA.transform.position;
        spaceContainerB.transform.position = visualPlaneB.transform.position;
        // Aim spaceContainerNull so that when we scale it, it will squish parallel to the rift planes.
        spaceContainerNull.transform.LookAt (visualPlaneB.transform.position);
    }
    
    /// <summary>
    /// Take the meshes in the space mesh lists and reparent them to their corresponding space containers
    /// </summary>
    public void ReparentGeometryToSpaceContainers()
    {
        this.Log("ReparentGeometryToSpaceContainers called");
        // Create teh lists if they don't exist yet
        spaceMeshesA ??= new HashSet<GameObject>();
        spaceMeshesB ??= new HashSet<GameObject>();
        spaceMeshesNull ??= new HashSet<GameObject>();
        
        
        foreach (var mesh in spaceMeshesA)
        {
            mesh.transform.parent = spaceContainerA.transform;
        }
        foreach (var mesh in spaceMeshesB)
        {
            mesh.transform.parent = spaceContainerB.transform;
        }
        foreach (var mesh in spaceMeshesNull)
        {
            mesh.transform.parent = spaceContainerNull.transform;
        }
    }

    public void ReparentActorsToSpaceContainers()
    {
        this.Log("ReparentActorsToSpaceContainers called");
        foreach (CorGeo_Actor actor in RiftManager_ActorHandler.CorGeo_Actors)
        {
            actor.DetermineRiftSpace();
            if (actor.dynamic)
            {
                continue; //don't parent dynamic actors to the space-containers
            }
            if (actor.space == CorGeo_Actor.Space.B)
            {
                actor.transform.SetParent(spaceContainerB.transform);
                continue;
            }
            if (actor.space == CorGeo_Actor.Space.Null)
            {
                actor.transform.SetParent (spaceContainerNull.transform);
            }
        }
    }

    public void RemoveObjectsFromSpaceContainers()
    {
        this.Log("RemoveObjectsFromSpaceContainers called");
        throw new NotImplementedException();
    }


    #endregion
}
