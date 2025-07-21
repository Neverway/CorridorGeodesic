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

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public bool pinned;

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private Vector3 startPos, endPos;
    private float travelDistance;
    

    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_RiftManager riftManager;
    [SerializeField] private GameObject outlineFX;
    private RaycastHit hit;
    [Tooltip("Reference to the gun so the projectile can check for valid placement")]
    public Item_Utility_Geogun geogun;


    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void Start()
    {
        riftManager = FindObjectOfType<GI_RiftManager>();
        
        // Good placement
            // Travel distance
                // Pin
        // Bad placement
            // Travel distance
                // Shatter
        // Null placement
            // Travel for N seconds
                // Shatter

        var placement = geogun.GetValidPlacement();
        hit = geogun.hit;
        
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

    private void MarkerPin()
    {
        pinned = true;
        transform.position = hit.point;
        transform.rotation = Quaternion.LookRotation(-hit.normal);

        outlineFX.SetActive(true);
        
        // Add itself from the rift manager if possible
        if (riftManager.markerA == null) riftManager.markerA = this;
        else if (riftManager.markerB == null) riftManager.markerB = this;
    }
    
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

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
