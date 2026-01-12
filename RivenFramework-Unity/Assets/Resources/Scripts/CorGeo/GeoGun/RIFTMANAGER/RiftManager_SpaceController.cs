//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M., Connorses, Errynei, Soulex
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

/// <summary>
/// Handles the parenting, unparenting, positioning, and scaling of space containers
/// </summary>
[Serializable]
public class RiftManager_SpaceController : ILoggable
{
    /// <summary>
    /// Class constructor
    /// </summary>
    public RiftManager_SpaceController(RiftManager riftManager, RiftManager_GeometryHandler geometryHandler)
    {
        this.riftManager = riftManager;
        EnableRuntimeLogging = riftManager.EnableRuntimeLogging;
        this.geometryHandler = geometryHandler;
    }

    
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public bool EnableRuntimeLogging { get; set; }


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("Link to parent class for logging")]
    private RiftManager riftManager;
    public RiftManager_GeometryHandler geometryHandler;
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
    /// Unparents all meshes and actors from the space containers, then clears the space lists
    /// </summary>
    public void RemoveObjectsFromSpaceContainers()
    {
        this.Log("RemoveObjectsFromSpaceContainers called");
        foreach (var mesh in spaceMeshes)
        {
            mesh.Key.transform.parent = null;
        }
        foreach (var actor in spaceActors)
        {
            actor.Key.transform.parent = null;
        }
        spaceMeshes.Clear();
        spaceActors.Clear();
    }
    
    /// <summary>
    /// Scales and moves the visual planes and space containers
    /// </summary>
    public void MoveGeometryWithRift()
    {
        if (!spaceContainerNull) return;

        // If the rift collapsed, ignore minimum size rule so that we don't have a gap.
        if (riftManager.linkedRiftController.collapseBehavior == Item_Utility_Geogun.CollapseBehavior.Default && RiftManager.currentRiftPercent == 0)
        {
            spaceContainerB.transform.position = RiftManager.riftNullSpaceStartingPosition;
            geometryHandler.visualPlaneB.transform.position = spaceContainerB.transform.position;
            return;
        }

        //  We use minAbsoluteRiftWidth to prevent the rift scale from getting too close to zero
        //  because collision mesh generation will bug out if the mesh is too skinny.

        float moddedRiftPercent = RiftManager.currentRiftPercent;

        if (RiftManager.currentRiftPercent < 0)
        {
            //Special case for negative rift scaling, where the rift can be mirrored.

            if (RiftManager.currentRiftWidth > -RiftManager.minAbsoluteRiftWidth)
            {
                moddedRiftPercent = 1 / RiftManager.riftStartingWidth * -RiftManager.minAbsoluteRiftWidth;
                RiftManager.currentRiftWidth = -RiftManager.minAbsoluteRiftWidth;
            }
            spaceContainerNull.transform.localScale = new Vector3 (1, 1, moddedRiftPercent);
            spaceContainerNull.transform.position = RiftManager.riftNullSpaceStartingPosition + spaceContainerNull.transform.forward * -RiftManager.currentRiftWidth;
            spaceContainerB.transform.position = spaceContainerNull.transform.position;
        }
        if (RiftManager.currentRiftPercent >= 0)
        {
            if (RiftManager.currentRiftWidth < RiftManager.minAbsoluteRiftWidth)
            {
                moddedRiftPercent = 1 / RiftManager.riftStartingWidth * RiftManager.minAbsoluteRiftWidth;
                RiftManager.currentRiftWidth = RiftManager.minAbsoluteRiftWidth;
            }
            spaceContainerNull.transform.localScale = new Vector3 (1, 1, moddedRiftPercent);
            spaceContainerB.transform.position = spaceContainerNull.transform.position + spaceContainerNull.transform.forward * RiftManager.currentRiftWidth;
            spaceContainerNull.transform.position = RiftManager.riftNullSpaceStartingPosition;
        }
        geometryHandler.visualPlaneB.gameObject.transform.position = spaceContainerB.transform.position;
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