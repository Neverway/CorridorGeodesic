//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Utility_Geogun : Item
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("GeoGun Upgrades")]
    [Tooltip("Allows rifts to be placed on walls")]
    public bool allowNonLinearSlicing = true;
    [Tooltip("Allows rifts to expand past the start position")]
    public bool allowExpandingRift;
    [Tooltip("Allows the player to slam rifts closed, creating a vacuum that flings things out of rifts")]
    public bool allowSlammingRift;
    [Tooltip("Debug parameter to... well, you get it")]
    public bool allowMarkerPlacementAnywhere;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private int maxProjectiles = 2;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private List<GameObject> spawnedProjectiles = new List<GameObject>();
    [Tooltip("This is the object to spawn when firing the gun")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("This is where the raycast for firing the gun starts from")]
    [SerializeField] private Transform gunBarrel;
    [Tooltip("This is what collision layers the raycast will collide with")] 
    [SerializeField] private LayerMask projectileLayerMask;

    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void FireMarker()
    {
        if (spawnedProjectiles.Count >= maxProjectiles) return;
        var projectile = Instantiate(projectilePrefab, gunBarrel.position, new Quaternion(), null);
        spawnedProjectiles.Add(projectile);
    }

    private void DestroyMarkers()
    {
        foreach (var _projectile in spawnedProjectiles)
        {
            Destroy(_projectile);
        }
        spawnedProjectiles.Clear();
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public override void UsePrimary(string _mode = "press")
    {
        switch (_mode)
        {
            case "press":
                FireMarker();
                break;
            case "release":
                break;
        }
    }
    
    public override void UseTertiary(string _mode = "press")
    {
        switch (_mode)
        {
            case "press":
                DestroyMarkers();
                break;
            case "release":
                break;
        }
    }

    #endregion
}
