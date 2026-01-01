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
using RivenFramework;
using UnityEngine;

[Serializable]
public class RiftManager_SpaceController : ILoggable
{
    /// <summary>
    /// Class constructor
    /// </summary>
    /// <param name="riftManager"></param>
    public RiftManager_SpaceController(RiftManager riftManager)
    {
        this.riftManager = riftManager;
        EnableRuntimeLogging = riftManager.EnableRuntimeLogging;
    }

    
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public bool EnableRuntimeLogging { get; set; }


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("Link to parent class for logging")]
    private RiftManager riftManager;
    public Dictionary<GameObject, RiftSpace> spaceMeshes =  new ();
    public Dictionary<GameObject, RiftSpace> spaceActors =  new ();
    public GameObject spaceContainerA, spaceContainerB, spaceContainerNull;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Create the empty game objects that will be used to sort and reposition objects in different rift-spaces (Referred to as space containers)
    /// </summary>
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
    /// <summary>
    /// Set the position and rotation of the space containers so they'll be ready to be scaled 
    /// </summary>
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

        foreach (var mesh in spaceMeshes)
        {
            switch (mesh.Value)
            {
                case RiftSpace.A:
                    mesh.Key.transform.parent = spaceContainerA.transform;
                    break;
                case RiftSpace.NULLSpace:
                    mesh.Key.transform.parent = spaceContainerNull.transform;
                    break;
                case RiftSpace.B:
                    mesh.Key.transform.parent = spaceContainerB.transform;
                    break;
                case RiftSpace.none:
                    Debug.LogError($"The mesh '{mesh.Key.name}' was not assigned to any space, this is a critical issue!");
                    break;
            }
        }
        
        /*
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
        }*/
    }

        /// <summary>
        /// Take the actors in the CorGeo_actors list and reparent them to their corresponding space containers
        /// </summary>
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
    
    /// <summary>
    /// 
    /// </summary>
    [Todo("Not implemented", severity:TodoSeverity.Critical, Owner = "Liz-RiftManagerRevamp")]
    public void RemoveObjectsFromSpaceContainers()
    {
        this.Log("RemoveObjectsFromSpaceContainers called");
        //throw new NotImplementedException();
    }


    #endregion
}

public enum RiftSpace
{
    none,
    A,
    B,
    NULLSpace
}