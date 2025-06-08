//==========================================( Neverway 2025 )=========================================================//
// Author
//  Connorses
//
// Contributors
//  Liz M.
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Logic_DeploymentVent : Prop_Respawner
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private Transform animStartPos;
    [SerializeField] private Transform animEndPos;
    [SerializeField] private float animDuration;
    [SerializeField] private Ease animEaseCurve = Ease.InQuad;
    [SerializeField] private float spawnVelocity;

    [Tooltip("This is the object that will be used for the tween animation, before the real object is spawned in.")]
    [SerializeField] private GameObject animatedObject;

    [SerializeField] Animator anim;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    protected override IEnumerator SpawnWorker ()
    {
        yield return new WaitForSeconds (spawnDelay);
        anim.SetBool ("Powered", true);
        GameObject animObject = Instantiate(animatedObject, animEndPos);
        animObject.transform.position = animStartPos.position;
        animObject.transform.rotation = animEndPos.transform.rotation;
        animObject.transform.DOLocalMove (Vector3.zero, animDuration).SetEase (animEaseCurve)
            .OnComplete (() => {
                Destroy (animObject);
                spawnedObject = Instantiate (propPrefab, animEndPos.position, animEndPos.rotation);
                if (spawnedObject.TryGetComponent<Rigidbody> (out var rigidbody))
                {
                    rigidbody.velocity = animEndPos.forward * spawnVelocity;
                }
                if (autoRespawn == false)
                {
                    waitForRespawn = true;
                }
                anim.SetBool ("Powered", false);
            });
        yield return new WaitForSeconds (animDuration);
        spawnWorker = null;
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}