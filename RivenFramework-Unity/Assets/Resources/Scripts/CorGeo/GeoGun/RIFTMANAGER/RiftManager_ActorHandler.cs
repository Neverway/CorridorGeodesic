//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M., Connorses, Errynei, Soulex
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

/// <summary>
/// ?????
/// </summary>
[Serializable]
public class RiftManager_ActorHandler : ILoggable
{
    /// <summary>
    /// Class constructor
    /// </summary>
    public RiftManager_ActorHandler(RiftManager riftManager)
    {
        this.riftManager = riftManager;
        EnableRuntimeLogging = riftManager.EnableRuntimeLogging;
    }
    
    
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public bool EnableRuntimeLogging { get; set; }


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("Link to parent class for logging")]
    private RiftManager riftManager;
    public static List<CorGeo_Actor> CorGeo_Actors = new List<CorGeo_Actor> { };


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    [Todo("Not implemented", severity:TodoSeverity.Critical, Owner = "Liz-RiftManagerRevamp")]
    public void RestoreActors()
    {
        //throw new NotImplementedException();
    }

    #endregion
}
