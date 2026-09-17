//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class OpeningSequence : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private bool completed;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public PlayableDirector playableDirector;
    public float introStartDelay = 0.1f;
    public bool enableIntroSequence;
    public string sceneToLoadOnCompletion;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void Update()
    {
        if (!enableIntroSequence && Input.GetKeyDown(KeyCode.Space))
        {
            playableDirector.Play();
        }
        if (enableIntroSequence && playableDirector.time >= playableDirector.duration-1 && !completed)
        {
            completed = true;
            var worldloader = GameInstance.Get<GI_WorldLoader>();
            worldloader.LoadWorld(sceneToLoadOnCompletion);
        }
    }

    public void Start()
    {
        if (!enableIntroSequence) return;
        StartCoroutine(StartSequence());
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private IEnumerator StartSequence()
    {
        yield return new WaitForSeconds(introStartDelay);
        playableDirector.Play();
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
