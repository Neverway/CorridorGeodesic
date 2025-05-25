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

/// <summary>
/// Handles the travel, placement, and shattering of CorGeo markers
/// </summary>
public class Projectile_Marker : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public float speed = 3;
    public LayerMask layerMask;
    public List<Material> validPlacementMaterials;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private bool pinned;
    private RaycastHit hit;

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/


    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void Awake()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, 255, layerMask))
        {
            if (GetIsValidTarget())
            {
                transform.position = hit.point;
                transform.rotation = Quaternion.LookRotation(-hit.normal);
            }
        }
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Returns true if the gun is pointed at a target it's allowed to shoot
    /// This is used by the hud's crosshair
    /// </summary>
    /// <returns></returns>
    public bool GetIsValidTarget()
    {
        // Gun is pointed at a bulb snapping point (That is valid!)
        // TODO - BulbCollisionBehaviour has not been ported!
        if (hit.collider.gameObject.TryGetComponent<MarkerCollisionBehaviour>(out _)) return true;

        // Gun is pointed at a sliceable object
        if (hit.collider.gameObject.TryGetComponent<CorGeo_MeshSlicable>(out _) is false) return false;
        // Non-mesh colliders don't support getting the polygon information, so we exit if it's not a mesh collider
        if (hit.collider is not MeshCollider mCollider) return false;
        // Get if the raycast hit a polygon with a valid material to place markers on
        if (hit.collider.gameObject.TryGetComponent(out Renderer rend) is false) return false;

        // Gather information about the mesh
        Mesh colMesh = mCollider.sharedMesh;
        int triIndex = hit.triangleIndex;
        int subMeshIndex = GetSubMeshIndex(colMesh, triIndex);

        return subMeshIndex == -1 || validPlacementMaterials.Contains(rend.sharedMaterials[subMeshIndex]);
    }

    int GetSubMeshIndex(Mesh mesh, int triIndex)
    {
        int triangleCounter = 0;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            triangleCounter += mesh.GetSubMesh(i).indexCount / 3;
            if (triIndex < triangleCounter)
            {
                return i;
            }
        }
        return -1;
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
