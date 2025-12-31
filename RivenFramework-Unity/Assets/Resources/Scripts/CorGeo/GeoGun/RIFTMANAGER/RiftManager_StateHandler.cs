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

[Serializable]
public class RiftManager_StateHandler : ILoggable
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public bool EnableRuntimeLogging { get; set; }


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Tooltip("Tells things what state the rift is changing from")]
    public static RiftState previousState = RiftState.None;
    [Tooltip("The current rift state  :O")]
    public static RiftState currentState = RiftState.None;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("Link to parent class for logging")]
    private RiftManager riftManager;
    
    [Tooltip("This event allows scripts to respond to any changes in the RiftState, such as the animated plane visuals or the rift audio effects")]
    public delegate void StateChanged ();
    public static event StateChanged OnStateChanged;


    #endregion

    // Class constructor
    public RiftManager_StateHandler(RiftManager riftManager)
    {
        this.riftManager = riftManager;
        EnableRuntimeLogging = riftManager.EnableRuntimeLogging;
    }

    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void SetState(RiftState _riftState)
    {
        if (currentState == _riftState)
        {
            return;
        }
        previousState = currentState;

        currentState = _riftState;

        OnStateChanged?.Invoke ();
    }


    #endregion
}
