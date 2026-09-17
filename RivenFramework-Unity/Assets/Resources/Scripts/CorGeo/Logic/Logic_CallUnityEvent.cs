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
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Disables or enables a target actor when powered
/// </summary>
public class Logic_CallUnityEvent : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public LogicInput<bool> input = new(true);

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public UnityEvent OnLogicPowered, OnLogicUnpowered;
    

    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        input.CallOnSourceChanged(Toggle);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void Toggle()
    {
        if (input.Get())
        {
            OnLogicPowered.Invoke();
        }
        else
        {
            OnLogicUnpowered.Invoke();
        }
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
