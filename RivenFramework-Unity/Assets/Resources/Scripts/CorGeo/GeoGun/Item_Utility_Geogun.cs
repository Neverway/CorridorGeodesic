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
using RivenFramework;
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
    [Todo("Need to add nonlinear slicing check to rift manager", "Liz")]
    [Tooltip("Allows rifts to be placed on walls")]
    public bool allowNonLinearSlicing = true;
    [Todo("Need to add expanding rift check to rift manager", "Liz")]
    [Tooltip("Allows rifts to expand past the start position")]
    public bool allowExpandingRift;
    [Todo("Need to add inverting rift check to rift manager", "Liz")]
    [Tooltip("Allows rifts collapsing into the negatives, mirroring null space")]
    public bool allowInvertingRift;
    [Todo("Need to add slamming rift check to rift manager", "Liz")]
    [Tooltip("Allows the player to slam rifts closed, creating a vacuum that flings things out of rifts")]
    public bool allowSlammingRift;
    [Tooltip("Debug parameter to... well, you get it (Allows markers to be placed on any material)")]
    public bool allowMarkerPlacementAnywhere;
    [Header("Projectile Checks")]
    [Tooltip("The materials that markers can be placed on")]
    public List<Material> validPlacementMaterials;
    [Tooltip("The layermask for firing projectiles")]
    public LayerMask layerMask;
    [Tooltip("How fast marker projectiles travel when fired")]
    public int projectileMarkerSpeed = 50;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Tooltip("This is set by a rift manager when it has latched onto this gun, " +
             "it's used to avoid multiple rift managers all trying to fight over the same gun link")]
    [HideInInspector] public bool isLinkedToManager;
    [Tooltip("Subscribed to by rift manager to tell when gun wants to collapse")]
    public event Action OnCollapseHeld;
    [Tooltip("Subscribed to by rift manager to tell when gun wants to stop collapsing")]
    public event Action OnCollapseReleased;
    [Tooltip("Subscribed to by rift manager to tell when gun wants to expand")]
    public event Action OnExpandHeld;
    [Tooltip("Subscribed to by rift manager to tell when gun wants to stop expanding")]
    public event Action OnExpandReleased;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private int maxProjectiles = 2;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [HideInInspector] public List<GameObject> spawnedProjectiles = new List<GameObject>();
    private Transform playerViewPoint;
    [Header("References")]
    [Tooltip("This is the object to spawn when firing the gun")]
    [SerializeField] private GameObject projectilePrefab;
    [Tooltip("This is where the raycast for firing the gun starts from")] // <== This is a lie >: ~Liz
    [SerializeField] private Transform gunBarrel;
    [Tooltip("A reference to the gun's, and it's outline's, animator")]
    [SerializeField] private Animator animator1, animator2;
    //[Tooltip("This is what collision layers the raycast will collide with")] 
    //[SerializeField] private LayerMask projectileLayerMask;

    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Update()
    {
        // Get a reference to the player view point
        if (!playerViewPoint)
        {
            var targetPawn = GetComponentInParent<Pawn>();
            if (targetPawn) playerViewPoint = targetPawn.viewPoint;
            return;
        }
        
        
        AimBarrelTowardsCenterOfView();
        
        // Auto-removes null projectiles from the spawnedProjectiles list
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
        var projectile = Instantiate(projectilePrefab, playerViewPoint.position, playerViewPoint.rotation, null).GetComponent<Projectile_Marker>();
        
        projectile.geogun = this;
        Physics.Raycast(playerViewPoint.position, playerViewPoint.forward, out RaycastHit hit2, 255, layerMask);
        projectile.InitializeProjectile(projectileMarkerSpeed, gunBarrel.position, Vector3.Distance(gunBarrel.position, hit2.point));
        projectile.allowMarkerPlacementAnywhere = allowMarkerPlacementAnywhere;
        
        // Keep track of fired markers
        spawnedProjectiles.Add(projectile.gameObject);
        if (spawnedProjectiles.Count >= maxProjectiles)
        {
            animator1.SetBool("Empty", true);
            animator2.SetBool("Empty", true);
        }

        return true;
    }
    
    public void DestroyMarkers()
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
    
    /// <summary>
    /// Ensures that the actual point in which projectiles are fired from is facing where the player's crosshair is aimed
    /// </summary>
    private void AimBarrelTowardsCenterOfView ()
    {
        // Perform the raycast, ignoring the trigger layer
        if (Physics.Raycast (playerViewPoint.position, playerViewPoint.forward, out RaycastHit viewPoint, Mathf.Infinity, layerMask))
        {
            // If the raycast hits something, aim the barrel towards the hit point
            gunBarrel.LookAt (viewPoint.point);
        }
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

    /// <summary>
    /// Picking up objects instantiates a copy of the object, so this function fixes the fact that the copy will already be marked as linked
    /// This is called by VolumeItemPickup's OnAttemptItemPickup UnityEvent
    /// </summary>
    public void BreakRiftManagerLink()
    {
        isLinkedToManager = false;
    }

    /// <summary>
    /// A version of the GetIsValidTarget function that's setup to use the player's view point raycast as the point to check
    /// This is used by the placement indicator on the crosshair
    /// </summary>
    /// <returns>Returns true if the player is looking at a target they are allowed to shoot</returns>
    public bool GetIsValidTargetFromView()
    {
        Physics.Raycast(GetComponentInParent<Pawn>().viewPoint.position, GetComponentInParent<Pawn>().viewPoint.forward, out RaycastHit _hit, 255, layerMask);
        return GetIsValidTarget(_hit);
    }
    
    /// <summary>
    /// Used by the projectile markers to see if they are pointed at an object they can pin to
    /// </summary>
    /// <returns>Returns true if the gun is pointed at a target it's allowed to shoot</returns>
    public bool GetIsValidTarget(RaycastHit _hit)
    {
        // Gun is pointed at a bulb snapping point (That is valid!)
        // TODO - BulbCollisionBehaviour has not been ported!
        if (_hit.collider.gameObject.GetComponent<BulbCollisionBehaviour>() != null) return true;

        // Gun is pointed at a sliceable object
        if (_hit.collider.gameObject.TryGetComponent<CorGeo_SliceableMesh>(out _) is false) return false;
        // Non-mesh colliders don't support getting the polygon information, so we exit if it's not a mesh collider
        if (_hit.collider is not MeshCollider mCollider) return false;
        // Get if the raycast hit a polygon with a valid material to place markers on
        if (_hit.collider.gameObject.TryGetComponent(out Renderer rend) is false) return false;
        // Return true if allowMarkerPlacementAnywhere
        if (allowMarkerPlacementAnywhere) return true;
        
        // Gather information about the mesh
        Mesh colMesh = mCollider.sharedMesh;
        int triIndex = _hit.triangleIndex;
        int subMeshIndex = GetSubMeshIndex(colMesh, triIndex);

        return subMeshIndex == -1 || validPlacementMaterials.Contains(rend.sharedMaterials[subMeshIndex]);
    }
    
    /// <summary>
    /// Used by GetIsValidTarget to get the index of the tri that was hit on a mesh
    /// (So GetIsValidTarget can check for valid placement materials)
    /// </summary>
    [Todo("Can someone fact check me on this function's summary? ~Liz")]
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
