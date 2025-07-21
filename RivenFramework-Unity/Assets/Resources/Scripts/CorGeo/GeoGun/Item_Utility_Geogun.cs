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
    [Tooltip("The materials that markers can be placed on")]
    public List<Material> validPlacementMaterials;
    [Tooltip("The layermask for firing projectiles")]
    public LayerMask layerMask;

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
    public RaycastHit hit;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public List<GameObject> spawnedProjectiles = new List<GameObject>();
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
        // Exit if the gun has already shot both markers
        if (spawnedProjectiles.Count >= maxProjectiles) return false;
        
        // Play the shoot anim for the gun and it's outline
        animator1.SetTrigger("Shoot");
        animator2.SetTrigger("Shoot");
        
        // Spawn the projectile
        var projectile = Instantiate(projectilePrefab, gunBarrel.position, gunBarrel.rotation, null).GetComponent<Projectile_Marker>();
        
        projectile.geogun = this;
        
        // Keep track of fired markers
        spawnedProjectiles.Add(projectile.gameObject);
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
                print("UseTertiary");
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
    
    public string GetValidPlacement()
    {
        if (Physics.Raycast(GetComponentInParent<Pawn>().viewPoint.position, GetComponentInParent<Pawn>().viewPoint.forward, out hit, 255, layerMask))
        {
            return GetIsValidTarget() ? "good" : "bad";
        }

        return "null";
    }
    
    /// <summary>
    /// Returns true if the gun is pointed at a target it's allowed to shoot
    /// This is used by the hud's crosshair & the projectile markers
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

    #endregion
}
