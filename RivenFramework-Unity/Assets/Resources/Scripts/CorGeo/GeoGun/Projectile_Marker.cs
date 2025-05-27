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
    public bool pinned;

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private RaycastHit hit;
    private Vector3 startPos, endPos;
    private float travelDistance;
    

    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/


    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void Awake()
    {
        // Good placement
            // Travel distance
                // Pin
        // Bad placement
            // Travel distance
                // Shatter
        // Null placement
            // Travel for N seconds
                // Shatter

        var placement = GetPlacement();
        
        if (placement is "good")
        {
            MarkerPin();
        }
        else if (placement is "bad")
        {
            MarkerBreak();
        }
        else if (placement is "null")
        {
            MarkerBreak();
        }
                
    }

    private void OnDestroy()
    {
        MarkerBreak();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Returns true if the gun is pointed at a target it's allowed to shoot
    /// This is used by the hud's crosshair
    /// </summary>
    /// <returns></returns>
    private bool GetIsValidTarget()
    {
        // Gun is pointed at a bulb snapping point (That is valid!)
        // TODO - BulbCollisionBehaviour has not been ported!
        if (hit.collider.gameObject.TryGetComponent<MarkerCollisionBehaviour>(out _)) return true;

        // Gun is pointed at a sliceable object
        if (hit.collider.gameObject.TryGetComponent<Mesh_Sliceable>(out _) is false) return false;
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

    private int GetSubMeshIndex(Mesh mesh, int triIndex)
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

    private string GetPlacement()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, 255, layerMask))
        {
            return GetIsValidTarget() ? "good" : "bad";
        }
        return "null";
    }

    private void MarkerPin()
    {
        pinned = true;
        transform.position = hit.point;
        transform.rotation = Quaternion.LookRotation(-hit.normal);
    }
    
    private void MarkerBreak()
    {
        Destroy(gameObject, 0.25f);
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
