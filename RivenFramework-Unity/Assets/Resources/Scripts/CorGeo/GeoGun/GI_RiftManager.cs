//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//  Connorses, Errynei, Soulex
//
//====================================================================================================================//

using UnityEngine;

/// <summary>
/// Finds pinned markers, does rift stuff, referenced by gun script to control rift movements
/// </summary>
public class GI_RiftManager : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private GameObject cutPlanePrefab;
    private GameObject cutPlaneA, cutPlaneB;
    private Projectile_Marker markerA, markerB;
    

    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        // The objects used for the rift are created when the level loads
        // They are never destroyed, only hidden or unhidden
        // This is done this way to avoid lag spikes caused by having to constantly spawn and despawn the rift objects
        CreateCutPlanes();
        CreateSpaceContainers();
        SetRiftHidden(true);
    }

    private void Update()
    {
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Called in Start, Spawn the planes used for the cutting of the world as well as the visuals for previewing the cut
    /// </summary>
    private void CreateCutPlanes()
    {
        cutPlaneA = Instantiate(cutPlanePrefab, null);
        cutPlaneB = Instantiate(cutPlanePrefab, null);
        cutPlaneA.name = "CutPlaneA";
        cutPlaneB.name = "CutPlaneB";
    }    
    
    /// <summary>
    /// Called in Start, Spawn the empty game objects that represent the space that matter exists in while a rift is active
    /// These objects are used to scale and reposition all objects at once, according to the space they occupy
    /// </summary>
    private void CreateSpaceContainers()
    {
        var spaceContainer = new GameObject();
        spaceContainer.name = "ASpace";
        spaceContainer = new GameObject();
        spaceContainer.name = "BSpace";
        spaceContainer = new GameObject();
        spaceContainer.name = "NullSpace";
    }

    // Toggles the visibility of the cut planes, used for hiding/showing the rift objects when the rift is deactivated/activated
    private void SetRiftHidden(bool _hidden)
    {
        cutPlaneA.SetActive(_hidden);
        cutPlaneB.SetActive(_hidden);
    }
    
    private void GetPinnedMarkers()
    {
        
    }    

    private void PositionCutPlanes()
    {
        
    }

    private void SliceCutPlanes()
    {
        
    }

    private void UpdateMatterInSpaces()
    {
        
    }

    private void RestoreRift()
    {
        
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void AdjustRiftPosition(float _amount)
    {
        
    }


    #endregion
}