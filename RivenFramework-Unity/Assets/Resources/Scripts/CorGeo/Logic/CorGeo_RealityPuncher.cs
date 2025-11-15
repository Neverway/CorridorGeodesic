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

public class CorGeo_RealityPuncher : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public CorGeo_RealityPuncher linkedPuncher;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Volume bubbleVolume;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Swap()
    {
        StopAllCoroutines();
        linkedPuncher.GetComponent<Animator>().Play("On");
        StartCoroutine(CoSwap());
    }
    
    public IEnumerator CoSwap()
    {
        yield return new WaitForSeconds(4);
        var offset = linkedPuncher.transform.position-this.transform.position;
        
        // Move current bubble actors to other bubble
        foreach (var pawn in bubbleVolume.pawnsInTrigger)
        {
            pawn.transform.position += offset;
        }
        foreach (var prop in bubbleVolume.propsInTrigger)
        {
            prop.transform.position += offset;
        }
        
        // Move other bubble actors to current bubble
        foreach (var pawn in linkedPuncher.bubbleVolume.pawnsInTrigger)
        {
            pawn.transform.position -= offset;
        }
        foreach (var prop in linkedPuncher.bubbleVolume.propsInTrigger)
        {
            prop.transform.position -= offset;
        }
        
        
        // Swap bubble islands
        // ReSharper disable once SwapViaDeconstruction
        var currentPosition = gameObject.transform.position;
        gameObject.transform.position = linkedPuncher.transform.position;
        linkedPuncher.transform.position = currentPosition;
    }


    #endregion
}
