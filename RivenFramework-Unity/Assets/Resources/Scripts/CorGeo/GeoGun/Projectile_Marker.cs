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

    
    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public bool pinned;
    
    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/

    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_RiftManager riftManager;
    private RaycastHit hit;
    [Tooltip("This is the mesh on the object that is shown through walls, the reference is needed so we can hide the highlight if the projectile is not pinned")]
    [SerializeField] private GameObject outlineFX;
    [Tooltip("Reference to the gun so the projectile can check for valid placement (this is set by the gun when it spawns the projectile)")]
    [HideInInspector] public Item_Utility_Geogun geogun;

    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void Start()
    {
        riftManager = FindObjectOfType<GI_RiftManager>();
    }

    private void OnDestroy()
    {
        MarkerBreak();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/

    
    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    private void MarkerPin()
    {
        MarkerPinAt(hit.point, -hit.normal);
    }
    
    /// <summary>
    /// Handles the placement logic of the marker projectile and sets its position and rotation
    /// </summary>
    /// <param name="_position">The position to place the marker</param>
    /// <param name="_direction">The direction to point the marker</param>
    private void MarkerPinAt(Vector3 _position, Vector3 _direction)
    {
        pinned = true;
        transform.position = _position;
        transform.rotation = Quaternion.LookRotation(_direction);

        outlineFX.SetActive(true);
        
        // Add itself to the rift manager if possible
        if (riftManager.markerA == null) riftManager.markerA = this;
        else if (riftManager.markerB == null) riftManager.markerB = this;
    }
    
    /// <summary>
    /// Handles all the logic for cleanly destroying the marker projectile
    /// </summary>
    private void MarkerBreak()
    {
        // Remove itself from the rift manager if present
        if (pinned)
        {
            if (riftManager.markerA == this) riftManager.markerA = null;
            else if (riftManager.markerB == this) riftManager.markerB = null;
        }
        Destroy(gameObject, 0.25f);
    }


    #endregion
}
