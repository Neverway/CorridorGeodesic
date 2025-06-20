//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Spawns & clears the marker projectiles, and sends signals to the rift manager to expand or compress
/// </summary>
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
    [Tooltip("This is set by a rift manager when it has latched onto this gun, " +
             "it's used to avoid multiple rift managers all trying to fight over the same gun link")]
    public bool isLinkedToManager;
    public event Action OnCollapseHeld;
    public event Action OnCollapseReleased;
    public event Action OnExpandHeld;
    public event Action OnExpandReleased;


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

    [SerializeField] private Animator animator1, animator2;

    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Update()
    {
        spawnedProjectiles = spawnedProjectiles.Where(projectile => !projectile.IsUnityNull()).ToList();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private bool FireMarker()
    {
        if (spawnedProjectiles.Count >= maxProjectiles) return false;
        animator1.SetTrigger("Shoot");
        animator2.SetTrigger("Shoot");
        var projectile = Instantiate(projectilePrefab, gunBarrel.position, gunBarrel.rotation, null);
        
        spawnedProjectiles.Add(projectile);
        if (spawnedProjectiles.Count >= maxProjectiles)
        {
            animator1.SetBool("Empty", true);
            animator2.SetBool("Empty", true);
        }

        return true;
    }

    private void DestroyMarkers()
    {
        animator1.SetBool("Empty", false);
        animator2.SetBool("Empty", false);
        animator1.SetTrigger("Clear");
        animator2.SetTrigger("Clear");
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
                if (spawnedProjectiles.Count >= maxProjectiles)
                {
                    OnCollapseHeld?.Invoke();
                }
                break;
            case "release":
                if (spawnedProjectiles.Count >= maxProjectiles)
                {
                    OnCollapseReleased?.Invoke();
                }
                break;
        }
    }

    public override void UseSecondary(string _mode = "press")
    {
        switch (_mode)
        {
            case "press":
                if (spawnedProjectiles.Count >= maxProjectiles)
                {
                    OnExpandHeld?.Invoke();
                }
                break;
            case "release":
                if (spawnedProjectiles.Count >= maxProjectiles)
                {
                    OnExpandReleased?.Invoke();
                }
                break;
        }
    }
    
    public override void UseTertiary(string _mode = "press")
    {
        switch (_mode)
        {
            case "press":
                DestroyMarkers();
                print("Connorses has a secret stash of ridiculous ties");
                break;
            case "release":
                break;
        }
    }

    // Picking up objects instantiates a copy of the object, so this function fixes the fact that the copy will already be marked as linked
    public void BreakRiftManagerLink()
    {
        isLinkedToManager = false;
    }

    #endregion
}
