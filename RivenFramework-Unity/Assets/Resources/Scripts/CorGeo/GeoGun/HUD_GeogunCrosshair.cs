//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using RivenFramework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles, the showing and hiding, and the functionality of the geogun crosshair
/// </summary>
public class HUD_GeogunCrosshair : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    public bool hasInitializedCrosshairSine;

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GI_PawnManager pawnManager;
    private Item_Utility_Geogun geogun;
    [SerializeField] private Image AMarkerIndicator, BMarkerIndicator, PlacementIndicator, ASine, BSine;
    [SerializeField] private Color activeIndicator, inactiveIndicator;

    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void LateUpdate()
    {
        if (geogun == null)
        {
            FindReferences();
            return;
        }
        
        SetMarkerIndicators();
        SetPlacementIndicator();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void FindReferences()
    {
        // Exit if we have all reference
        if (pawnManager != null && geogun != null) return;
        
        // Get the pawn manager
        if (pawnManager == null)
        {
            pawnManager = GameInstance.Get<GI_PawnManager>();
            return;
        }
        
        // Get the geogun
        if (geogun == null)
        {
            var player = pawnManager.localPlayerCharacter;
            if (player != null)
            {
                geogun = player.GetComponentInChildren<Item_Utility_Geogun>();
                var crosshairObject = transform.GetChild(0).gameObject;
                crosshairObject.SetActive(geogun != null); // Disable the crosshair if the gun wasn't found
            }
            return;
        }
        transform.GetChild(0).gameObject.SetActive(true); // Enable the crosshair
    }

    private void SetMarkerIndicators()
    {
        switch (geogun.spawnedProjectiles.Count)
        {
            // Both Markers (Or too many markers (Which should never happen... riiiight?))
            case >= 2:
                AMarkerIndicator.color = activeIndicator;
                BMarkerIndicator.color = activeIndicator;
                InitializeCrosshairSine();
                break;
            // One Marker
            case 1:
                AMarkerIndicator.color = activeIndicator;
                BMarkerIndicator.color = inactiveIndicator;
                hasInitializedCrosshairSine = false;
                ASine.fillAmount = 0;
                BSine.fillAmount = 0;
                break;
            // No Markers
            default:
                AMarkerIndicator.color = inactiveIndicator;
                BMarkerIndicator.color = inactiveIndicator;
                hasInitializedCrosshairSine = false;
                ASine.fillAmount = 0;
                BSine.fillAmount = 0;
                break;
        }
    }

    private void SetPlacementIndicator()
    {
        var isValidTarget = geogun.GetIsValidTargetFromView();
        if (isValidTarget)
        {
            PlacementIndicator.color = activeIndicator;
        }
        else
        {
            PlacementIndicator.color = inactiveIndicator;
        }
    }
    
    private void InitializeCrosshairSine()
    {
        if (hasInitializedCrosshairSine) return;
        StartCoroutine(LerpSineFill());
    }
    
    IEnumerator LerpSineFill()
    {
        float time = 0;
        float duration = 0.5f;

        while (time < duration)
        {
            var charge = Mathf.Lerp(0, 1, time / duration);
            time += Time.deltaTime;
            ASine.fillAmount = charge;
            BSine.fillAmount = charge;
            hasInitializedCrosshairSine = true;
            yield return null;
        }
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}

// Find the local player
// Check their inventory for the gun
// ^ REPEAT UNTIL SUCCESS

// The gun was found
// Unhide the crosshair
// Highlight the center when pointed at valid target
// For each deployed marker, highlight the bars
// When both markers, lerp the sine