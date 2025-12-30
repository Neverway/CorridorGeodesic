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
public class RiftManager_SpaceController
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public GameObject spaceContainerA, spaceContainerB, spaceContainerNull;
    public List<GameObject> spaceMeshesA, spaceMeshesB, spaceMeshesNull;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Take the meshes in the space mesh lists and reparent them to their corresponding space containers
    /// </summary>
    public void ReparentGeometryToSpaceContainers()
    {
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
        throw new NotImplementedException();
    }


    #endregion
}
